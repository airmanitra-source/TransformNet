using MachineLearning.Web.Models.Simulation.Companies;
using MachineLearning.Web.Models.Simulation.Household;

namespace MachineLearning.Web.Models.Simulation.Government;

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

    /// <summary>Dépenses publiques cumulées</summary>
    public double TotalDepensesPubliques { get; set; }

    /// <summary>Budget journalier de dépenses publiques (infrastructure, fonctionnaires, etc.) en MGA</summary>
    public double DepensesPubliquesJour { get; set; } = 500_000;

    /// <summary>Part des dépenses redistribuées aux ménages (transferts sociaux ~15%)</summary>
    public double TauxRedistribution { get; set; } = 0.15;

    /// <summary>Solde budgétaire (recettes - dépenses)</summary>
    public double SoldeBudgetaire { get; set; }

    /// <summary>Dette publique cumulée</summary>
    public double DettePublique { get; set; } = 10_000_000;

    /// <summary>
    /// Calcule l'IRSA mensuel selon le barème progressif malgache.
    /// L'impôt est calculé par tranches marginales.
    /// </summary>
    public double CalculerIRSAMensuel(double salaireMensuelBrut)
    {
        double impot = 0;
        double resteImposable = salaireMensuelBrut;

        for (int i = 0; i < TranchesIRSA.Count; i++)
        {
            var tranche = TranchesIRSA[i];
            double plafondTranche = (i + 1 < TranchesIRSA.Count)
                ? TranchesIRSA[i + 1].SeuilMin
                : double.MaxValue;

            double montantDansTranche = Math.Min(resteImposable, plafondTranche - tranche.SeuilMin);
            if (montantDansTranche <= 0) break;

            impot += montantDansTranche * tranche.Taux;
            resteImposable -= montantDansTranche;
        }

        // Minimum de perception IRSA : 2 000 MGA/mois si imposable
        if (impot > 0 && impot < 2_000 && salaireMensuelBrut > TranchesIRSA[0].SeuilMin)
            impot = 2_000;

        return impot;
    }

    /// <summary>
    /// Calcule l'IRSA journalier (prorata du mensuel).
    /// </summary>
    public double CalculerIRSAJournalier(double salaireMensuelBrut)
    {
        return CalculerIRSAMensuel(salaireMensuelBrut) / 30.0;
    }

    /// <summary>
    /// Taux effectif d'IRSA pour un salaire donné.
    /// </summary>
    public double TauxEffectifIRSA(double salaireMensuelBrut)
    {
        if (salaireMensuelBrut <= 0) return 0;
        return CalculerIRSAMensuel(salaireMensuelBrut) / salaireMensuelBrut;
    }

    /// <summary>
    /// Consolide les résultats d'une journée de simulation.
    /// Collecte les impôts, effectue les dépenses publiques.
    /// </summary>
    public DailyGovernmentResult SimulerJournee(
        List<DailyHouseholdResult> resultsMenages,
        List<DailyCompanyResult> resultsEntreprises,
        List<DailyImporterResult> resultsImportateurs,
        List<DailyExporterResult> resultsExportateurs)
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

        // 7. Recettes totales du jour
        double recettesJour = irCollecte + isCollecte + tvaCollectee + result.RecettesDouanieres;
        result.RecettesTotales = recettesJour;

        // 5. Dépenses publiques du jour
        double depenses = DepensesPubliquesJour;
        TotalDepensesPubliques += depenses;
        result.DepensesPubliques = depenses;

        // 6. Transferts sociaux (redistribution)
        double transferts = depenses * TauxRedistribution;
        result.TransfertsSociaux = transferts;

        // 7. Solde budgétaire
        double soldeJour = recettesJour - depenses;
        SoldeBudgetaire += soldeJour;
        result.SoldeJour = soldeJour;
        result.SoldeCumule = SoldeBudgetaire;

        // 8. Dette publique (augmente si déficit)
        if (soldeJour < 0)
        {
            DettePublique += Math.Abs(soldeJour);
            // Intérêts sur la dette (taux directeur)
            double interetsJour = DettePublique * (TauxDirecteur / 365.0);
            DettePublique += interetsJour;
            result.InteretsDette = interetsJour;
        }
        result.DettePublique = DettePublique;

        return result;
    }
}

public class DailyGovernmentResult
{
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double RecettesTotales { get; set; }
    public double DepensesPubliques { get; set; }
    public double TransfertsSociaux { get; set; }
    public double SoldeJour { get; set; }
    public double SoldeCumule { get; set; }
    public double DettePublique { get; set; }
    public double InteretsDette { get; set; }

    // --- Commerce extérieur ---
    public double DroitsDouane { get; set; }
    public double Accise { get; set; }
    public double TVAImport { get; set; }
    public double TaxeExport { get; set; }
    public double RecettesDouanieres { get; set; }
    public double ImportationsCIF { get; set; }
    public double ExportationsFOB { get; set; }
    public double BalanceCommerciale { get; set; }
}

/// <summary>
/// Tranche du barème IRSA (Impôt sur les Revenus Salariaux et Assimilés) de Madagascar.
/// Barème progressif par tranches marginales.
/// </summary>
public class TrancheIRSA
{
    /// <summary>Seuil minimum de la tranche (MGA/mois)</summary>
    public double SeuilMin { get; set; }

    /// <summary>Taux marginal de la tranche</summary>
    public double Taux { get; set; }

    /// <summary>Description de la tranche</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Barème IRSA officiel de Madagascar (Code Général des Impôts).
    /// Tranches mensuelles progressives.
    /// </summary>
    public static List<TrancheIRSA> BaremeMadagascar() =>
    [
        new() { SeuilMin = 0,       Taux = 0.00, Description = "Exonéré (≤ 350 000 MGA)" },
        new() { SeuilMin = 350_001, Taux = 0.05, Description = "5% (350 001 – 400 000)" },
        new() { SeuilMin = 400_001, Taux = 0.10, Description = "10% (400 001 – 500 000)" },
        new() { SeuilMin = 500_001, Taux = 0.15, Description = "15% (500 001 – 600 000)" },
        new() { SeuilMin = 600_001, Taux = 0.20, Description = "20% (> 600 000)" },
    ];
}
