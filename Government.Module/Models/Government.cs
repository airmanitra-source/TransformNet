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

}


