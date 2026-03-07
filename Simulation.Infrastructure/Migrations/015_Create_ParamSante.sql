-- ============================================================
-- 015 - Table sim.ParamSante
-- Paramètres du module dépenses de santé : taux d'occupation
-- hôpitaux, coût consultation/hospitalisation, part formelle.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamSante]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamSante] (
        [Id]                          INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamSante PRIMARY KEY,
        [ScenarioId]                  INT   NOT NULL
            CONSTRAINT FK_ParamSante_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        [TauxOccupationHopitaux]      FLOAT NOT NULL DEFAULT 0.68,      -- 68% taux d'occupation
        [CoutConsultationBase]        FLOAT NOT NULL DEFAULT 8000.0,    -- MGA/consultation
        [CoutHospitalisationBase]     FLOAT NOT NULL DEFAULT 45000.0,   -- MGA/hospitalisation
        [PartFormelleDepenseSante]    FLOAT NOT NULL DEFAULT 0.70,      -- 70% vers secteur formel

        -- Probabilité base d'hospitalisation (augmentée par taux occupation)
        [ProbabiliteHospitalisationBase] FLOAT NOT NULL DEFAULT 0.10,   -- 10% des malades

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamSante_Scenario UNIQUE ([ScenarioId])
    );
END
GO
