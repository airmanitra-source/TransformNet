namespace Company.Module.Models;

/// <summary>
/// Résultat journalier d'un exportateur.
/// </summary>
public class DailyExporterResult : CompanyDailyResult
{
    public double ValeurFOB { get; set; }
    public double TaxeExport { get; set; }
    public double RedevanceExport { get; set; }
    public double DevisesRapatriees { get; set; }

    /// <summary>Total taxes export générées ce jour</summary>
    public double RecettesFiscalesExport => TaxeExport + RedevanceExport;
}



