using Company.Module.Models;

namespace Government.Module.Models;

/// <summary>
/// Représente l'État malgache dans la simulation économique.
/// Collecte les impôts (IR, IS, TVA), définit la politique monétaire et fiscale.
/// Paramètres calibrés sur les réalités de Madagascar.
/// </summary>
public class Government
{
    // --- Politique fiscale ---
    /// <summary>
    /// Barème IRSA (Impôt sur les Revenus Salariaux et Assimilés) de Madagascar.
    /// Tranches progressives appliquées sur le salaire mensuel brut.
    /// Source : Code Général des Impôts de Madagascar.
    /// </summary>
    public List<TrancheIRSA> TranchesIRSA { get; set; } = TrancheIRSA.BaremeMadagascar();

    /// <summary>Taux d'impôt sur les sociétés (IS ~20%)</summary>
    public double TauxIS { get; set; } = 0.20;

    /// <summary>Taux de TVA (~20%)</summary>
    public double TauxTVA { get; set; } = 0.20;

    // --- Politique monétaire ---
    /// <summary>Taux directeur de la Banque Centrale de Madagascar (~9%)</summary>
    public double TauxDirecteur { get; set; } = 0.09;

    /// <summary>Taux d'inflation annuel (~8%)</summary>
    public double TauxInflation { get; set; } = 0.08;

    // --- Recettes et dépenses ---
    /// <summary>Total des recettes IRSA collectées</summary>
    public double TotalRecettesIR { get; set; }

    /// <summary>Total des recettes IS collectées</summary>
    public double TotalRecettesIS { get; set; }

    /// <summary>Total des recettes TVA collectées</summary>
    public double TotalRecettesTVA { get; set; }

    // --- Recettes douanières et commerce extérieur ---
    /// <summary>Total des droits de douane collectés</summary>
    public double TotalDroitsDouane { get; set; }

    /// <summary>Total des droits d'accise collectés</summary>
    public double TotalAccise { get; set; }

    /// <summary>Total de la TVA à l'importation collectée</summary>
    public double TotalTVAImport { get; set; }

    /// <summary>Total des taxes à l'exportation collectées</summary>
    public double TotalTaxeExport { get; set; }

    /// <summary>Total des redevances (import + export)</summary>
    public double TotalRedevances { get; set; }

    /// <summary>Total des recettes douanières (DD + Accise + TVA import + Taxe export + Redevances)</summary>
    public double TotalRecettesDouanieres => TotalDroitsDouane + TotalAccise + TotalTVAImport + TotalTaxeExport + TotalRedevances;

    /// <summary>Total des recettes fiscales (tout compris)</summary>
    public double TotalRecettesFiscales => TotalRecettesIR + TotalRecettesIS + TotalRecettesTVA + TotalRecettesDouanieres;

    /// <summary>Total de l'aide internationale reçue</summary>
    public double TotalAideInternationale { get; set; }

    /// <summary>Total des subventions versées à la Jirama</summary>
    public double TotalSubventionsJirama { get; set; }

    /// <summary>Total des cotisations CNaPS collectées</summary>
    public double TotalCotisationsCNaPS { get; set; }

    /// <summary>Dépenses publiques cumulées</summary>
    public double TotalDepensesPubliques { get; set; }

    /// <summary>Masse salariale des fonctionnaires cumulée</summary>
    public double TotalSalairesFonctionnaires { get; set; }

    /// <summary>Budget journalier de dépenses publiques (infrastructure, fonctionnaires, etc.) en MGA</summary>
    public double DepensesPubliquesJour { get; set; } = 500_000;

    /// <summary>Part des dépenses redistribuées aux ménages (transferts sociaux ~15%)</summary>
    public double TauxRedistribution { get; set; } = 0.15;

    /// <summary>Solde budgétaire (recettes - dépenses)</summary>
    public double SoldeBudgetaire { get; set; }

    /// <summary>Total des dépenses d'électricité Jirama de l'État (éclairage public + bâtiments)</summary>
    public double TotalDepensesElectricite { get; set; }

