using Household.Module.Models;
using Simulation.Module.Config;
using Simulation.Module.Models;
using Simulation.Module.Models.Data;

namespace Simulation.Module
{
    public interface ISimulationModule
    {
        SimulationResult Result { get; }
        bool EnCours { get; }
        int JourCourant { get; }
        DistributionStats StatsInitiales { get; }

        event Action? OnTickCompleted;

        /// <summary>Retourne la liste des scénarios disponibles en base de données.</summary>
        Task<IEnumerable<IScenarioReadModel>> ListScenariosAsync();

        /// <summary>Charge le ScenarioConfig complet pour un scénario donné depuis la base de données.</summary>
        Task<ScenarioConfig> ChargerScenarioAsync(int scenarioId);

        void Initialiser(ScenarioConfig config);
        Task DemarrerAsync();
        Task ExecuterRapideAsync(int delaiEntreJoursMs = 50);
        void Arreter();
        void SimulerUnJourSync();
        void SimulerNJoursSync(int nbJours);
        /// <summary>
        /// Applique les valeurs de propension marginale à consommer par classe
        /// aux ménages déjà initialisés dans la simulation.
        /// Utile pour mettre à jour les ménages en cours d'exécution lorsque
        /// l'utilisateur modifie les sliders dans l'UI.
        /// </summary>
        void AppliquerPropensionConsommationParClasse(ScenarioConfig config);

        /// <summary>
        /// Valide les résultats simulés contre les données macro de référence.
        /// Compare 16+ indicateurs (PIB, commerce, fiscal, monétaire, inégalités, énergie)
        /// et retourne un rapport avec scoring (0-100), alertes et recommandations.
        /// Peut être appelé à tout moment (pendant ou après la simulation).
        /// </summary>
        MacroValidationReport ValiderMacro();
    }
}
