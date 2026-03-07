using Simulation.Infrastructure.Entities;

namespace Simulation.Infrastructure;

/// <summary>
/// Contrat unique du dépôt de scénarios : lecture + écriture de toutes
/// les tables de paramétrage du simulateur économique.
/// </summary>
public interface IScenarioRepository
{
    // ── Scénarios ─────────────────────────────────────────────────────
    Task<IEnumerable<ScenarioEntity>> GetAllScenariosAsync();
    Task<ScenarioEntity?> GetScenarioByIdAsync(int id);
    Task<int> CreateScenarioAsync(ScenarioEntity scenario);
    Task UpdateScenarioAsync(ScenarioEntity scenario);
    Task DeleteScenarioAsync(int id);

    // ── Macro ─────────────────────────────────────────────────────────
    Task<ParamMacroEntity?> GetParamMacroAsync(int scenarioId);
    Task UpsertParamMacroAsync(ParamMacroEntity entity);

    // ── Fiscalité ─────────────────────────────────────────────────────
    Task<ParamFiscaliteEntity?> GetParamFiscaliteAsync(int scenarioId);
    Task UpsertParamFiscaliteAsync(ParamFiscaliteEntity entity);

    // ── IRSA ──────────────────────────────────────────────────────────
    Task<IEnumerable<ParamIRSATrancheEntity>> GetParamIRSATranchesAsync(int scenarioId);
    Task ReplaceParamIRSATranchesAsync(int scenarioId, IEnumerable<ParamIRSATrancheEntity> tranches);

    // ── Distribution Salariale ────────────────────────────────────────
    Task<ParamDistributionSalarialeEntity?> GetParamDistributionSalarialeAsync(int scenarioId);
    Task UpsertParamDistributionSalarialeAsync(ParamDistributionSalarialeEntity entity);

    // ── Comportement Ménage ───────────────────────────────────────────
    Task<IEnumerable<ParamComportementMenageEntity>> GetParamComportementMenageAsync(int scenarioId);
    Task UpsertParamComportementMenageAsync(ParamComportementMenageEntity entity);

    // ── Banque ────────────────────────────────────────────────────────
    Task<ParamBanqueEntity?> GetParamBanqueAsync(int scenarioId);
    Task UpsertParamBanqueAsync(ParamBanqueEntity entity);

    // ── Prix ──────────────────────────────────────────────────────────
    Task<ParamPrixEntity?> GetParamPrixAsync(int scenarioId);
    Task UpsertParamPrixAsync(ParamPrixEntity entity);

    // ── Inflation ─────────────────────────────────────────────────────
    Task<ParamInflationEntity?> GetParamInflationAsync(int scenarioId);
    Task UpsertParamInflationAsync(ParamInflationEntity entity);

    // ── Taux de Change ────────────────────────────────────────────────
    Task<ParamTauxChangeEntity?> GetParamTauxChangeAsync(int scenarioId);
    Task UpsertParamTauxChangeAsync(ParamTauxChangeEntity entity);

    // ── Agriculture ───────────────────────────────────────────────────
    Task<ParamAgricultureEntity?> GetParamAgricultureAsync(int scenarioId);
    Task UpsertParamAgricultureAsync(ParamAgricultureEntity entity);

    // ── Cyclone ───────────────────────────────────────────────────────
    Task<ParamCycloneEntity?> GetParamCycloneAsync(int scenarioId);
    Task UpsertParamCycloneAsync(ParamCycloneEntity entity);

    // ── Transport ─────────────────────────────────────────────────────
    Task<ParamTransportEntity?> GetParamTransportAsync(int scenarioId);
    Task UpsertParamTransportAsync(ParamTransportEntity entity);

    // ── Santé ─────────────────────────────────────────────────────────
    Task<ParamSanteEntity?> GetParamSanteAsync(int scenarioId);
    Task UpsertParamSanteAsync(ParamSanteEntity entity);

    // ── Éducation ─────────────────────────────────────────────────────
    Task<ParamEducationEntity?> GetParamEducationAsync(int scenarioId);
    Task UpsertParamEducationAsync(ParamEducationEntity entity);

    // ── Loisirs ───────────────────────────────────────────────────────
    Task<IEnumerable<ParamLoisirsEntity>> GetParamLoisirsAsync(int scenarioId);
    Task UpsertParamLoisirsAsync(ParamLoisirsEntity entity);

    // ── Entreprises ───────────────────────────────────────────────────
    Task<ParamEntreprisesEntity?> GetParamEntreprisesAsync(int scenarioId);
    Task UpsertParamEntreprisesAsync(ParamEntreprisesEntity entity);

    // ── Secteurs d'Activité ───────────────────────────────────────────
    Task<IEnumerable<ParamSecteurActiviteEntity>> GetParamSecteursActiviteAsync(int scenarioId);
    Task UpsertParamSecteurActiviteAsync(ParamSecteurActiviteEntity entity);

    // ── JIRAMA ────────────────────────────────────────────────────────
    Task<ParamJiramaEntity?> GetParamJiramaAsync(int scenarioId);
    Task UpsertParamJiramaAsync(ParamJiramaEntity entity);

    // ── Commerce ──────────────────────────────────────────────────────
    Task<ParamCommerceEntity?> GetParamCommerceAsync(int scenarioId);
    Task UpsertParamCommerceAsync(ParamCommerceEntity entity);

    // ── Immobilier ────────────────────────────────────────────────────
    Task<ParamImmobilierEntity?> GetParamImmobilierAsync(int scenarioId);
    Task UpsertParamImmobilierAsync(ParamImmobilierEntity entity);
}