    /// <summary>Dette publique cumulée (initialisée par ScenarioConfig.DettePubliqueInitiale)</summary>
    public double DettePublique { get; set; }

    /// <summary>
    /// Consolide les résultats d'une journée de simulation.
    /// Collecte les impôts, effectue les dépenses publiques.
    /// </summary>
    public DailyGovernmentResult SimulerJournee(
        List<DailyHouseholdResult> resultsMenages,
        List<CompanyDailyResult> resultsEntreprises,
        List<DailyImporterResult> resultsImportateurs,
        List<DailyExporterResult> resultsExportateurs,
        Jirama? Jirama = null,
        double consoElecEtatKWhJour = 0,
        double aideInternationaleJour = 0,
        double subventionJiramaJour = 0,
        double masseSalarialeFonctionnairesJour = 0,
        double tauxReinvestissementPrive = 0,
        double depensesCapitalJour = 0,
        double interetsDetteJour = 0)
    {
        var result = new DailyGovernmentResult();

        // 1. Collecte IR (déjà prélevé sur les ménages)
        double irCollecte = resultsMenages.Sum(r => r.ImpotIR);
        TotalRecettesIR += irCollecte;
        result.RecettesIR = irCollecte;

        // 2. Collecte IS (toutes les entreprises : normales + importateurs + exportateurs)
        double isCollecte = resultsEntreprises.Sum(r => r.ImpotIS)
                          + resultsImportateurs.Sum(r => r.ImpotIS)
                          + resultsExportateurs.Sum(r => r.ImpotIS);
        TotalRecettesIS += isCollecte;
        result.RecettesIS = isCollecte;

        // 3. Collecte TVA (ventes locales)
        double tvaCollectee = resultsEntreprises.Sum(r => r.TVACollectee)
                            + resultsImportateurs.Sum(r => r.TVACollectee)
                            + resultsExportateurs.Sum(r => r.TVACollectee);

        // 3b. TVA Jirama (reversement de la TVA collectée sur factures eau + électricité)
        double tvaJirama = Jirama?.TVACollecteeJour ?? 0;
        tvaCollectee += tvaJirama;
        result.TVAJirama = tvaJirama;

        TotalRecettesTVA += tvaCollectee;
        result.RecettesTVA = tvaCollectee;

        // 4. Recettes douanières (importations)
        double droitsDouane = resultsImportateurs.Sum(r => r.DroitsDouane);
        double accise = resultsImportateurs.Sum(r => r.Accise);
        double tvaImport = resultsImportateurs.Sum(r => r.TVAImport);
        double redevancesImport = resultsImportateurs.Sum(r => r.RedevanceStatistique);
        TotalDroitsDouane += droitsDouane;
        TotalAccise += accise;
        TotalTVAImport += tvaImport;
        TotalRedevances += redevancesImport;
        result.DroitsDouane = droitsDouane;
        result.Accise = accise;
        result.TVAImport = tvaImport;

        // 5. Taxes à l'exportation
        double taxeExport = resultsExportateurs.Sum(r => r.TaxeExport);
        double redevancesExport = resultsExportateurs.Sum(r => r.RedevanceExport);
        TotalTaxeExport += taxeExport;
        TotalRedevances += redevancesExport;
        result.TaxeExport = taxeExport;

        result.RecettesDouanieres = droitsDouane + accise + tvaImport + redevancesImport + taxeExport + redevancesExport;

        // 6. Volumes commerce extérieur
        result.ImportationsCIF = resultsImportateurs.Sum(r => r.ValeurCIF);
        result.ExportationsFOB = resultsExportateurs.Sum(r => r.ValeurFOB);
        result.BalanceCommerciale = result.ExportationsFOB - result.ImportationsCIF;

        // 7. Recettes totales du jour (fiscales + aide internationale)
        double recettesJour = irCollecte + isCollecte + tvaCollectee + result.RecettesDouanieres;

        // 7b. Aide internationale
        TotalAideInternationale += aideInternationaleJour;
        result.AideInternationale = aideInternationaleJour;
        recettesJour += aideInternationaleJour;

        // 7c. Cotisations CNaPS collectées (toutes entreprises formelles)
        double cotisationsCNaPS = resultsEntreprises.Sum(r => r.CotisationsCNaPS)
                                + resultsImportateurs.Sum(r => r.CotisationsCNaPS)
                                + resultsExportateurs.Sum(r => r.CotisationsCNaPS);
        TotalCotisationsCNaPS += cotisationsCNaPS;
        result.CotisationsCNaPS = cotisationsCNaPS;
        recettesJour += cotisationsCNaPS;

        result.RecettesTotales = recettesJour;

        // 8. Dépenses publiques du jour
        double depenses = DepensesPubliquesJour;

        // 8b. Électricité Jirama de l'État (éclairage public + bâtiments publics)
        double facteurInflationJour = 1.0 + (TauxInflation / 365.0);
        double prixKWh = Jirama?.PrixElectriciteArKWh ?? 653;
        double depensesElecEtat = consoElecEtatKWhJour * prixKWh * facteurInflationJour;
        if (Jirama != null && consoElecEtatKWhJour > 0)
        {
            Jirama.EnregistrerPaiementEtat(depensesElecEtat, consoElecEtatKWhJour);
        }
        result.DepensesElectriciteEtat = depensesElecEtat;
        TotalDepensesElectricite += depensesElecEtat;
        depenses += depensesElecEtat;

        // 8c. Subventions à la Jirama
        result.SubventionsJirama = subventionJiramaJour;
        TotalSubventionsJirama += subventionJiramaJour;
        depenses += subventionJiramaJour;
        if (Jirama != null)
        {
            Jirama.Tresorerie += subventionJiramaJour;
        }

        // 8d. Masse salariale des fonctionnaires (calculée micro : somme des salaires des ménages fonctionnaires)
        result.SalairesFonctionnaires = masseSalarialeFonctionnairesJour;
        TotalSalairesFonctionnaires += masseSalarialeFonctionnairesJour;
        depenses += masseSalarialeFonctionnairesJour;

        // 8e. Intérêts de la dette publique (TOFE : dette extérieure + intérieure)
        result.InteretsDette = interetsDetteJour;
        depenses += interetsDetteJour;

        // 8f. Dépenses en capital (FBCF publique, TOFE : financement intérieur + extérieur)
        result.DepensesCapital = depensesCapitalJour;
        depenses += depensesCapitalJour;

        TotalDepensesPubliques += depenses;
        result.DepensesPubliques = depenses;

        // 9. FBCF (Formation Brute de Capital Fixe)
        // FBCF privée = part des bénéfices positifs réinvestie par les entreprises
        double profitPositifTotal = resultsEntreprises.Sum(r => Math.Max(0, r.BeneficeAvantImpot))
                                  + resultsImportateurs.Sum(r => Math.Max(0, r.BeneficeAvantImpot))
                                  + resultsExportateurs.Sum(r => Math.Max(0, r.BeneficeAvantImpot));
        double fbcfPrivee = profitPositifTotal * tauxReinvestissementPrive;

        // FBCF publique = dépenses en capital de l'État (source TOFE directe)
        result.FBCF = fbcfPrivee + depensesCapitalJour;

        // Consommation finale de l'État (G dans le PIB) = dépenses courantes - subventions (transferts)
        // G exclut : subventions (transferts), dépenses en capital (dans FBCF), intérêts (transfert au créancier)
        result.ConsommationFinaleEtat = depenses - subventionJiramaJour - depensesCapitalJour - interetsDetteJour;

        // 10. Transferts sociaux (redistribution)
        double transferts = DepensesPubliquesJour * TauxRedistribution;
        result.TransfertsSociaux = transferts;

        // 11. Solde budgétaire
        double soldeJour = recettesJour - depenses;
        SoldeBudgetaire += soldeJour;
        result.SoldeJour = soldeJour;
        result.SoldeCumule = SoldeBudgetaire;

        // 12. Dette publique (accumule les déficits)
        if (soldeJour < 0)
        {
            DettePublique += Math.Abs(soldeJour);
        }
        result.DettePublique = DettePublique;

        return result;
    }
}


