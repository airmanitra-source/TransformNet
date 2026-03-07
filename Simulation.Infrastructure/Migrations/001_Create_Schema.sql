-- ============================================================
-- 001 - Création du schéma sim
-- Toutes les tables de paramétrage du simulateur économique
-- sont regroupées sous le schéma simulation.sim pour l'isolation.
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'sim')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA simulation.sim';
END
GO
