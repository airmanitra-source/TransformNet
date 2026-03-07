-- ============================================================
-- 008 - Table sim.ParamBanque
-- Paramètres du secteur bancaire : agrégats monétaires initiaux,
-- taux d'intérêt, réserves, NPL. Calibré rapports BCM 2024.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamBanque]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamBanque] (
        [Id]                          INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamBanque PRIMARY KEY,
        [ScenarioId]                  INT   NOT NULL
            CONSTRAINT FK_ParamBanque_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Taux d'intérêt
        [TauxInteretDepots]           FLOAT NOT NULL DEFAULT 0.045,   -- 4.5% créditeur BCM 2024
        [TauxInteretCredits]          FLOAT NOT NULL DEFAULT 0.16,    -- 16% débiteur BCM 2024

        -- Réserves obligatoires
        [TauxReserveObligatoire]      FLOAT NOT NULL DEFAULT 0.13,    -- 13% RO BCM

        -- Multiplicateur monétaire
        [PartDepotsAVue]              FLOAT NOT NULL DEFAULT 0.55,    -- 55% dépôts à vue
        [PartMonnaieCirculationDansM1] FLOAT NOT NULL DEFAULT 0.45,   -- 45% fiduciaire dans M1
        [RatioM3SurM2]                FLOAT NOT NULL DEFAULT 1.10,    -- M3/M2 = 1.10

        -- Croissance crédit
        [CroissanceCreditJour]        FLOAT NOT NULL DEFAULT 0.00041, -- ~15%/an (1.15^(1/365)-1)
        [PartCreditEntreprises]       FLOAT NOT NULL DEFAULT 0.75,    -- 75% crédit → entreprises

        -- NPL
        [ProbabiliteDefautCreditJour] FLOAT NOT NULL DEFAULT 0.0003,  -- ~10% NPL/an
        [TauxRecouvrementNPLJour]     FLOAT NOT NULL DEFAULT 0.002,

        -- Agrégats initiaux (MGA, mis à l'échelle par facteurEchelle)
        [AvoirsExterieursNetsInitiaux]  FLOAT NOT NULL DEFAULT 11250000000000.0,
        [CreancesNettesEtatInitiales]   FLOAT NOT NULL DEFAULT 3000000000000.0,
        [SCBInitial]                    FLOAT NOT NULL DEFAULT 2430000000000.0,

        -- BFM interventions
        [IntensiteInterventionBFM]    FLOAT NOT NULL DEFAULT 0.50,
        [RatioExcedentSCBCible]       FLOAT NOT NULL DEFAULT 0.055,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamBanque_Scenario UNIQUE ([ScenarioId])
    );
END
GO
