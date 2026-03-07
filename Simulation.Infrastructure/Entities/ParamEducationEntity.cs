namespace Simulation.Infrastructure.Entities;

/// <summary>Paramètres du module dépenses d'éducation.</summary>
public class ParamEducationEntity
{
    public int Id { get; set; }
    public int ScenarioId { get; set; }

    public double NombreEnfantsMoyenParMenage { get; set; } = 2.3;
    public double PartEnfantsScolarises { get; set; } = 0.72;
    public int DureeDepenseEducationJours { get; set; } = 180;
    public double CoutEducationJournalierParEnfant { get; set; } = 900;
    public double PartFormelleDepenseEducation { get; set; } = 0.75;

    public DateTime MisAJourAt { get; set; }
}
