namespace Company.Module.Models;

/// <summary>
/// Représente une entreprise (société) malgache dans la simulation.
/// Vend des produits/services aux ménages (B2C) et entre entreprises (B2B).
/// </summary>
public class Company
{
    private static int _nextId = 1;

    public static void ResetIdCounter() => _nextId = 1;

    public int Id { get; } = _nextId++;
    public string Name { get; set; } = string.Empty;
    public ESecteurActivite SecteurActivite { get; set; } = ESecteurActivite.Services;

    /// <summary>
    /// Indique si l'entreprise est dans le secteur informel.
    /// Les entreprises informelles ne paient ni IS ni TVA.
    /// ~85% de l'économie malgache est informelle (INSTAT).
    /// </summary>
    public bool EstInformel { get; set; }

    /// <summary>
    /// Facteur de productivité informelle (0.3-0.6 du formel).
    /// Reflète l'absence de capital, de formation et de technologie.
    /// Source : INSTAT ENEMPSI — productivité informelle ≈ 30-60% du formel à secteur égal.
    /// Vaut 1.0 pour les entreprises formelles (aucune réduction).
    /// </summary>
    public double FacteurProductiviteInformel { get; set; } = 1.0;

    /// <summary>
    /// Productivité effective par employé par jour (MGA).
    /// = ProductiviteParEmployeJour × FacteurProductiviteInformel.
    /// </summary>
    public double ProductiviteEffectiveParEmployeJour =>
        ProductiviteParEmployeJour * FacteurProductiviteInformel;

    // --- Finances (en Ariary - MGA) ---
    /// <summary>Trésorerie disponible</summary>
    public double Tresorerie { get; set; } = 5_000_000;

    /// <summary>Chiffre d'affaires cumulé</summary>
    public double ChiffreAffairesCumule { get; set; }

    /// <summary>Charges cumulées (salaires + achats B2B + taxes)</summary>
    public double ChargesCumulees { get; set; }

    // --- Production ---
    /// <summary>Nombre d'employés</summary>
    public int NombreEmployes { get; set; } = 10;

    /// <summary>Salaire mensuel moyen versé par employé (MGA)</summary>
    public double SalaireMoyenMensuel { get; set; } = 200_000;

    /// <summary>Productivité par employé par jour (valeur produite en MGA)</summary>
    public double ProductiviteParEmployeJour { get; set; } = 15_000;

    /// <summary>Marge bénéficiaire sur les ventes (~20%)</summary>
    public double MargeBeneficiaire { get; set; } = 0.20;

    // --- Ventes B2B ---
    /// <summary>Part du chiffre d'affaires provenant du B2B (~30%)</summary>
    public double PartB2B { get; set; } = 0.30;

    /// <summary>Total des achats B2B effectués (charges)</summary>
    public double TotalAchatsB2B { get; set; }

    // --- Impôts ---
    /// <summary>Total impôt sur les sociétés payé (IS)</summary>
    public double TotalImpotsSociete { get; set; }

    /// <summary>Total TVA collectée et reversée</summary>
    public double TotalTVACollectee { get; set; }

    /// <summary>Total des cotisations CNaPS patronales payées</summary>
    public double TotalCotisationsCNaPS { get; set; }

    // ═══════════════════════════════════════════
    //  DYNAMIQUE DU MARCHÉ DU TRAVAIL
    // ═══════════════════════════════════════════

    /// <summary>
    /// Nombre de jours consécutifs avec trésorerie négative (stress financier).
    /// Déclenche des licenciements si dépasse le seuil configuré.
    /// Reset à 0 dès que la trésorerie repasse en positif.
    /// </summary>
    public int JoursStressTresorerieConsecutifs { get; set; }

    /// <summary>
    /// Nombre de jours consécutifs où la demande excède la capacité de production.
    /// Déclenche des embauches si dépasse le seuil configuré.
    /// Reset à 0 dès que la demande repasse sous la capacité.
    /// </summary>
    public int JoursDemandeExcedentaireConsecutifs { get; set; }

    /// <summary>
    /// Demande moyenne lissée sur les derniers jours (MGA).
    /// Utilisée pour lisser les décisions d'embauche et éviter les oscillations.
    /// </summary>
    public double DemandeLissee { get; set; }

    /// <summary>
    /// Nombre minimum d'employés (plancher). L'entreprise ne peut pas descendre
    /// en dessous (au moins 1 pour exister).
    /// </summary>
    public int NombreEmployesMinimum { get; set; } = 1;

    /// <summary>Total cumulé des embauches sur la simulation.</summary>
    public int TotalEmbauches { get; set; }

    /// <summary>Total cumulé des licenciements sur la simulation.</summary>
    public int TotalLicenciements { get; set; }

