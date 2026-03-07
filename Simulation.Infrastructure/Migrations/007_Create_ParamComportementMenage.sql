-- ============================================================
-- 007 - Table sim.ParamComportementMenage
-- Comportement économique par classe socio-économique :
-- épargne, consommation, alimentation, mobilité, loisirs.
-- 5 lignes par scénario (une par classe : 0=Subsistance .. 4=Cadre).
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamComportementMenage]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamComportementMenage] (
        [Id]                          INT          NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamComportementMenage PRIMARY KEY,
        [ScenarioId]                  INT          NOT NULL
            CONSTRAINT FK_ParamCptMen_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,
        [Classe]                      INT          NOT NULL,   -- 0=Subsistance,1=InformelBas,2=FormelBas,3=FormelQualifie,4=Cadre
        [ClasseLibelle]               NVARCHAR(50) NOT NULL,

        -- Revenus et épargne
        [TauxEpargneMin]              FLOAT NOT NULL,   -- Borne basse du tirage aléatoire
        [TauxEpargneMax]              FLOAT NOT NULL,
        [PropensionConsommationMin]   FLOAT NOT NULL,
        [PropensionConsommationMax]   FLOAT NOT NULL,
        [EpargneInitialeMax]          FLOAT NOT NULL,   -- Upper bound du tirage [0, Max]

        -- Dépenses journalières alimentaires
        [DepensesAlimentairesJourMin] FLOAT NOT NULL,
        [DepensesAlimentairesJourMax] FLOAT NOT NULL,

        -- Dépenses diverses (hors alimentation)
        [DepensesDiversJourMin]       FLOAT NOT NULL,
        [DepensesDiversJourMax]       FLOAT NOT NULL,

        -- Emploi
        [ProbabiliteEmploiMin]        FLOAT NOT NULL,
        [ProbabiliteEmploiMax]        FLOAT NOT NULL,

        -- Transport (0=APied,1=TransportPublic,2=Moto,3=Voiture)
        [ModeTransportPreferentiel]   INT   NOT NULL,
        [DistanceDomicileTravailKmMin] FLOAT NOT NULL,
        [DistanceDomicileTravailKmMax] FLOAT NOT NULL,

        -- Loisirs
        [BudgetSortieWeekendMin]      FLOAT NOT NULL DEFAULT 0,
        [BudgetSortieWeekendMax]      FLOAT NOT NULL DEFAULT 0,
        [BudgetVacancesMin]           FLOAT NOT NULL DEFAULT 0,
        [BudgetVacancesMax]           FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteSortieWeekendMin] FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteSortieWeekendMax] FLOAT NOT NULL DEFAULT 0,
        [FrequenceVacancesJours]      INT   NOT NULL DEFAULT 0,  -- 0=jamais,90=trimestre,180=semestriel
        [ProbabiliteVacancesMin]      FLOAT NOT NULL DEFAULT 0,
        [ProbabiliteVacancesMax]      FLOAT NOT NULL DEFAULT 0,
        [DureeVacancesJoursMin]       INT   NOT NULL DEFAULT 0,
        [DureeVacancesJoursMax]       INT   NOT NULL DEFAULT 0,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamCptMen_Scenario_Classe UNIQUE ([ScenarioId], [Classe])
    );
END
GO
