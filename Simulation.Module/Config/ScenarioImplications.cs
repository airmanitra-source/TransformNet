namespace Simulation.Module.Config;

/// <summary>
/// Résumé structuré des implications économiques attendues pour un scénario donné.
/// Affiché dans l'encadré bleu de l'interface utilisateur quand l'utilisateur
/// sélectionne un scénario prédéfini.
/// </summary>
public class ScenarioImplications
{
    /// <summary>
    /// Les 2-4 changements paramétriques clés par rapport au baseline.
    /// Ex : ["SMIG 200k → 300k MGA/mois", "Charges CNaPS entreprises ↑"]
    /// </summary>
    public string[] ChangementsClés { get; set; } = [];

    /// <summary>
    /// Canaux de transmission macroéconomiques activés par ce scénario.
    /// Ex : ["Consommation C ↑", "Pass-through salarial → inflation"]
    /// </summary>
    public string[] CanauxTransmission { get; set; } = [];

    /// <summary>
    /// Risques ou effets secondaires potentiels à surveiller.
    /// Ex : ["Inflation ~7.5%", "Licenciements informels"]
    /// </summary>
    public string[] RisquesAttendus { get; set; } = [];

    /// <summary>
    /// Horizon temporel recommandé pour observer les effets de ce scénario.
    /// Ex : "3 mois", "1 an", "5 ans"
    /// </summary>
    public string HorizonRecommande { get; set; } = "1 an";

    /// <summary>
    /// Catégorie du scénario pour le style visuel de l'encadré.
    /// Valeurs : "baseline" | "social" | "monetaire" | "fiscal" | "budgetaire" |
    ///           "sectoriel" | "choc" | "composite"
    /// </summary>
    public string Categorie { get; set; } = "baseline";

    /// <summary>
    /// Indique si ce scénario est vide (aucune implication définie).
    /// </summary>
    public bool EstVide =>
        ChangementsClés.Length == 0 &&
        CanauxTransmission.Length == 0 &&
        RisquesAttendus.Length == 0;
}
