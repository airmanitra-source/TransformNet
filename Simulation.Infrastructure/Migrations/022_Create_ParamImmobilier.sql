-- ============================================================
-- 022 - Table sim.ParamImmobilier
-- Paramètres du marché immobilier et du logement :
-- loyer, propriété, construction. Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamImmobilier]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamImmobilier] (
        [Id]                                   INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamImmobilier PRIMARY KEY,
        [ScenarioId]                           INT   NOT NULL
            CONSTRAINT FK_ParamImmobilier_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Loyer
        [LoyerImputeJourParMenage]             FLOAT NOT NULL DEFAULT 1000.0,   -- MGA/jour propriétaire
        [TauxMenagesProprietaires]             FLOAT NOT NULL DEFAULT 0.65,     -- 65% propriétaires
        [LoyerJourLocataire]                   FLOAT NOT NULL DEFAULT 3500.0,   -- MGA/jour locataire

        -- Construction
        [ProbabiliteConstructionMaisonLocataire] FLOAT NOT NULL DEFAULT 0.08,   -- 8%/an
        [DureeConstructionMaisonJours]         INT   NOT NULL DEFAULT 240,
        [BudgetConstructionMaisonJour]         FLOAT NOT NULL DEFAULT 7500.0,   -- MGA/jour

        -- Ventilation budget construction
        [PartBudgetConstructionBTP]            FLOAT NOT NULL DEFAULT 0.55,     -- 55% BTP
        [PartBudgetConstructionQuincaillerie]  FLOAT NOT NULL DEFAULT 0.30,     -- 30% quincaillerie
        [PartBudgetConstructionTransportInformel] FLOAT NOT NULL DEFAULT 0.15,  -- 15% transport informel

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamImmobilier_Scenario UNIQUE ([ScenarioId])
    );
END
GO
