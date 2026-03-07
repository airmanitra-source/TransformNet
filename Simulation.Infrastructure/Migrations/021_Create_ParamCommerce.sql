-- ============================================================
-- 021 - Table sim.ParamCommerce
-- Paramètres du commerce extérieur : importations/exportations,
-- transferts diaspora (remittances), part importée du riz.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamCommerce]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamCommerce] (
        [Id]                    INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamCommerce PRIMARY KEY,
        [ScenarioId]            INT   NOT NULL
            CONSTRAINT FK_ParamCommerce_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Transferts diaspora
        [RemittancesJour]       FLOAT NOT NULL DEFAULT 7400000000.0,     -- MGA/jour (~2.7 Mds USD/an)

        -- Riz (denrée stratégique)
        [ConsommationRizAnnuelleKgParPersonne] FLOAT NOT NULL DEFAULT 130.0,
        [PrixRizLocalKg]        FLOAT NOT NULL DEFAULT 2400.0,           -- MGA/kg riz local
        [PrixRizImporteKg]      FLOAT NOT NULL DEFAULT 2800.0,           -- MGA/kg riz importé
        [PartRizImporte]        FLOAT NOT NULL DEFAULT 0.18,             -- 18% du riz est importé

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamCommerce_Scenario UNIQUE ([ScenarioId])
    );
END
GO
