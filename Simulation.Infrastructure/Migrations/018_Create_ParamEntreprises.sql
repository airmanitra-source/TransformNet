-- ============================================================
-- 018 - Table sim.ParamEntreprises
-- Paramètres globaux des entreprises : répartition sectorielle,
-- marge, productivité, marché du travail, FBCF.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamEntreprises]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamEntreprises] (
        [Id]                                 INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamEntreprises PRIMARY KEY,
        [ScenarioId]                         INT   NOT NULL
            CONSTRAINT FK_ParamEntreprises_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Répartition sectorielle
        [PartEntreprisesAgricoles]           FLOAT NOT NULL DEFAULT 0.30,
        [PartEntreprisesConstruction]        FLOAT NOT NULL DEFAULT 0.05,
        [PartEntreprisesHotellerieTourisme]  FLOAT NOT NULL DEFAULT 0.03,

        -- Paramètres généraux
        [MargeBeneficiaireEntreprise]        FLOAT NOT NULL DEFAULT 0.20,   -- 20% marge nette
        [ProductiviteParEmployeJourDefaut]   FLOAT NOT NULL DEFAULT 15000.0, -- MGA/jour (défaut)
        [PartB2B]                            FLOAT NOT NULL DEFAULT 0.30,    -- 30% CA = B2B

        -- Facteur productivité informel (0.3-0.6 du formel)
        [FacteurProductiviteInformelMin]     FLOAT NOT NULL DEFAULT 0.30,
        [FacteurProductiviteInformelMax]     FLOAT NOT NULL DEFAULT 0.60,

        -- Marché du travail (seuils de décision embauche/licenciement)
        [SeuilJoursStressTresorerie]         INT   NOT NULL DEFAULT 30,    -- jours → licenciement
        [SeuilJoursDemandeExcedentaire]      INT   NOT NULL DEFAULT 15,    -- jours → embauche
        [SalaireMoyenMensuelDefaut]          FLOAT NOT NULL DEFAULT 200000.0, -- MGA/mois/employé

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamEntreprises_Scenario UNIQUE ([ScenarioId])
    );
END
GO
