-- ============================================================
-- 009 - Table sim.ParamPrix
-- Paramètres du module Prix : carburant, élasticités,
-- volatilité de marché, comportement consommateur.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamPrix]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamPrix] (
        [Id]                            INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamPrix PRIMARY KEY,
        [ScenarioId]                    INT   NOT NULL
            CONSTRAINT FK_ParamPrix_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        [PrixCarburantLitre]            FLOAT NOT NULL DEFAULT 5500.0,  -- MGA/litre
        [PrixCarburantReference]        FLOAT NOT NULL DEFAULT 5500.0,  -- MGA/litre (base élasticité)

        -- Élasticités
        [ElasticitePrixParCarburant]    FLOAT NOT NULL DEFAULT 0.70,    -- transmission carburant → prix
        [VolatiliteAleatoireMarche]     FLOAT NOT NULL DEFAULT 0.10,    -- σ aléa journalier ±10%
        [PartRevenuAlimentaire]         FLOAT NOT NULL DEFAULT 0.40,    -- 40% revenu = nourriture
        [ElasticiteComportementMenage]  FLOAT NOT NULL DEFAULT 0.65,    -- sensibilité prix répétitifs

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamPrix_Scenario UNIQUE ([ScenarioId])
    );
END
GO
