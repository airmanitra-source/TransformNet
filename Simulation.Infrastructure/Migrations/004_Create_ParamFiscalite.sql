-- ============================================================
-- 004 - Table sim.ParamFiscalite
-- Taux d'imposition : IS, TVA, droits de douane, accises,
-- taxes export, cotisations CNaPS. Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamFiscalite]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamFiscalite] (
        [Id]                                INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamFiscalite PRIMARY KEY,
        [ScenarioId]                        INT   NOT NULL
            CONSTRAINT FK_ParamFiscalite_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Impôts directs
        [TauxIS]                            FLOAT NOT NULL DEFAULT 0.20,  -- Impôt sur les Sociétés 20%
        [TauxTVA]                           FLOAT NOT NULL DEFAULT 0.20,  -- TVA 20%
        [TauxDirecteur]                     FLOAT NOT NULL DEFAULT 0.09,  -- Taux directeur BCM 9%

        -- Fiscalité douanière
        [TauxDroitsDouane]                  FLOAT NOT NULL DEFAULT 0.12,  -- Droits de douane 12%
        [TauxAccise]                        FLOAT NOT NULL DEFAULT 0.10,  -- Droits d'accise 10%
        [TauxTaxeExport]                    FLOAT NOT NULL DEFAULT 0.03,  -- Taxe export 3%

        -- Cotisations sociales
        [TauxCotisationsPatronalesCNaPS]    FLOAT NOT NULL DEFAULT 0.18,  -- CNaPS patronal 18%
        [TauxCotisationsSalarialesCNaPS]    FLOAT NOT NULL DEFAULT 0.01,  -- CNaPS salarial 1%

        -- IRSA minimum de perception (MGA/mois)
        [IRSAMinimumPerception]             FLOAT NOT NULL DEFAULT 2000.0,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamFiscalite_Scenario UNIQUE ([ScenarioId])
    );
END
GO