    /// <summary>
    /// Retourne la productivité journalière réaliste par employé selon le secteur.
    /// Basé sur des données terrain malgaches (CA/employé/jour).
    /// </summary>
    /// <remarks>
    /// SUPERSEDED : cette méthode statique est conservée pour compatibilité.
    /// Privilégier <c>ICompanyModule.GetProductiviteParSecteur()</c> injecté dans
    /// <c>EconomicSimulatorViewModel</c> pour permettre la substitution et les tests.
    /// </remarks>
    public static double GetProductiviteParSecteur(ESecteurActivite secteur, bool moyenneBasse = false)
    {
        return secteur switch
        {
            // Agriculture : ~4 500 Mds MGA/an VA, ~6M travailleurs → ~2-3k MGA/travailleur/jour
            // Faible productivité, surtout subsistance
            ESecteurActivite.Agriculture => moyenneBasse ? 2_000 : 3_500,

            // Textile Export : 500 employés → 5-10 Mds/an → 27-55k MGA/employé/jour
            ESecteurActivite.Textiles => moyenneBasse ? 27_000 : 41_000,

            // Commerce/Services : 15 employés → 400-600M/an → 73-110k MGA/employé/jour
            ESecteurActivite.Commerces => moyenneBasse ? 73_000 : 91_000,
            ESecteurActivite.Services => moyenneBasse ? 73_000 : 91_000,

            // Minier : 300 employés → 30+ Mds/an → 274k MGA/employé/jour
            ESecteurActivite.SecteurMinier => moyenneBasse ? 200_000 : 274_000,

            // Construction/BTP : productivité intermédiaire ~15-25k MGA/employé/jour
            ESecteurActivite.Construction => moyenneBasse ? 15_000 : 25_000,

            _ => 50_000  // Défaut pour autres secteurs
        };
    }

    /// <summary>
    /// Retourne la trésorerie initiale par secteur (en MGA).
    /// Utilise le dictionnaire configurable si fourni, sinon les valeurs par défaut.
    /// Calibré sur les réalités malgaches : les entreprises agricoles informelles
    /// disposent de très peu de fonds de roulement, tandis que les entreprises minières
    /// ont une trésorerie conséquente pour financer leurs opérations.
    /// </summary>
    /// <remarks>
    /// SUPERSEDED : cette méthode statique est conservée pour compatibilité.
    /// Privilégier <c>EconomicSimulatorViewModel.GetTresorerieInitiale()</c> qui combine
    /// le dictionnaire de scénario config et <c>ICompanyModule.GetTresorerieInitialeParSecteur()</c>.
    /// </remarks>
    public static double GetTresorerieInitialeParSecteur(
        ESecteurActivite secteur,
        Dictionary<ESecteurActivite, double>? configParSecteur = null)
    {
        if (configParSecteur != null && configParSecteur.TryGetValue(secteur, out double valeur))
            return valeur;

        return secteur switch
        {
            ESecteurActivite.Agriculture => 500_000,
            ESecteurActivite.Textiles => 4_000_000,
            ESecteurActivite.Commerces => 8_000_000,
            ESecteurActivite.Services => 4_000_000,
            ESecteurActivite.SecteurMinier => 50_000_000,
            ESecteurActivite.Construction => 7_000_000,
            _ => 5_000_000
        };
    }

    /// <summary>
    /// Calcule le CA annuel estimé à partir du CA cumulé et du nombre de jours simulés.
    /// </summary>
    public double CAEstimeAnnuel(int joursSimules) => 
        joursSimules > 0 ? (ChiffreAffairesCumule / joursSimules) * 365.0 : 0;

    /// <summary>
    /// Vérifie si l'entreprise dépasse le seuil légal de TVA (400M MGA/an).
    /// </summary>
    public bool DoitCollecterTVA(int joursSimules) => 
        CAEstimeAnnuel(joursSimules) > 400_000_000;

    /// <summary>
    /// Valide la cohérence du CA par rapport aux ratios réalistes du secteur.
    /// Retourne le ratio (CA réel / CA attendu). 
    /// Valeur proche de 1.0 = cohérent, < 0.5 ou > 2.0 = incohérent.
    /// </summary>
    public double VerifierCoherenceCA(int joursSimules)
    {
        if (joursSimules == 0 || NombreEmployes == 0) return 1.0;

        double caReel = CAEstimeAnnuel(joursSimules);
        double caAttendu = GetProductiviteParSecteur(SecteurActivite, moyenneBasse: true) 
                           * NombreEmployes * 365.0;

        return caAttendu > 0 ? caReel / caAttendu : 1.0;
    }
}



