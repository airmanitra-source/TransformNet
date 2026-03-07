-- ============================================================
-- 006 - Table sim.ParamDistributionSalariale
-- Loi log-normale des salaires : médiane, sigma, plancher/plafond,
-- part du secteur informel. Calibré INSTAT Madagascar (Gini ~0.43).
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamDistributionSalariale]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamDistributionSalariale] (
        [Id]                   INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamDistributionSalariale PRIMARY KEY,
        [ScenarioId]           INT   NOT NULL
            CONSTRAINT FK_ParamDistSal_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        [SalaireMedian]        FLOAT NOT NULL DEFAULT 170000.0,   -- MGA/mois
        [Sigma]                FLOAT NOT NULL DEFAULT 0.85,        -- Dispersion log-normale (Gini ~0.43)
        [SalairePlancher]      FLOAT NOT NULL DEFAULT 50000.0,     -- MGA/mois (informel sous SMIG)
        [SalairePlafond]       FLOAT NOT NULL DEFAULT 10000000.0,  -- MGA/mois (PDG grandes entreprises)
        [PartSecteurInformel]  FLOAT NOT NULL DEFAULT 0.85,        -- 85% informel (INSTAT ENEMPSI)

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamDistSal_Scenario UNIQUE ([ScenarioId])
    );
END
GO
