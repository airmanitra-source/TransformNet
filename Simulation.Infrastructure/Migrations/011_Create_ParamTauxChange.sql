-- ============================================================
-- 011 - Table sim.ParamTauxChange
-- Taux de change dynamique MGA/USD : flottement géré BCM,
-- PPA relative, intervention, saisonnalité vanille.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamTauxChange]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamTauxChange] (
        [Id]                                INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamTauxChange PRIMARY KEY,
        [ScenarioId]                        INT   NOT NULL
            CONSTRAINT FK_ParamTauxChange_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        [TauxChangeDynamiqueActive]         BIT   NOT NULL DEFAULT 1,
        [TauxChangeMGAParUSD]               FLOAT NOT NULL DEFAULT 4500.0,    -- BCM sept. 2024
        [ReservesBCMUSD]                    FLOAT NOT NULL DEFAULT 2500000000.0, -- 2.5 Mds USD

        -- Élasticités
        [ElasticiteChangeBalanceCommerciale] FLOAT NOT NULL DEFAULT 0.50,     -- flottement géré
        [PoidsChangePPA]                    FLOAT NOT NULL DEFAULT 0.30,      -- convergence PPA
        [IntensiteInterventionBCM]          FLOAT NOT NULL DEFAULT 0.50,      -- intervention modérée
        [ReservesMinimalesMoisImports]      FLOAT NOT NULL DEFAULT 3.0,       -- FMI recommande 3 mois
        [DepreciationStructurelleAnnuelle]  FLOAT NOT NULL DEFAULT 0.05,      -- ~5%/an historique
        [InflationEtrangere]                FLOAT NOT NULL DEFAULT 0.03,      -- inflation USD 3%
        [ElasticiteRemittancesChange]       FLOAT NOT NULL DEFAULT 0.50,      -- élasticité remittances/change

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamTauxChange_Scenario UNIQUE ([ScenarioId])
    );
END
GO
