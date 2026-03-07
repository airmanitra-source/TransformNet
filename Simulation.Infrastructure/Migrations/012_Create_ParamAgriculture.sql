-- ============================================================
-- 012 - Table sim.ParamAgriculture
-- Sécheresse stochastique (Kere du Grand Sud) : probabilité,
-- durée, impact production agricole, aide alimentaire, migration.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamAgriculture]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamAgriculture] (
        [Id]                            INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamAgriculture PRIMARY KEY,
        [ScenarioId]                    INT   NOT NULL
            CONSTRAINT FK_ParamAgriculture_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Probabilité déclenchement (saison sèche : mai-novembre)
        [ProbabiliteSecheresseJourSaison]  FLOAT NOT NULL DEFAULT 0.001,    -- 0.1%/jour en saison
        [PartMenagesRurauxAffectes]        FLOAT NOT NULL DEFAULT 0.08,     -- 8% des ruraux
        [DureeSecheresseJoursBase]         INT   NOT NULL DEFAULT 120,      -- ±30 jours aléatoire
        [ReductionProductionAgricole]      FLOAT NOT NULL DEFAULT 0.60,     -- -60% production au pic

        -- Aide et migration
        [AideAlimentaireJourParMenage]     FLOAT NOT NULL DEFAULT 3000.0,   -- MGA/jour/ménage affecté
        [ProbabiliteMigrationSaison]       FLOAT NOT NULL DEFAULT 0.12,     -- 12% migrent sur la durée

        -- Autoconsommation
        [ValeurAutoconsommationJourBase]   FLOAT NOT NULL DEFAULT 2500.0,   -- MGA/jour ménage rural

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamAgriculture_Scenario UNIQUE ([ScenarioId])
    );
END
GO
