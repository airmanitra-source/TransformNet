-- ============================================================
-- 019 - Table sim.ParamSecteurActivite
-- Valeurs par défaut par secteur d'activité :
-- productivité journalière (MGA/employé/jour) et trésorerie initiale.
-- Une ligne par (scénario × secteur). Secteurs : 0=Agriculture,
-- 1=Textiles, 2=Commerces, 3=Services, 4=SecteurMinier,
-- 5=Construction, 6=HotellerieTourisme.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamSecteurActivite]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamSecteurActivite] (
        [Id]                         INT          NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamSecteurActivite PRIMARY KEY,
        [ScenarioId]                 INT          NOT NULL
            CONSTRAINT FK_ParamSecteur_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,
        [Secteur]                    INT          NOT NULL,   -- ESecteurActivite ordinal
        [SecteurLibelle]             NVARCHAR(50) NOT NULL,

        [ProductiviteJourMoyenne]    FLOAT NOT NULL,   -- MGA/employé/jour (moyenne)
        [ProductiviteJourBasse]      FLOAT NOT NULL,   -- MGA/employé/jour (basse estimation)
        [TresorerieInitiale]         FLOAT NOT NULL,   -- MGA

        -- Nombre d'employés par défaut pour ce secteur
        [NombreEmployesDefaut]       INT   NOT NULL DEFAULT 10,

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamSecteur_Scenario_Secteur UNIQUE ([ScenarioId], [Secteur])
    );
END
GO
