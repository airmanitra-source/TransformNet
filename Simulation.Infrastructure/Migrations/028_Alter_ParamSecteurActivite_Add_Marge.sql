-- ============================================================
-- 028 - ALTER sim.ParamSecteurActivite
-- Ajout de la colonne MargeBeneficiaire par secteur.
-- Était hardcodée dans SimulationModule.cs :
--   secteur == HotellerieTourisme ? 0.25 : MargeBeneficiaireEntreprise
-- Chaque secteur peut maintenant avoir sa propre marge.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'simulation.sim.[ParamSecteurActivite]')
      AND name = N'MargeBeneficiaire'
)
BEGIN
    ALTER TABLE simulation.sim.[ParamSecteurActivite]
    ADD [MargeBeneficiaire] FLOAT NOT NULL DEFAULT 0.20;
END
GO
