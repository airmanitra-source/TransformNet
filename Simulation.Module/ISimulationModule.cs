using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Household.Module.Models;
using Simulation.Module.Config;
using Simulation.Module.Models;

namespace Simulation.Module
{
    public interface ISimulationModule
    {
        SimulationResult Result { get; }
        bool EnCours { get; }
        int JourCourant { get; }
        DistributionStats StatsInitiales { get; }

        event Action? OnTickCompleted;

        void Initialiser(ScenarioConfig config);
        Task DemarrerAsync();
        Task ExecuterRapideAsync(int delaiEntreJoursMs = 50);
        void Arreter();
        void SimulerUnJourSync();
        void SimulerNJoursSync(int nbJours);
    }
}
