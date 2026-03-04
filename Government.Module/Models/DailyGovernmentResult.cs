namespace Government.Module.Models;

public class DailyGovernmentResult
{
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    /// <summary>TVA reversée par la Jirama sur les factures eau + électricité</summary>
    public double TVAJirama { get; set; }
    /// <summary>Aide internationale reçue ce jour</summary>
    public double AideInternationale { get; set; }
    /// <summary>Cotisations CNaPS collectées ce jour (patronales)</summary>
    public double CotisationsCNaPS { get; set; }
    public double RecettesTotales { get; set; }
    public double DepensesPubliques { get; set; }
    /// <summary>Dépenses d'électricité Jirama de l'État ce jour (éclairage public + bâtiments)</summary>
    public double DepensesElectriciteEtat { get; set; }
    /// <summary>Subventions versées à la Jirama ce jour</summary>
    public double SubventionsJirama { get; set; }
    /// <summary>Masse salariale des fonctionnaires ce jour</summary>
    public double SalairesFonctionnaires { get; set; }
    /// <summary>FBCF estimée ce jour (Formation Brute de Capital Fixe)</summary>
    public double FBCF { get; set; }
    /// <summary>Dépenses en capital de l'État ce jour (TOFE : financement intérieur + extérieur)</summary>
    public double DepensesCapital { get; set; }
    /// <summary>
    /// Consommation finale de l'État (G dans le PIB par la demande).
    /// = Dépenses totales - Subventions (transferts) - FBCF publique.
    /// En SCN 2008, G exclut les transferts et l'investissement public (qui est dans FBCF).
    /// </summary>
    public double ConsommationFinaleEtat { get; set; }
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


