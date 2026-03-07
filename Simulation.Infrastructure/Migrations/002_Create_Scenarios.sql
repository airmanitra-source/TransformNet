-- ============================================================
-- 002 - Table sim.Scenarios
-- Catalogue des scénarios de simulation. Chaque scénario est
-- une version nommée et versionnable de l'ensemble des paramètres.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[Scenarios]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[Scenarios] (
        [Id]          INT            NOT NULL IDENTITY(1,1)  CONSTRAINT PK_Scenarios PRIMARY KEY,
        [Nom]         NVARCHAR(200)  NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL DEFAULT N'',
        [Version]     INT            NOT NULL DEFAULT 1,
        [EstBase]     BIT            NOT NULL DEFAULT 0,   -- Scénario de référence (Madagascar baseline)
        [EstActif]    BIT            NOT NULL DEFAULT 1,
        [CreePar]     NVARCHAR(100)  NOT NULL DEFAULT N'system',
        [CreeAt]      DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
        [MisAJourAt]  DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_Scenarios_Nom_Version
        ON simulation.sim.[Scenarios] ([Nom], [Version]);
END
GO
