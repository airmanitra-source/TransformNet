-- ============================================================
-- 005 - Table sim.ParamIRSATranches
-- Barème progressif IRSA (Impôt sur les Revenus Salariaux).
-- Plusieurs tranches par scénario, ordonnées par [Ordre].
-- Source : Code Général des Impôts de Madagascar.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamIRSATranches]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamIRSATranches] (
        [Id]          INT            NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamIRSATranches PRIMARY KEY,
        [ScenarioId]  INT            NOT NULL
            CONSTRAINT FK_ParamIRSATranches_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,
        [Ordre]       INT            NOT NULL,             -- Ordre d'application (1 = plus basse)
        [SeuilMin]    FLOAT          NOT NULL,             -- Seuil minimum MGA/mois
        [Taux]        FLOAT          NOT NULL,             -- Taux marginal (0.00 à 0.20)
        [Description] NVARCHAR(200)  NOT NULL DEFAULT N'',
        [MisAJourAt]  DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamIRSATranches_Scenario_Ordre UNIQUE ([ScenarioId], [Ordre])
    );

    -- Valeurs par défaut Madagascar (insérées via seed, pas ici)
END
GO
