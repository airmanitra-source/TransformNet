namespace Simulation.Module.Models;

public class DailySnapshot
{
    public int Jour { get; set; }
    public int JourSemaine { get; set; }
    public string NomJour { get; set; } = string.Empty;
    public bool EstJourOuvrable { get; set; }
    public double EpargneMoyenneMenages { get; set; }
    public double EpargneTotaleMenages { get; set; }
    public double ConsommationTotaleMenages { get; set; }
    public double RevenuTotalMenages { get; set; }
    public double TauxEmploi { get; set; }
    public double RemittancesTotales { get; set; }
    public double LoyersImputesTotaux { get; set; }
    public double CotisationsCNaPSSalariales { get; set; }
    public double DepensesRizTotales { get; set; }
    public double RecettesEauJirama { get; set; }
    public double RecettesElectriciteJirama { get; set; }
    public double RecettesTotalesJirama { get; set; }
    public int NbAbonnesEau { get; set; }
    public int NbAbonnesElectricite { get; set; }
    public double ConsommationMenagesKWh { get; set; }
    public double ConsommationEntreprisesKWh { get; set; }
    public double ConsommationEtatKWh { get; set; }
    public double ProductionElecKWh { get; set; }
    public double PrixElectriciteArKWh { get; set; }
    public double FactureElecMoyenneParMenage { get; set; }
    public double TVAJiramaJour { get; set; }
    public double DepensesTransportJirama { get; set; }
    public double ValeurAjouteeJirama { get; set; }
    public double ChiffreAffairesTotalEntreprises { get; set; }
    public double TresorerieMoyenneEntreprises { get; set; }
    public double BeneficeTotalEntreprises { get; set; }
    public double VentesB2CTotales { get; set; }
    public double VentesB2BTotales { get; set; }
    public double DepensesElectriciteEntreprises { get; set; }
    public double CotisationsCNaPSPatronales { get; set; }
    public int NbEntreprisesInformelles { get; set; }
    public double RecettesFiscalesTotales { get; set; }
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double DepensesPubliques { get; set; }
    public double DepensesElectriciteEtat { get; set; }
    public double AideInternationale { get; set; }
    public double SubventionsJirama { get; set; }
    public double SalairesFonctionnaires { get; set; }
    public int NbFonctionnaires { get; set; }
    public double SoldeBudgetaire { get; set; }
    public double DettePublique { get; set; }
    public double PIBProxy { get; set; }
    public double FBCF { get; set; }
    public double VariationStocksImportateurs { get; set; }
    public double PIBParValeurAjoutee { get; set; }
    public double PIBParRevenus { get; set; }
    public double ChargesSalarialesTotalesEntreprises { get; set; }
    public double CotisationsCNaPSPatronalesTotales { get; set; }
    public double ExcedentBrutExploitation { get; set; }
    public double ValeurAjouteeAdminPublique { get; set; }
    public double ImportationsCIF { get; set; }
    public double ExportationsFOB { get; set; }
    public double BalanceCommerciale { get; set; }
    public double RecettesDouanieres { get; set; }
    public double DroitsDouaneJour { get; set; }
    public double AcciseJour { get; set; }
    public int NbImportateurs { get; set; }
    public int NbExportateurs { get; set; }
    public double Gini { get; set; }
    public double RatioD9D1 { get; set; }
    public double EpargneQ1 { get; set; }
    public double EpargneQ2 { get; set; }
    public double EpargneQ3 { get; set; }
    public double EpargneQ4 { get; set; }
    public double EpargneQ5 { get; set; }
    public int NbSubsistance { get; set; }
    public int NbInformelBas { get; set; }
    public int NbFormelBas { get; set; }
    public int NbFormelQualifie { get; set; }
    public int NbCadre { get; set; }
    public int NbEntreprisesAssujettiesTVA { get; set; }
    public double PourcentageCoherenceCA { get; set; }
    public double CAMoyenParEmploye { get; set; }
    public double CAMoyenParEmployeTextile { get; set; }
    public double CAMoyenParEmployeCommerce { get; set; }
    public double CAMoyenParEmployeMinier { get; set; }

    // --- Secteur Bancaire et Masse Monétaire ---
    public double MasseMonetaireM3 { get; set; }
    public double TotalDepotsBancaires { get; set; }
    public double TotalCreditsAccordes { get; set; }

    // ─── Achat alimentaire journalier (IHouseholdModule.AcheteProduitsAlimentaires) ───

    /// <summary>Somme des paniers alimentaires journaliers de tous les ménages (MGA).</summary>
    public double DepensesAlimentairesTotales { get; set; }

    /// <summary>Part informelle des achats alimentaires (85 %, marchés de quartier, MGA).</summary>
    public double DepensesAlimentairesInformelTotal { get; set; }

    /// <summary>Part formelle des achats alimentaires (15 % + TVA 20 %, épiceries, MGA).</summary>
    public double DepensesAlimentairesFormelTotal { get; set; }

    /// <summary>
    /// Facteur de réduction moyen des quantités alimentaires (0.30–1.00).
    /// 1.00 = aucune privation | 0.30 = plancher biologique.
    /// </summary>
    public double FacteurReductionAlimentaireMoyen { get; set; }

    // ─── Loisirs et vacances ──────────────────────────────────────────────────

    /// <summary>Total des dépenses de loisirs (sorties + vacances) tous ménages (MGA).</summary>
    public double DepensesLoisirsTotales { get; set; }

    /// <summary>Nombre de ménages en sortie weekend ce jour.</summary>
    public int NbMenagesEnSortie { get; set; }

    /// <summary>Nombre de ménages en vacances ce jour.</summary>
    public int NbMenagesEnVacances { get; set; }

    /// <summary>
    /// Facteur de réduction moyen des loisirs dû à la hausse des prix (0-1).
    /// 1.0 = aucune réduction | 0.0 = loisirs totalement supprimés.
    /// </summary>
    public double FacteurReductionLoisirsMoyen { get; set; }

    // ─── Transport (ITransportationModule) ──────────────────────────────────────

    /// <summary>Total des dépenses de transport tous ménages (MGA).</summary>
    public double DepensesTransportTotales { get; set; }

    /// <summary>Part des dépenses de transport routée vers le secteur informel (taxi-be, garagistes, MGA).</summary>
    public double DepensesTransportInformel { get; set; }

    /// <summary>Part des dépenses de transport routée vers le secteur formel (bus, stations-service, MGA).</summary>
    public double DepensesTransportFormel { get; set; }

    /// <summary>Part du transport liée au carburant (moto + voiture, alimente les importateurs, MGA).</summary>
    public double DepensesTransportCarburant { get; set; }

    // ─── Tourisme & Emploi formel (recalibration mensuelle) ─────────────────────

    /// <summary>
    /// CA cumulé des entreprises HôtellerieTourisme (proxy des recettes touristiques, MGA).
    /// Utilisé comme comparable aux données BCM "apport en devises des visiteurs non-résidents".
    /// </summary>
    public double RecettesTourismeCumulees { get; set; }

    /// <summary>
    /// Nombre total de salariés dans les entreprises formelles (non-informelles).
    /// Proxy du stock d'affiliés CNaPS dans la simulation.
    /// </summary>
    public int NbSalariesSecteurFormel { get; set; }

    /// <summary>
    /// Nombre d'entreprises dans le secteur HôtellerieTourisme.
    /// </summary>
    public int NbEntreprisesTourisme { get; set; }
}


