-- ============================================================
-- 025 - ALTER sim.ParamSecteurActivite
-- Ajout de la colonne manquante qui était hardcodée dans
-- SimulationModule.cs via un switch expression :
--   Agriculture 0.95 / Commerces 0.80 / Construction 0.75 /
--   SecteurMinier 0.20 / HotellerieTourisme 0.40 / autres 0.70
--
-- ProbabiliteInformel : probabilité qu'une entreprise de ce
-- secteur soit dans le secteur informel (source INSTAT EPM).
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamSecteurActivite]')
      AND name = N'ProbabiliteInformel'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamSecteurActivite]
    ADD [ProbabiliteInformel] FLOAT NOT NULL DEFAULT 0.70;
END
GO
