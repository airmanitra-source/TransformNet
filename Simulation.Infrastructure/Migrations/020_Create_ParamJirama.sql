-- ============================================================
-- 020 - Table sim.ParamJirama
-- Paramètres de la JIRAMA (eau + électricité) :
-- prix, consommation, pertes de distribution, accès.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamJirama]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamJirama] (
        [Id]                              INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamJirama PRIMARY KEY,
        [ScenarioId]                      INT   NOT NULL
            CONSTRAINT FK_ParamJirama_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Électricité
        [PrixElectriciteArKWh]            FLOAT NOT NULL DEFAULT 653.0,      -- MGA/kWh
        [ConsommationElecMenageKWhJour]   FLOAT NOT NULL DEFAULT 1.03,       -- kWh/ménage/jour (accédant)
        [ConsommationElecParEmployeKWhJour] FLOAT NOT NULL DEFAULT 2.5,      -- kWh/employé/jour
        [ConsommationElecEtatKWhJour]     FLOAT NOT NULL DEFAULT 44400.0,    -- kWh/jour État

        -- Production et pertes
        [PartProductionHydraulique]       FLOAT NOT NULL DEFAULT 0.516,      -- 51.6% hydraulique
        [TauxPertesDistribution]          FLOAT NOT NULL DEFAULT 0.289,      -- 28.9% pertes
        [PartConsommationElecMenages]     FLOAT NOT NULL DEFAULT 0.474,      -- 47.4% consommation ménages

        -- Eau
        [TarifEauJourMenage]              FLOAT NOT NULL DEFAULT 500.0,      -- MGA/jour/ménage
        [TauxAccesEau]                    FLOAT NOT NULL DEFAULT 0.25,       -- 25% accès eau potable
        [TauxAccesElectricite]            FLOAT NOT NULL DEFAULT 0.30,       -- 30% accès électricité

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamJirama_Scenario UNIQUE ([ScenarioId])
    );
END
GO
