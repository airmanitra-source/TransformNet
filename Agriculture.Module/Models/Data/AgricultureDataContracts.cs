namespace Agriculture.Module.Models.Data;

/// <summary>Contrat de lecture des paramètres agricoles (sécheresse, autoconsommation).</summary>
public interface IAgricultureReadModel
{
    int Id { get; }
    int ScenarioId { get; }
    double ProbabiliteSecheresseJourSaison { get; }
    double PartMenagesRurauxAffectes { get; }
    int DureeSecheresseJoursBase { get; }
    double ReductionProductionAgricole { get; }
    double AideAlimentaireJourParMenage { get; }
    double ProbabiliteMigrationSaison { get; }
    double ValeurAutoconsommationJourBase { get; }
    DateTime MisAJourAt { get; }
}

/// <summary>Contrat d'écriture pour configurer les paramètres agricoles.</summary>
public interface IAgricultureWriteModel
{
    int ScenarioId { get; }
    double ProbabiliteSecheresseJourSaison { get; }
    double PartMenagesRurauxAffectes { get; }
    int DureeSecheresseJoursBase { get; }
    double ReductionProductionAgricole { get; }
    double AideAlimentaireJourParMenage { get; }
    double ProbabiliteMigrationSaison { get; }
    double ValeurAutoconsommationJourBase { get; }
}

public record AgricultureReadModel(
    int Id, int ScenarioId,
    double ProbabiliteSecheresseJourSaison, double PartMenagesRurauxAffectes,
    int DureeSecheresseJoursBase, double ReductionProductionAgricole,
    double AideAlimentaireJourParMenage, double ProbabiliteMigrationSaison,
    double ValeurAutoconsommationJourBase,
    DateTime MisAJourAt) : IAgricultureReadModel;

public record AgricultureWriteModel(
    int ScenarioId,
    double ProbabiliteSecheresseJourSaison, double PartMenagesRurauxAffectes,
    int DureeSecheresseJoursBase, double ReductionProductionAgricole,
    double AideAlimentaireJourParMenage, double ProbabiliteMigrationSaison,
    double ValeurAutoconsommationJourBase) : IAgricultureWriteModel;
