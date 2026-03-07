-- ============================================================
-- 016 - Table sim.ParamEducation
-- Paramètres du module dépenses d'éducation : scolarisation,
-- coût journalier par enfant, durée, part formelle.
-- Un enregistrement par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamEducation]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamEducation] (
        [Id]                               INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamEducation PRIMARY KEY,
        [ScenarioId]                       INT   NOT NULL
            CONSTRAINT FK_ParamEducation_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        [NombreEnfantsMoyenParMenage]      FLOAT NOT NULL DEFAULT 2.3,    -- enfants/ménage (RGPH)
        [PartEnfantsScolarises]            FLOAT NOT NULL DEFAULT 0.72,   -- 72% taux de scolarisation
        [DureeDepenseEducationJours]       INT   NOT NULL DEFAULT 180,    -- 180 jours/an (année scolaire)
        [CoutEducationJournalierParEnfant] FLOAT NOT NULL DEFAULT 900.0,  -- MGA/jour/enfant scolarisé
        [PartFormelleDepenseEducation]     FLOAT NOT NULL DEFAULT 0.75,   -- 75% vers secteur formel

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamEducation_Scenario UNIQUE ([ScenarioId])
    );
END
GO
