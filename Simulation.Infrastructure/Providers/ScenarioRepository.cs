using Dapper;
using Simulation.Infrastructure.Entities;

namespace Simulation.Infrastructure.Providers;

/// <summary>
/// Implémentation Dapper du dépôt de scénarios.
/// Toutes les requêtes utilisent des paramètres nommés pour éviter les injections SQL.
/// Le pattern UPSERT (MERGE) garantit l'idempotence des sauvegardes.
/// </summary>
public sealed class ScenarioRepository : IScenarioRepository
{
    private readonly IDbConnectionFactory _factory;

    public ScenarioRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    // ════════════════════════════════════════════════════════════════
    //  SCÉNARIOS
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<ScenarioEntity>> GetAllScenariosAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ScenarioEntity>(
            "SELECT * FROM simulation.sim.[Scenarios] WHERE [EstActif] = 1 ORDER BY [EstBase] DESC, [Id]");
    }

    public async Task<ScenarioEntity?> GetScenarioByIdAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ScenarioEntity>(
            "SELECT * FROM simulation.sim.[Scenarios] WHERE [Id] = @Id", new { Id = id });
    }

    public async Task<int> CreateScenarioAsync(ScenarioEntity scenario)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO simulation.sim.[Scenarios] ([Nom],[Description],[Version],[EstBase],[EstActif],[CreePar])
            VALUES (@Nom,@Description,@Version,@EstBase,@EstActif,@CreePar);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return await conn.ExecuteScalarAsync<int>(sql, scenario);
    }

    public async Task UpdateScenarioAsync(ScenarioEntity scenario)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE simulation.sim.[Scenarios]
            SET [Nom]=@Nom,[Description]=@Description,[Version]=@Version,
                [EstBase]=@EstBase,[EstActif]=@EstActif,[MisAJourAt]=SYSUTCDATETIME()
            WHERE [Id]=@Id";
        await conn.ExecuteAsync(sql, scenario);
    }

    public async Task DeleteScenarioAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE simulation.sim.[Scenarios] SET [EstActif]=0,[MisAJourAt]=SYSUTCDATETIME() WHERE [Id]=@Id",
            new { Id = id });
    }

    // ════════════════════════════════════════════════════════════════
    //  PARAM MACRO
    // ════════════════════════════════════════════════════════════════

    public async Task<ParamMacroEntity?> GetParamMacroAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamMacroEntity>(
            "SELECT * FROM simulation.sim.[ParamMacro] WHERE [ScenarioId]=@ScenarioId",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamMacroAsync(ParamMacroEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamMacro] AS T
            USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [DureeJours]=@DureeJours,[NombreMenages]=@NombreMenages,
                [NombreEntreprises]=@NombreEntreprises,[PartMenagesUrbains]=@PartMenagesUrbains,
                [AideInternationaleJour]=@AideInternationaleJour,[SubventionJiramaJour]=@SubventionJiramaJour,
                [DepensesCapitalJour]=@DepensesCapitalJour,[InteretsDetteJour]=@InteretsDetteJour,
                [DettePubliqueInitiale]=@DettePubliqueInitiale,[DepensesPubliquesJour]=@DepensesPubliquesJour,
                [TauxRedistribution]=@TauxRedistribution,[NombreFonctionnaires]=@NombreFonctionnaires,
                [SalaireMoyenFonctionnaireMensuel]=@SalaireMoyenFonctionnaireMensuel,
                [TauxReinvestissementPrive]=@TauxReinvestissementPrive,
                [InvestissementProductifActive]=@InvestissementProductifActive,
                [TauxDepreciationCapitalAnnuel]=@TauxDepreciationCapitalAnnuel,
                [SeuilUtilisationInvestissement]=@SeuilUtilisationInvestissement,
                [ElasticiteCapitalProductivite]=@ElasticiteCapitalProductivite,
                [InputOutputActivee]=@InputOutputActivee,
                [ValeurAutoconsommationJourBase]=@ValeurAutoconsommationJourBase,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[DureeJours],[NombreMenages],[NombreEntreprises],[PartMenagesUrbains],
                 [AideInternationaleJour],[SubventionJiramaJour],[DepensesCapitalJour],
                 [InteretsDetteJour],[DettePubliqueInitiale],[DepensesPubliquesJour],[TauxRedistribution],
                 [NombreFonctionnaires],[SalaireMoyenFonctionnaireMensuel],[TauxReinvestissementPrive],
                 [InvestissementProductifActive],[TauxDepreciationCapitalAnnuel],
                 [SeuilUtilisationInvestissement],[ElasticiteCapitalProductivite],
                 [InputOutputActivee],[ValeurAutoconsommationJourBase])
            VALUES
                (@ScenarioId,@DureeJours,@NombreMenages,@NombreEntreprises,@PartMenagesUrbains,
                 @AideInternationaleJour,@SubventionJiramaJour,@DepensesCapitalJour,
                 @InteretsDetteJour,@DettePubliqueInitiale,@DepensesPubliquesJour,@TauxRedistribution,
                 @NombreFonctionnaires,@SalaireMoyenFonctionnaireMensuel,@TauxReinvestissementPrive,
                 @InvestissementProductifActive,@TauxDepreciationCapitalAnnuel,
                 @SeuilUtilisationInvestissement,@ElasticiteCapitalProductivite,
                 @InputOutputActivee,@ValeurAutoconsommationJourBase);";
        await conn.ExecuteAsync(sql, e);
    }

    // ════════════════════════════════════════════════════════════════
    //  PARAM FISCALITE
    // ════════════════════════════════════════════════════════════════

    public async Task<ParamFiscaliteEntity?> GetParamFiscaliteAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamFiscaliteEntity>(
            "SELECT * FROM simulation.sim.[ParamFiscalite] WHERE [ScenarioId]=@ScenarioId",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamFiscaliteAsync(ParamFiscaliteEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamFiscalite] AS T
            USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [TauxIS]=@TauxIS,[TauxTVA]=@TauxTVA,[TauxDirecteur]=@TauxDirecteur,
                [TauxDroitsDouane]=@TauxDroitsDouane,[TauxAccise]=@TauxAccise,
                [TauxTaxeExport]=@TauxTaxeExport,
                [TauxCotisationsPatronalesCNaPS]=@TauxCotisationsPatronalesCNaPS,
                [TauxCotisationsSalarialesCNaPS]=@TauxCotisationsSalarialesCNaPS,
                [IRSAMinimumPerception]=@IRSAMinimumPerception,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[TauxIS],[TauxTVA],[TauxDirecteur],[TauxDroitsDouane],[TauxAccise],
                 [TauxTaxeExport],[TauxCotisationsPatronalesCNaPS],[TauxCotisationsSalarialesCNaPS],
                 [IRSAMinimumPerception])
            VALUES
                (@ScenarioId,@TauxIS,@TauxTVA,@TauxDirecteur,@TauxDroitsDouane,@TauxAccise,
                 @TauxTaxeExport,@TauxCotisationsPatronalesCNaPS,@TauxCotisationsSalarialesCNaPS,
                 @IRSAMinimumPerception);";
        await conn.ExecuteAsync(sql, e);
    }

    // ════════════════════════════════════════════════════════════════
    //  PARAM IRSA TRANCHES
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<ParamIRSATrancheEntity>> GetParamIRSATranchesAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ParamIRSATrancheEntity>(
            "SELECT * FROM simulation.sim.[ParamIRSATranches] WHERE [ScenarioId]=@ScenarioId ORDER BY [Ordre]",
            new { ScenarioId = scenarioId });
    }

    public async Task ReplaceParamIRSATranchesAsync(int scenarioId, IEnumerable<ParamIRSATrancheEntity> tranches)
    {
        using var conn = _factory.CreateConnection();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(
            "DELETE FROM simulation.sim.[ParamIRSATranches] WHERE [ScenarioId]=@ScenarioId",
            new { ScenarioId = scenarioId }, tx);
        const string ins = @"
            INSERT INTO simulation.sim.[ParamIRSATranches]
                ([ScenarioId],[Ordre],[SeuilMin],[Taux],[Description])
            VALUES (@ScenarioId,@Ordre,@SeuilMin,@Taux,@Description);";
        await conn.ExecuteAsync(ins, tranches, tx);
        tx.Commit();
    }

    // ════════════════════════════════════════════════════════════════
    //  PARAM DISTRIBUTION SALARIALE
    // ════════════════════════════════════════════════════════════════

    public async Task<ParamDistributionSalarialeEntity?> GetParamDistributionSalarialeAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamDistributionSalarialeEntity>(
            "SELECT * FROM simulation.sim.[ParamDistributionSalariale] WHERE [ScenarioId]=@ScenarioId",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamDistributionSalarialeAsync(ParamDistributionSalarialeEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamDistributionSalariale] AS T
            USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [SalaireMedian]=@SalaireMedian,[Sigma]=@Sigma,[SalairePlancher]=@SalairePlancher,
                [SalairePlafond]=@SalairePlafond,[PartSecteurInformel]=@PartSecteurInformel,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[SalaireMedian],[Sigma],[SalairePlancher],[SalairePlafond],[PartSecteurInformel])
            VALUES (@ScenarioId,@SalaireMedian,@Sigma,@SalairePlancher,@SalairePlafond,@PartSecteurInformel);";
        await conn.ExecuteAsync(sql, e);
    }

    // ════════════════════════════════════════════════════════════════
    //  PARAM COMPORTEMENT MENAGE
    // ════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<ParamComportementMenageEntity>> GetParamComportementMenageAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ParamComportementMenageEntity>(
            "SELECT * FROM simulation.sim.[ParamComportementMenage] WHERE [ScenarioId]=@ScenarioId ORDER BY [Classe]",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamComportementMenageAsync(ParamComportementMenageEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamComportementMenage] AS T
            USING (SELECT @ScenarioId AS ScenarioId, @Classe AS Classe) AS S
                ON T.[ScenarioId]=S.[ScenarioId] AND T.[Classe]=S.[Classe]
            WHEN MATCHED THEN UPDATE SET
                [TauxEpargneMin]=@TauxEpargneMin,[TauxEpargneMax]=@TauxEpargneMax,
                [PropensionConsommationMin]=@PropensionConsommationMin,[PropensionConsommationMax]=@PropensionConsommationMax,
                [EpargneInitialeMax]=@EpargneInitialeMax,
                [DepensesAlimentairesJourMin]=@DepensesAlimentairesJourMin,[DepensesAlimentairesJourMax]=@DepensesAlimentairesJourMax,
                [DepensesDiversJourMin]=@DepensesDiversJourMin,[DepensesDiversJourMax]=@DepensesDiversJourMax,
                [ProbabiliteEmploiMin]=@ProbabiliteEmploiMin,[ProbabiliteEmploiMax]=@ProbabiliteEmploiMax,
                [ModeTransportPreferentiel]=@ModeTransportPreferentiel,
                [DistanceDomicileTravailKmMin]=@DistanceDomicileTravailKmMin,[DistanceDomicileTravailKmMax]=@DistanceDomicileTravailKmMax,
                [BudgetSortieWeekendMin]=@BudgetSortieWeekendMin,[BudgetSortieWeekendMax]=@BudgetSortieWeekendMax,
                [BudgetVacancesMin]=@BudgetVacancesMin,[BudgetVacancesMax]=@BudgetVacancesMax,
                [ProbabiliteSortieWeekendMin]=@ProbabiliteSortieWeekendMin,[ProbabiliteSortieWeekendMax]=@ProbabiliteSortieWeekendMax,
                [FrequenceVacancesJours]=@FrequenceVacancesJours,
                [ProbabiliteVacancesMin]=@ProbabiliteVacancesMin,[ProbabiliteVacancesMax]=@ProbabiliteVacancesMax,
                [DureeVacancesJoursMin]=@DureeVacancesJoursMin,[DureeVacancesJoursMax]=@DureeVacancesJoursMax,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[Classe],[ClasseLibelle],
                 [TauxEpargneMin],[TauxEpargneMax],[PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
                 [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
                 [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
                 [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax],
                 [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],[BudgetVacancesMin],[BudgetVacancesMax],
                 [ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax],
                 [FrequenceVacancesJours],[ProbabiliteVacancesMin],[ProbabiliteVacancesMax],
                 [DureeVacancesJoursMin],[DureeVacancesJoursMax])
            VALUES
                (@ScenarioId,@Classe,@ClasseLibelle,
                 @TauxEpargneMin,@TauxEpargneMax,@PropensionConsommationMin,@PropensionConsommationMax,@EpargneInitialeMax,
                 @DepensesAlimentairesJourMin,@DepensesAlimentairesJourMax,@DepensesDiversJourMin,@DepensesDiversJourMax,
                 @ProbabiliteEmploiMin,@ProbabiliteEmploiMax,@ModeTransportPreferentiel,
                 @DistanceDomicileTravailKmMin,@DistanceDomicileTravailKmMax,
                 @BudgetSortieWeekendMin,@BudgetSortieWeekendMax,@BudgetVacancesMin,@BudgetVacancesMax,
                 @ProbabiliteSortieWeekendMin,@ProbabiliteSortieWeekendMax,
                 @FrequenceVacancesJours,@ProbabiliteVacancesMin,@ProbabiliteVacancesMax,
                 @DureeVacancesJoursMin,@DureeVacancesJoursMax);";
        await conn.ExecuteAsync(sql, e);
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS — UPSERT GÉNÉRIQUE MONO-COLONNE
    //  (pour toutes les tables à une seule ligne par scénario)
    // ════════════════════════════════════════════════════════════════

    public async Task<ParamBanqueEntity?> GetParamBanqueAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamBanqueEntity>(
            "SELECT * FROM simulation.sim.[ParamBanque] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamBanqueAsync(ParamBanqueEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamBanque] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [TauxInteretDepots]=@TauxInteretDepots,[TauxInteretCredits]=@TauxInteretCredits,
                [TauxReserveObligatoire]=@TauxReserveObligatoire,[PartDepotsAVue]=@PartDepotsAVue,
                [PartMonnaieCirculationDansM1]=@PartMonnaieCirculationDansM1,[RatioM3SurM2]=@RatioM3SurM2,
                [CroissanceCreditJour]=@CroissanceCreditJour,[PartCreditEntreprises]=@PartCreditEntreprises,
                [ProbabiliteDefautCreditJour]=@ProbabiliteDefautCreditJour,[TauxRecouvrementNPLJour]=@TauxRecouvrementNPLJour,
                [AvoirsExterieursNetsInitiaux]=@AvoirsExterieursNetsInitiaux,
                [CreancesNettesEtatInitiales]=@CreancesNettesEtatInitiales,[SCBInitial]=@SCBInitial,
                [IntensiteInterventionBFM]=@IntensiteInterventionBFM,[RatioExcedentSCBCible]=@RatioExcedentSCBCible,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[TauxInteretDepots],[TauxInteretCredits],[TauxReserveObligatoire],
                 [PartDepotsAVue],[PartMonnaieCirculationDansM1],[RatioM3SurM2],[CroissanceCreditJour],
                 [PartCreditEntreprises],[ProbabiliteDefautCreditJour],[TauxRecouvrementNPLJour],
                 [AvoirsExterieursNetsInitiaux],[CreancesNettesEtatInitiales],[SCBInitial],
                 [IntensiteInterventionBFM],[RatioExcedentSCBCible])
            VALUES
                (@ScenarioId,@TauxInteretDepots,@TauxInteretCredits,@TauxReserveObligatoire,
                 @PartDepotsAVue,@PartMonnaieCirculationDansM1,@RatioM3SurM2,@CroissanceCreditJour,
                 @PartCreditEntreprises,@ProbabiliteDefautCreditJour,@TauxRecouvrementNPLJour,
                 @AvoirsExterieursNetsInitiaux,@CreancesNettesEtatInitiales,@SCBInitial,
                 @IntensiteInterventionBFM,@RatioExcedentSCBCible);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamPrixEntity?> GetParamPrixAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamPrixEntity>(
            "SELECT * FROM simulation.sim.[ParamPrix] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamPrixAsync(ParamPrixEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamPrix] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [PrixCarburantLitre]=@PrixCarburantLitre,[PrixCarburantReference]=@PrixCarburantReference,
                [ElasticitePrixParCarburant]=@ElasticitePrixParCarburant,[VolatiliteAleatoireMarche]=@VolatiliteAleatoireMarche,
                [PartRevenuAlimentaire]=@PartRevenuAlimentaire,[ElasticiteComportementMenage]=@ElasticiteComportementMenage,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[PrixCarburantLitre],[PrixCarburantReference],[ElasticitePrixParCarburant],
                 [VolatiliteAleatoireMarche],[PartRevenuAlimentaire],[ElasticiteComportementMenage])
            VALUES
                (@ScenarioId,@PrixCarburantLitre,@PrixCarburantReference,@ElasticitePrixParCarburant,
                 @VolatiliteAleatoireMarche,@PartRevenuAlimentaire,@ElasticiteComportementMenage);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamInflationEntity?> GetParamInflationAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamInflationEntity>(
            "SELECT * FROM simulation.sim.[ParamInflation] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamInflationAsync(ParamInflationEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamInflation] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [TauxInflationInitial]=@TauxInflationInitial,[InflationEndogeneActivee]=@InflationEndogeneActivee,
                [NAIRU]=@NAIRU,[CoefficientPhillips]=@CoefficientPhillips,
                [ElasticiteCarburantInflation]=@ElasticiteCarburantInflation,
                [ElasticiteImportInflation]=@ElasticiteImportInflation,
                [ElasticiteChangeInflation]=@ElasticiteChangeInflation,
                [ElasticiteSalairesInflation]=@ElasticiteSalairesInflation,
                [CoefficientMonetaire]=@CoefficientMonetaire,
                [PoidsAnticipationsAdaptatives]=@PoidsAnticipationsAdaptatives,
                [PoidsAncrageInflation]=@PoidsAncrageInflation,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[TauxInflationInitial],[InflationEndogeneActivee],[NAIRU],[CoefficientPhillips],
                 [ElasticiteCarburantInflation],[ElasticiteImportInflation],[ElasticiteChangeInflation],
                 [ElasticiteSalairesInflation],[CoefficientMonetaire],
                 [PoidsAnticipationsAdaptatives],[PoidsAncrageInflation])
            VALUES
                (@ScenarioId,@TauxInflationInitial,@InflationEndogeneActivee,@NAIRU,@CoefficientPhillips,
                 @ElasticiteCarburantInflation,@ElasticiteImportInflation,@ElasticiteChangeInflation,
                 @ElasticiteSalairesInflation,@CoefficientMonetaire,
                 @PoidsAnticipationsAdaptatives,@PoidsAncrageInflation);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamTauxChangeEntity?> GetParamTauxChangeAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamTauxChangeEntity>(
            "SELECT * FROM simulation.sim.[ParamTauxChange] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamTauxChangeAsync(ParamTauxChangeEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamTauxChange] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [TauxChangeDynamiqueActive]=@TauxChangeDynamiqueActive,
                [TauxChangeMGAParUSD]=@TauxChangeMGAParUSD,[ReservesBCMUSD]=@ReservesBCMUSD,
                [ElasticiteChangeBalanceCommerciale]=@ElasticiteChangeBalanceCommerciale,
                [PoidsChangePPA]=@PoidsChangePPA,[IntensiteInterventionBCM]=@IntensiteInterventionBCM,
                [ReservesMinimalesMoisImports]=@ReservesMinimalesMoisImports,
                [DepreciationStructurelleAnnuelle]=@DepreciationStructurelleAnnuelle,
                [InflationEtrangere]=@InflationEtrangere,
                [ElasticiteRemittancesChange]=@ElasticiteRemittancesChange,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[TauxChangeDynamiqueActive],[TauxChangeMGAParUSD],[ReservesBCMUSD],
                 [ElasticiteChangeBalanceCommerciale],[PoidsChangePPA],[IntensiteInterventionBCM],
                 [ReservesMinimalesMoisImports],[DepreciationStructurelleAnnuelle],
                 [InflationEtrangere],[ElasticiteRemittancesChange])
            VALUES
                (@ScenarioId,@TauxChangeDynamiqueActive,@TauxChangeMGAParUSD,@ReservesBCMUSD,
                 @ElasticiteChangeBalanceCommerciale,@PoidsChangePPA,@IntensiteInterventionBCM,
                 @ReservesMinimalesMoisImports,@DepreciationStructurelleAnnuelle,
                 @InflationEtrangere,@ElasticiteRemittancesChange);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamAgricultureEntity?> GetParamAgricultureAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamAgricultureEntity>(
            "SELECT * FROM simulation.sim.[ParamAgriculture] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamAgricultureAsync(ParamAgricultureEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamAgriculture] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [ProbabiliteSecheresseJourSaison]=@ProbabiliteSecheresseJourSaison,
                [PartMenagesRurauxAffectes]=@PartMenagesRurauxAffectes,
                [DureeSecheresseJoursBase]=@DureeSecheresseJoursBase,
                [ReductionProductionAgricole]=@ReductionProductionAgricole,
                [AideAlimentaireJourParMenage]=@AideAlimentaireJourParMenage,
                [ProbabiliteMigrationSaison]=@ProbabiliteMigrationSaison,
                [ValeurAutoconsommationJourBase]=@ValeurAutoconsommationJourBase,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[ProbabiliteSecheresseJourSaison],[PartMenagesRurauxAffectes],
                 [DureeSecheresseJoursBase],[ReductionProductionAgricole],[AideAlimentaireJourParMenage],
                 [ProbabiliteMigrationSaison],[ValeurAutoconsommationJourBase])
            VALUES
                (@ScenarioId,@ProbabiliteSecheresseJourSaison,@PartMenagesRurauxAffectes,
                 @DureeSecheresseJoursBase,@ReductionProductionAgricole,@AideAlimentaireJourParMenage,
                 @ProbabiliteMigrationSaison,@ValeurAutoconsommationJourBase);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamCycloneEntity?> GetParamCycloneAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamCycloneEntity>(
            "SELECT * FROM simulation.sim.[ParamCyclone] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamCycloneAsync(ParamCycloneEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamCyclone] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [ProbabiliteCycloneJourSaison]=@ProbabiliteCycloneJourSaison,
                [ProbabiliteCycloneJourHorsSaison]=@ProbabiliteCycloneJourHorsSaison,
                [DureeCycloneJoursMin]=@DureeCycloneJoursMin,[DureeCycloneJoursMax]=@DureeCycloneJoursMax,
                [BudgetTotalReconstructionBase]=@BudgetTotalReconstructionBase,
                [DureeReconstructionJoursMin]=@DureeReconstructionJoursMin,[DureeReconstructionJoursMax]=@DureeReconstructionJoursMax,
                [PartMenagesAffectesMin]=@PartMenagesAffectesMin,[PartMenagesAffectesMax]=@PartMenagesAffectesMax,
                [DelaiMinEntreDeuxCyclones]=@DelaiMinEntreDeuxCyclones,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[ProbabiliteCycloneJourSaison],[ProbabiliteCycloneJourHorsSaison],
                 [DureeCycloneJoursMin],[DureeCycloneJoursMax],[BudgetTotalReconstructionBase],
                 [DureeReconstructionJoursMin],[DureeReconstructionJoursMax],
                 [PartMenagesAffectesMin],[PartMenagesAffectesMax],[DelaiMinEntreDeuxCyclones])
            VALUES
                (@ScenarioId,@ProbabiliteCycloneJourSaison,@ProbabiliteCycloneJourHorsSaison,
                 @DureeCycloneJoursMin,@DureeCycloneJoursMax,@BudgetTotalReconstructionBase,
                 @DureeReconstructionJoursMin,@DureeReconstructionJoursMax,
                 @PartMenagesAffectesMin,@PartMenagesAffectesMax,@DelaiMinEntreDeuxCyclones);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamTransportEntity?> GetParamTransportAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamTransportEntity>(
            "SELECT * FROM simulation.sim.[ParamTransport] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamTransportAsync(ParamTransportEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamTransport] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [PartInformelTransportPublic]=@PartInformelTransportPublic,[PartFormelCarburant]=@PartFormelCarburant,
                [PartInformelEntretien]=@PartInformelEntretien,[CoutTaxiBe]=@CoutTaxiBe,
                [EntretienVoitureJour]=@EntretienVoitureJour,[EntretienFractionRevenuVoiture]=@EntretienFractionRevenuVoiture,
                [ConsoMotoLitrePour100km]=@ConsoMotoLitrePour100km,[ConsoVoitureLitrePour100km]=@ConsoVoitureLitrePour100km,
                [CoutTransportPaiementJirama]=@CoutTransportPaiementJirama,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[PartInformelTransportPublic],[PartFormelCarburant],[PartInformelEntretien],
                 [CoutTaxiBe],[EntretienVoitureJour],[EntretienFractionRevenuVoiture],
                 [ConsoMotoLitrePour100km],[ConsoVoitureLitrePour100km],[CoutTransportPaiementJirama])
            VALUES
                (@ScenarioId,@PartInformelTransportPublic,@PartFormelCarburant,@PartInformelEntretien,
                 @CoutTaxiBe,@EntretienVoitureJour,@EntretienFractionRevenuVoiture,
                 @ConsoMotoLitrePour100km,@ConsoVoitureLitrePour100km,@CoutTransportPaiementJirama);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamSanteEntity?> GetParamSanteAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamSanteEntity>(
            "SELECT * FROM simulation.sim.[ParamSante] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamSanteAsync(ParamSanteEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamSante] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [TauxOccupationHopitaux]=@TauxOccupationHopitaux,[CoutConsultationBase]=@CoutConsultationBase,
                [CoutHospitalisationBase]=@CoutHospitalisationBase,[PartFormelleDepenseSante]=@PartFormelleDepenseSante,
                [ProbabiliteHospitalisationBase]=@ProbabiliteHospitalisationBase,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[TauxOccupationHopitaux],[CoutConsultationBase],
                 [CoutHospitalisationBase],[PartFormelleDepenseSante],[ProbabiliteHospitalisationBase])
            VALUES
                (@ScenarioId,@TauxOccupationHopitaux,@CoutConsultationBase,
                 @CoutHospitalisationBase,@PartFormelleDepenseSante,@ProbabiliteHospitalisationBase);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamEducationEntity?> GetParamEducationAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamEducationEntity>(
            "SELECT * FROM simulation.sim.[ParamEducation] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamEducationAsync(ParamEducationEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamEducation] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [NombreEnfantsMoyenParMenage]=@NombreEnfantsMoyenParMenage,
                [PartEnfantsScolarises]=@PartEnfantsScolarises,[DureeDepenseEducationJours]=@DureeDepenseEducationJours,
                [CoutEducationJournalierParEnfant]=@CoutEducationJournalierParEnfant,
                [PartFormelleDepenseEducation]=@PartFormelleDepenseEducation,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[NombreEnfantsMoyenParMenage],[PartEnfantsScolarises],
                 [DureeDepenseEducationJours],[CoutEducationJournalierParEnfant],[PartFormelleDepenseEducation])
            VALUES
                (@ScenarioId,@NombreEnfantsMoyenParMenage,@PartEnfantsScolarises,
                 @DureeDepenseEducationJours,@CoutEducationJournalierParEnfant,@PartFormelleDepenseEducation);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<IEnumerable<ParamLoisirsEntity>> GetParamLoisirsAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ParamLoisirsEntity>(
            "SELECT * FROM simulation.sim.[ParamLoisirs] WHERE [ScenarioId]=@ScenarioId ORDER BY [Classe]",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamLoisirsAsync(ParamLoisirsEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamLoisirs] AS T
            USING (SELECT @ScenarioId AS ScenarioId, @Classe AS Classe) AS S
                ON T.[ScenarioId]=S.[ScenarioId] AND T.[Classe]=S.[Classe]
            WHEN MATCHED THEN UPDATE SET
                [BudgetSortieWeekendMin]=@BudgetSortieWeekendMin,[BudgetSortieWeekendMax]=@BudgetSortieWeekendMax,
                [BudgetVacancesMin]=@BudgetVacancesMin,[BudgetVacancesMax]=@BudgetVacancesMax,
                [ProbabiliteSortieWeekendMin]=@ProbabiliteSortieWeekendMin,[ProbabiliteSortieWeekendMax]=@ProbabiliteSortieWeekendMax,
                [ProbabiliteVacancesMin]=@ProbabiliteVacancesMin,[ProbabiliteVacancesMax]=@ProbabiliteVacancesMax,
                [FrequenceVacancesJours]=@FrequenceVacancesJours,
                [DureeVacancesJoursMin]=@DureeVacancesJoursMin,[DureeVacancesJoursMax]=@DureeVacancesJoursMax,
                [SensibiliteInflation]=@SensibiliteInflation,[SeuilInflationReaction]=@SeuilInflationReaction,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[Classe],[ClasseLibelle],
                 [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],[BudgetVacancesMin],[BudgetVacancesMax],
                 [ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax],
                 [ProbabiliteVacancesMin],[ProbabiliteVacancesMax],
                 [FrequenceVacancesJours],[DureeVacancesJoursMin],[DureeVacancesJoursMax],
                 [SensibiliteInflation],[SeuilInflationReaction])
            VALUES
                (@ScenarioId,@Classe,@ClasseLibelle,
                 @BudgetSortieWeekendMin,@BudgetSortieWeekendMax,@BudgetVacancesMin,@BudgetVacancesMax,
                 @ProbabiliteSortieWeekendMin,@ProbabiliteSortieWeekendMax,
                 @ProbabiliteVacancesMin,@ProbabiliteVacancesMax,
                 @FrequenceVacancesJours,@DureeVacancesJoursMin,@DureeVacancesJoursMax,
                 @SensibiliteInflation,@SeuilInflationReaction);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamEntreprisesEntity?> GetParamEntreprisesAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamEntreprisesEntity>(
            "SELECT * FROM simulation.sim.[ParamEntreprises] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamEntreprisesAsync(ParamEntreprisesEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamEntreprises] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [PartEntreprisesAgricoles]=@PartEntreprisesAgricoles,
                [PartEntreprisesConstruction]=@PartEntreprisesConstruction,
                [PartEntreprisesHotellerieTourisme]=@PartEntreprisesHotellerieTourisme,
                [MargeBeneficiaireEntreprise]=@MargeBeneficiaireEntreprise,
                [ProductiviteParEmployeJourDefaut]=@ProductiviteParEmployeJourDefaut,[PartB2B]=@PartB2B,
                [FacteurProductiviteInformelMin]=@FacteurProductiviteInformelMin,
                [FacteurProductiviteInformelMax]=@FacteurProductiviteInformelMax,
                [SeuilJoursStressTresorerie]=@SeuilJoursStressTresorerie,
                [SeuilJoursDemandeExcedentaire]=@SeuilJoursDemandeExcedentaire,
                [SalaireMoyenMensuelDefaut]=@SalaireMoyenMensuelDefaut,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[PartEntreprisesAgricoles],[PartEntreprisesConstruction],
                 [PartEntreprisesHotellerieTourisme],[MargeBeneficiaireEntreprise],
                 [ProductiviteParEmployeJourDefaut],[PartB2B],
                 [FacteurProductiviteInformelMin],[FacteurProductiviteInformelMax],
                 [SeuilJoursStressTresorerie],[SeuilJoursDemandeExcedentaire],[SalaireMoyenMensuelDefaut])
            VALUES
                (@ScenarioId,@PartEntreprisesAgricoles,@PartEntreprisesConstruction,
                 @PartEntreprisesHotellerieTourisme,@MargeBeneficiaireEntreprise,
                 @ProductiviteParEmployeJourDefaut,@PartB2B,
                 @FacteurProductiviteInformelMin,@FacteurProductiviteInformelMax,
                 @SeuilJoursStressTresorerie,@SeuilJoursDemandeExcedentaire,@SalaireMoyenMensuelDefaut);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<IEnumerable<ParamSecteurActiviteEntity>> GetParamSecteursActiviteAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<ParamSecteurActiviteEntity>(
            "SELECT * FROM simulation.sim.[ParamSecteurActivite] WHERE [ScenarioId]=@ScenarioId ORDER BY [Secteur]",
            new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamSecteurActiviteAsync(ParamSecteurActiviteEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamSecteurActivite] AS T
            USING (SELECT @ScenarioId AS ScenarioId, @Secteur AS Secteur) AS S
                ON T.[ScenarioId]=S.[ScenarioId] AND T.[Secteur]=S.[Secteur]
            WHEN MATCHED THEN UPDATE SET
                [ProductiviteJourMoyenne]=@ProductiviteJourMoyenne,[ProductiviteJourBasse]=@ProductiviteJourBasse,
                [TresorerieInitiale]=@TresorerieInitiale,[NombreEmployesDefaut]=@NombreEmployesDefaut,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[Secteur],[SecteurLibelle],[ProductiviteJourMoyenne],
                 [ProductiviteJourBasse],[TresorerieInitiale],[NombreEmployesDefaut])
            VALUES
                (@ScenarioId,@Secteur,@SecteurLibelle,@ProductiviteJourMoyenne,
                 @ProductiviteJourBasse,@TresorerieInitiale,@NombreEmployesDefaut);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamJiramaEntity?> GetParamJiramaAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamJiramaEntity>(
            "SELECT * FROM simulation.sim.[ParamJirama] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamJiramaAsync(ParamJiramaEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamJirama] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [PrixElectriciteArKWh]=@PrixElectriciteArKWh,[ConsommationElecMenageKWhJour]=@ConsommationElecMenageKWhJour,
                [ConsommationElecParEmployeKWhJour]=@ConsommationElecParEmployeKWhJour,
                [ConsommationElecEtatKWhJour]=@ConsommationElecEtatKWhJour,
                [PartProductionHydraulique]=@PartProductionHydraulique,
                [TauxPertesDistribution]=@TauxPertesDistribution,
                [PartConsommationElecMenages]=@PartConsommationElecMenages,
                [TarifEauJourMenage]=@TarifEauJourMenage,[TauxAccesEau]=@TauxAccesEau,
                [TauxAccesElectricite]=@TauxAccesElectricite,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[PrixElectriciteArKWh],[ConsommationElecMenageKWhJour],
                 [ConsommationElecParEmployeKWhJour],[ConsommationElecEtatKWhJour],
                 [PartProductionHydraulique],[TauxPertesDistribution],[PartConsommationElecMenages],
                 [TarifEauJourMenage],[TauxAccesEau],[TauxAccesElectricite])
            VALUES
                (@ScenarioId,@PrixElectriciteArKWh,@ConsommationElecMenageKWhJour,
                 @ConsommationElecParEmployeKWhJour,@ConsommationElecEtatKWhJour,
                 @PartProductionHydraulique,@TauxPertesDistribution,@PartConsommationElecMenages,
                 @TarifEauJourMenage,@TauxAccesEau,@TauxAccesElectricite);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamCommerceEntity?> GetParamCommerceAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamCommerceEntity>(
            "SELECT * FROM simulation.sim.[ParamCommerce] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamCommerceAsync(ParamCommerceEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamCommerce] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [RemittancesJour]=@RemittancesJour,
                [ConsommationRizAnnuelleKgParPersonne]=@ConsommationRizAnnuelleKgParPersonne,
                [PrixRizLocalKg]=@PrixRizLocalKg,[PrixRizImporteKg]=@PrixRizImporteKg,
                [PartRizImporte]=@PartRizImporte,[MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[RemittancesJour],[ConsommationRizAnnuelleKgParPersonne],
                 [PrixRizLocalKg],[PrixRizImporteKg],[PartRizImporte])
            VALUES
                (@ScenarioId,@RemittancesJour,@ConsommationRizAnnuelleKgParPersonne,
                 @PrixRizLocalKg,@PrixRizImporteKg,@PartRizImporte);";
        await conn.ExecuteAsync(sql, e);
    }

    public async Task<ParamImmobilierEntity?> GetParamImmobilierAsync(int scenarioId)
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ParamImmobilierEntity>(
            "SELECT * FROM simulation.sim.[ParamImmobilier] WHERE [ScenarioId]=@ScenarioId", new { ScenarioId = scenarioId });
    }

    public async Task UpsertParamImmobilierAsync(ParamImmobilierEntity e)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            MERGE simulation.sim.[ParamImmobilier] AS T USING (SELECT @ScenarioId AS ScenarioId) AS S ON T.[ScenarioId]=S.[ScenarioId]
            WHEN MATCHED THEN UPDATE SET
                [LoyerImputeJourParMenage]=@LoyerImputeJourParMenage,
                [TauxMenagesProprietaires]=@TauxMenagesProprietaires,[LoyerJourLocataire]=@LoyerJourLocataire,
                [ProbabiliteConstructionMaisonLocataire]=@ProbabiliteConstructionMaisonLocataire,
                [DureeConstructionMaisonJours]=@DureeConstructionMaisonJours,
                [BudgetConstructionMaisonJour]=@BudgetConstructionMaisonJour,
                [PartBudgetConstructionBTP]=@PartBudgetConstructionBTP,
                [PartBudgetConstructionQuincaillerie]=@PartBudgetConstructionQuincaillerie,
                [PartBudgetConstructionTransportInformel]=@PartBudgetConstructionTransportInformel,
                [MisAJourAt]=SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT
                ([ScenarioId],[LoyerImputeJourParMenage],[TauxMenagesProprietaires],[LoyerJourLocataire],
                 [ProbabiliteConstructionMaisonLocataire],[DureeConstructionMaisonJours],
                 [BudgetConstructionMaisonJour],[PartBudgetConstructionBTP],
                 [PartBudgetConstructionQuincaillerie],[PartBudgetConstructionTransportInformel])
            VALUES
                (@ScenarioId,@LoyerImputeJourParMenage,@TauxMenagesProprietaires,@LoyerJourLocataire,
                 @ProbabiliteConstructionMaisonLocataire,@DureeConstructionMaisonJours,
                 @BudgetConstructionMaisonJour,@PartBudgetConstructionBTP,
                 @PartBudgetConstructionQuincaillerie,@PartBudgetConstructionTransportInformel);";
        await conn.ExecuteAsync(sql, e);
    }
}
