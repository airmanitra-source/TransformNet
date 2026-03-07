-- ============================================================
-- 013 - Table sim.ParamCyclone
-- Chocs cycloniques stochastiques : probabilité par saison,
-- intensité, reconstruction BTP. Saison nov-avril à Madagascar.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamCyclone]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamCyclone] (
        [Id]                                  INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamCyclone PRIMARY KEY,
        [ScenarioId]                          INT   NOT NULL
            CONSTRAINT FK_ParamCyclone_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Probabilité de déclenchement
        [ProbabiliteCycloneJourSaison]        FLOAT NOT NULL DEFAULT 0.003,   -- 0.3%/jour saison (nov-avr)
        [ProbabiliteCycloneJourHorsSaison]    FLOAT NOT NULL DEFAULT 0.0002,  -- 0.02%/jour hors saison

        -- Durée cyclone
        [DureeCycloneJoursMin]                INT   NOT NULL DEFAULT 3,
        [DureeCycloneJoursMax]                INT   NOT NULL DEFAULT 7,

        -- Reconstruction
        [BudgetTotalReconstructionBase]       FLOAT NOT NULL DEFAULT 650000.0,  -- MGA/ménage affecté
        [DureeReconstructionJoursMin]         INT   NOT NULL DEFAULT 45,
        [DureeReconstructionJoursMax]         INT   NOT NULL DEFAULT 120,

        -- Impact sur ménages
        [PartMenagesAffectesMin]              FLOAT NOT NULL DEFAULT 0.05,    -- cat.1 → 5%
        [PartMenagesAffectesMax]              FLOAT NOT NULL DEFAULT 0.30,    -- cat.5 → 30%

        -- Délai minimum entre deux cyclones (jours)
        [DelaiMinEntreDeuxCyclones]           INT   NOT NULL DEFAULT 30,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamCyclone_Scenario UNIQUE ([ScenarioId])
    );
END
GO
