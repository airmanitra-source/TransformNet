-- ============================================================
-- 017 - Table sim.ParamLoisirs
-- Paramètres de dépenses de loisirs par classe socio-économique :
-- weekend, vacances, fréquences, sensibilité inflation.
-- 5 lignes par scénario (une par classe).
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamLoisirs]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamLoisirs] (
        [Id]                           INT          NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamLoisirs PRIMARY KEY,
        [ScenarioId]                   INT          NOT NULL
            CONSTRAINT FK_ParamLoisirs_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,
        [Classe]                       INT          NOT NULL,   -- 0..4
        [ClasseLibelle]                NVARCHAR(50) NOT NULL,

        -- Budget sorties weekend (MGA)
        [BudgetSortieWeekendMin]       FLOAT NOT NULL DEFAULT 0,
        [BudgetSortieWeekendMax]       FLOAT NOT NULL DEFAULT 0,

        -- Budget vacances par jour de séjour (MGA)
        [BudgetVacancesMin]            FLOAT NOT NULL DEFAULT 0,
        [BudgetVacancesMax]            FLOAT NOT NULL DEFAULT 0,

        -- Probabilités
        [ProbabiliteSortieWeekendMin]  FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteSortieWeekendMax]  FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteVacancesMin]       FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteVacancesMax]       FLOAT NOT NULL DEFAULT 0,

        -- Fréquence (0=jamais, 90=trimestriel, 180=semestriel)
        [FrequenceVacancesJours]       INT   NOT NULL DEFAULT 0,
        [DureeVacancesJoursMin]        INT   NOT NULL DEFAULT 0,
        [DureeVacancesJoursMax]        INT   NOT NULL DEFAULT 0,

        -- Sensibilité inflation (logistique)
        [SensibiliteInflation]         FLOAT NOT NULL DEFAULT 0.15,  -- k dans courbe logistique
        [SeuilInflationReaction]       FLOAT NOT NULL DEFAULT 8.0,   -- x0 (seuil en %)

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamLoisirs_Scenario_Classe UNIQUE ([ScenarioId], [Classe])
    );
END
GO
