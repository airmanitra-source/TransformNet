-- ============================================================
-- 027 - ALTER sim.ParamJirama
-- Ajout des colonnes manquantes pour l'initialisation de
-- l'agent AgentJirama dans SimulationModule, qui étaient
-- hardcodées dans le code C# :
--   TresorerieInitiale        = 10 000 000 MGA
--   NombreEmployesBase        = 5 000 employés (avant facteur échelle)
--   SalaireMoyenMensuelEmploye = 350 000 MGA/mois
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamJirama]')
      AND name = N'TresorerieInitiale'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamJirama]
    ADD [TresorerieInitiale] FLOAT NOT NULL DEFAULT 10000000.0;  -- MGA
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamJirama]')
      AND name = N'NombreEmployesBase'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamJirama]
    ADD [NombreEmployesBase] INT NOT NULL DEFAULT 5000;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamJirama]')
      AND name = N'SalaireMoyenMensuelEmploye'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamJirama]
    ADD [SalaireMoyenMensuelEmploye] FLOAT NOT NULL DEFAULT 350000.0;  -- MGA/mois
END
GO
