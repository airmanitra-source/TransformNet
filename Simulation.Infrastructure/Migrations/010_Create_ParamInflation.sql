-- ============================================================
-- 010 - Table sim.ParamInflation
-- Modèle d'inflation hybride (Phillips + cost-push + monétaire
-- + anticipations). Calibré sur la structure macroéconomique
-- de Madagascar. Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamInflation]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamInflation] (
        [Id]                             INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamInflation PRIMARY KEY,
        [ScenarioId]                     INT   NOT NULL
            CONSTRAINT FK_ParamInflation_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Paramètre de base
        [TauxInflationInitial]           FLOAT NOT NULL DEFAULT 0.08,    -- 8%/an

        -- Activation
        [InflationEndogeneActivee]       BIT   NOT NULL DEFAULT 1,

        -- Courbe de Phillips
        [NAIRU]                          FLOAT NOT NULL DEFAULT 0.175,   -- NAIRU ~17.5% (sous-emploi structurel)
        [CoefficientPhillips]            FLOAT NOT NULL DEFAULT 0.25,    -- sensibilité Phillips

        -- Cost-push (chocs d'offre)
        [ElasticiteCarburantInflation]   FLOAT NOT NULL DEFAULT 0.15,    -- canal carburant
        [ElasticiteImportInflation]      FLOAT NOT NULL DEFAULT 0.25,    -- canal importations
        [ElasticiteChangeInflation]      FLOAT NOT NULL DEFAULT 0.30,    -- canal taux de change
        [ElasticiteSalairesInflation]    FLOAT NOT NULL DEFAULT 0.10,    -- spirale prix-salaires

        -- Monétaire (théorie quantitative)
        [CoefficientMonetaire]           FLOAT NOT NULL DEFAULT 0.20,    -- λ dans MV=PY

        -- Anticipations adaptatives
        [PoidsAnticipationsAdaptatives]  FLOAT NOT NULL DEFAULT 0.70,    -- 70% adaptatives
        [PoidsAncrageInflation]          FLOAT NOT NULL DEFAULT 0.30,    -- 30% ancrage BCM

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamInflation_Scenario UNIQUE ([ScenarioId])
    );
END
GO
