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

    /// <summary>
    /// Simule une journée pour cette entreprise :
    /// 1. Production basée sur les employés (si jour ouvrable pour ce secteur)
    /// 2. Ventes B2C (demande des ménages) et B2B
    /// 3. Paiement des salaires (prorata journalier, uniquement jours travaillés)
    /// 4. Paiement de l'électricité Jirama (si connecté, tous les jours)
    /// 5. Paiement de l'IS sur le bénéfice
    /// 6. Collecte et reversement de la TVA
    ///
    /// Hypothèse jours ouvrables :
    /// - Si estJourOuvrable = false : pas de production, pas de ventes, pas de salaires
    /// - L'entreprise conserve sa trésorerie, pas de flux (sauf électricité)
    /// - Les coûts fixes (loyer, etc.) ne sont pas modélisés séparément
    /// </summary>
    public CompanyDailyResult SimulerJournee(
        double demandeConsommationMenages,
        double tauxIS,
        double tauxTVA,
        double tauxInflation,
        double tauxDirecteur,
        bool estJourOuvrable = true,
        Jirama? Jirama = null,
        double consoElecParEmployeKWhJour = 0,
        double tauxCNaPSPatronale = 0)
    {
        var result = new CompanyDailyResult();

        // Si l'entreprise ne travaille pas ce jour, seule l'électricité est facturée
        if (!estJourOuvrable)
        {
            // Électricité Jirama (facturée tous les jours, même hors jour ouvrable)
            double kwhJourRepos = consoElecParEmployeKWhJour * NombreEmployes * 0.3; // 30% en repos
            double facteurInflRepos = 1.0 + (tauxInflation / 365.0);
            double depElecRepos = Jirama != null ? kwhJourRepos * Jirama.PrixElectriciteArKWh * facteurInflRepos : 0;
            if (Jirama != null && kwhJourRepos > 0)
            {
                Jirama.EnregistrerPaiementEntreprise(depElecRepos, kwhJourRepos, tauxTVA);
            }
            result.DepensesElectricite = depElecRepos;
            Tresorerie -= depElecRepos;
            result.Tresorerie = Tresorerie;
            return result;
        }

        // 1. Capacité de production du jour
        double productionJour = NombreEmployes * ProductiviteParEmployeJour;

        // 2. Ventes
        // B2C : limitée par la demande des ménages et la capacité de production
        double capaciteB2C = productionJour * (1.0 - PartB2B);
        double ventesB2C = Math.Min(demandeConsommationMenages, capaciteB2C);

        // B2B : proportionnel à la production
        double ventesB2B = productionJour * PartB2B;
        double ventesTotales = ventesB2C + ventesB2B;

        result.VentesB2C = ventesB2C;
        result.VentesB2B = ventesB2B;
        ChiffreAffairesCumule += ventesTotales;

        // 3. TVA collectée sur les ventes (entreprises informelles exonérées)
        double tvaCollectee = EstInformel ? 0 : ventesTotales * (tauxTVA / (1.0 + tauxTVA));
        result.TVACollectee = tvaCollectee;
        TotalTVACollectee += tvaCollectee;

        // 4. Charges salariales journalières
        double chargesSalariales = (NombreEmployes * SalaireMoyenMensuel) / 30.0;
        result.ChargesSalariales = chargesSalariales;

        // 4b. Cotisations patronales CNaPS (entreprises formelles uniquement)
        double cotisationsCNaPS = EstInformel ? 0 : chargesSalariales * tauxCNaPSPatronale;
        result.CotisationsCNaPS = cotisationsCNaPS;
        TotalCotisationsCNaPS += cotisationsCNaPS;

        // 5. Coût des achats B2B (matières premières, services)
        double coutProduction = productionJour * (1.0 - MargeBeneficiaire) - chargesSalariales;
        coutProduction = Math.Max(0, coutProduction);
        result.AchatsB2B = coutProduction * PartB2B;
        TotalAchatsB2B += result.AchatsB2B;

        // 5b. Électricité Jirama (facturée tous les jours si connecté)
        double facteurInflationJour = 1.0 + (tauxInflation / 365.0);
        double kwhEntreprise = consoElecParEmployeKWhJour * NombreEmployes;
        double depensesElectricite = Jirama != null ? kwhEntreprise * Jirama.PrixElectriciteArKWh * facteurInflationJour : 0;
        if (Jirama != null && kwhEntreprise > 0)
        {
            Jirama.EnregistrerPaiementEntreprise(depensesElectricite, kwhEntreprise, tauxTVA);
        }
        result.DepensesElectricite = depensesElectricite;

        // 6. Bénéfice avant impôt
        double chargesTotales = chargesSalariales + cotisationsCNaPS + result.AchatsB2B + depensesElectricite;
        double beneficeAvantImpot = ventesTotales - tvaCollectee - chargesTotales;
        result.BeneficeAvantImpot = beneficeAvantImpot;

        ChargesCumulees += chargesTotales;

        // 7. Impôt sur les sociétés (IS) — entreprises informelles exonérées
        double impotIS = (!EstInformel && beneficeAvantImpot > 0) ? beneficeAvantImpot * tauxIS : 0;
        result.ImpotIS = impotIS;
        TotalImpotsSociete += impotIS;

        // 8. Valeur ajoutée = Production - Consommations intermédiaires (achats B2B + électricité)
        double valeurAjoutee = ventesTotales - result.AchatsB2B - depensesElectricite;
        result.ValeurAjoutee = valeurAjoutee;

        // 9. Mise à jour de la trésorerie
        double fluxNet = ventesTotales - tvaCollectee - chargesTotales - impotIS;
        Tresorerie += fluxNet;
        result.FluxNetJour = fluxNet;
        result.Tresorerie = Tresorerie;

        // 10. Effet du taux directeur sur le coût du crédit (simplifié)
        // Un taux directeur élevé augmente le coût de financement
        double coutFinancementJour = Math.Max(0, Tresorerie * -1) * (tauxDirecteur / 365.0);
        if (Tresorerie < 0)
        {
            Tresorerie -= coutFinancementJour;
            result.CoutFinancement = coutFinancementJour;
        }

        return result;
    }

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



