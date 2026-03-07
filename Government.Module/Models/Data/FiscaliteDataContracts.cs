namespace Government.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres de fiscalité d'un scénario.</summary>
public interface IFiscaliteReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double TauxIS { get; }
    double TauxTVA { get; }
    double TauxDirecteur { get; }
    double TauxDroitsDouane { get; }
    double TauxAccise { get; }
    double TauxTaxeExport { get; }
    double TauxCotisationsPatronalesCNaPS { get; }
    double TauxCotisationsSalarialesCNaPS { get; }
    double IRSAMinimumPerception { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer la fiscalité.</summary>
public interface IFiscaliteWriteModel
{
    int ScenarioId { get; }
    double TauxIS { get; }
    double TauxTVA { get; }
    double TauxDirecteur { get; }
    double TauxDroitsDouane { get; }
    double TauxAccise { get; }
    double TauxTaxeExport { get; }
    double TauxCotisationsPatronalesCNaPS { get; }
    double TauxCotisationsSalarialesCNaPS { get; }
    double IRSAMinimumPerception { get; }
}

public record FiscaliteReadModel(
    int Id, int ScenarioId,
    double TauxIS, double TauxTVA, double TauxDirecteur,
    double TauxDroitsDouane, double TauxAccise, double TauxTaxeExport,
    double TauxCotisationsPatronalesCNaPS, double TauxCotisationsSalarialesCNaPS,
    double IRSAMinimumPerception, DateTime MisAJourAt) : IFiscaliteReadModel;

public record FiscaliteWriteModel(
    int ScenarioId,
    double TauxIS, double TauxTVA, double TauxDirecteur,
    double TauxDroitsDouane, double TauxAccise, double TauxTaxeExport,
    double TauxCotisationsPatronalesCNaPS, double TauxCotisationsSalarialesCNaPS,
    double IRSAMinimumPerception) : IFiscaliteWriteModel;
