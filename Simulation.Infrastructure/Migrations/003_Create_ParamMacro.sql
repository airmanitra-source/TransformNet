-- ============================================================
-- 003 - Table sim.ParamMacro
-- Paramètres macroéconomiques globaux : durée, population,
-- aides extérieures, dépenses en capital, dette publique.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamMacro]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamMacro] (
        [Id]                           INT      NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamMacro PRIMARY KEY,
        [ScenarioId]                   INT      NOT NULL
            CONSTRAINT FK_ParamMacro_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Simulation
        [DureeJours]                   INT      NOT NULL DEFAULT 365,
        [NombreMenages]                INT      NOT NULL DEFAULT 100000,
        [NombreEntreprises]            INT      NOT NULL DEFAULT 50000,

        -- Dualité urbain/rural
        [PartMenagesUrbains]           FLOAT    NOT NULL DEFAULT 0.30,  -- 30% urbains (RGPH 2018)

        -- Aide internationale & finances publiques
        [AideInternationaleJour]       FLOAT    NOT NULL DEFAULT 3704000000.0,
        [SubventionJiramaJour]         FLOAT    NOT NULL DEFAULT 1370000000.0,
        [DepensesCapitalJour]          FLOAT    NOT NULL DEFAULT 13526000000.0,
        [InteretsDetteJour]            FLOAT    NOT NULL DEFAULT 1678000000.0,
        [DettePubliqueInitiale]        FLOAT    NOT NULL DEFAULT 29250000000000.0,
        [DepensesPubliquesJour]        FLOAT    NOT NULL DEFAULT 3218000000.0,
        [TauxRedistribution]           FLOAT    NOT NULL DEFAULT 0.15,  -- transferts sociaux 15%

        -- Fonctionnaires
        [NombreFonctionnaires]         INT      NOT NULL DEFAULT 350000,
        [SalaireMoyenFonctionnaireMensuel] FLOAT NOT NULL DEFAULT 863000.0,

        -- Investissement & matrice IO
        [TauxReinvestissementPrive]    FLOAT    NOT NULL DEFAULT 0.25,
        [InvestissementProductifActive] BIT     NOT NULL DEFAULT 1,
        [TauxDepreciationCapitalAnnuel] FLOAT   NOT NULL DEFAULT 0.07,
        [SeuilUtilisationInvestissement] FLOAT  NOT NULL DEFAULT 0.70,
        [ElasticiteCapitalProductivite] FLOAT   NOT NULL DEFAULT 0.08,
        [InputOutputActivee]           BIT      NOT NULL DEFAULT 1,

        -- Autoconsommation agricole
        [ValeurAutoconsommationJourBase] FLOAT  NOT NULL DEFAULT 2500.0,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamMacro_Scenario UNIQUE ([ScenarioId])
    );
END
GO
