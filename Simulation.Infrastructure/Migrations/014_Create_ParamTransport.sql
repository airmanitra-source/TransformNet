-- ============================================================
-- 014 - Table sim.ParamTransport
-- Paramètres du module de transport : parts formel/informel
-- par mode, coûts (taxi-be, moto, voiture). Un enregistrement
-- par scénario.
-- ============================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamTransport]') AND type = N'U'
)
BEGIN
    CREATE TABLE simulation.sim.[ParamTransport] (
        [Id]                              INT   NOT NULL IDENTITY(1,1) CONSTRAINT PK_ParamTransport PRIMARY KEY,
        [ScenarioId]                      INT   NOT NULL
            CONSTRAINT FK_ParamTransport_Scenarios REFERENCES simulation.sim.[Scenarios]([Id]) ON DELETE CASCADE,

        -- Parts formel/informel
        [PartInformelTransportPublic]     FLOAT NOT NULL DEFAULT 0.70,    -- 70% taxi-be informel
        [PartFormelCarburant]             FLOAT NOT NULL DEFAULT 0.60,    -- 60% stations formelles
        [PartInformelEntretien]           FLOAT NOT NULL DEFAULT 0.90,    -- 90% garagistes informels

        -- Coûts de référence (MGA)
        [CoutTaxiBe]                      FLOAT NOT NULL DEFAULT 600.0,   -- MGA/trajet taxi-be
        [EntretienVoitureJour]            FLOAT NOT NULL DEFAULT 500.0,   -- MGA/jour voiture
        [EntretienFractionRevenuVoiture]  FLOAT NOT NULL DEFAULT 0.15,    -- fraction du coût transport

        -- Consommation carburant
        [ConsoMotoLitrePour100km]         FLOAT NOT NULL DEFAULT 3.0,     -- L/100km moto
        [ConsoVoitureLitrePour100km]      FLOAT NOT NULL DEFAULT 8.0,     -- L/100km voiture

        -- Coût accès Jirama pour transport Jirama (eau + électricité)
        [CoutTransportPaiementJirama]     FLOAT NOT NULL DEFAULT 1200.0,  -- MGA/visite

        [MisAJourAt]  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

        CONSTRAINT UX_ParamTransport_Scenario UNIQUE ([ScenarioId])
    );
END
GO
