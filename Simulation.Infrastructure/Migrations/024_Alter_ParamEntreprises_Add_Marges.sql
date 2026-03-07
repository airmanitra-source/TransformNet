-- ============================================================
-- 024 - ALTER sim.ParamEntreprises
-- Ajout des colonnes manquantes qui étaient hardcodées dans
-- SimulationModule.cs :
--   MargeReventeImport     : marge de revente des importateurs (0.25 = 25%)
--   PartExporteurProduction: part de la production orientée export (0.70 = 70%)
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamEntreprises]')
      AND name = N'MargeReventeImport'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamEntreprises]
    ADD [MargeReventeImport] FLOAT NOT NULL DEFAULT 0.25;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamEntreprises]')
      AND name = N'PartExporteurProduction'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamEntreprises]
    ADD [PartExporteurProduction] FLOAT NOT NULL DEFAULT 0.70;
END
GO
