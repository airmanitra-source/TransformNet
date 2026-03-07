-- ============================================================
-- 029 - Seed UPDATE : Jirama agent fields + marges par secteur
-- ============================================================

-- 1. Mise à jour ParamJirama
UPDATE simulation.sim.[ParamJirama]
SET
    [TresorerieInitiale]         = 10000000.0,
    [NombreEmployesBase]         = 5000,
    [SalaireMoyenMensuelEmploye] = 350000.0,
    [MisAJourAt]                 = SYSUTCDATETIME()
WHERE [ScenarioId] IN (SELECT [Id] FROM simulation.sim.[Scenarios] WHERE [EstBase] = 1);

-- 2. Mise à jour ParamSecteurActivite
UPDATE simulation.sim.[ParamSecteurActivite]
SET
    [MargeBeneficiaire] = CASE [Secteur]
        WHEN 0 THEN 0.10   -- Agriculture
        WHEN 1 THEN 0.15   -- Textiles
        WHEN 2 THEN 0.18   -- Commerces
        WHEN 3 THEN 0.20   -- Services
        WHEN 4 THEN 0.35   -- SecteurMinier
        WHEN 5 THEN 0.20   -- Construction
        WHEN 6 THEN 0.25   -- HotellerieTourisme
        ELSE 0.20
    END,
    [MisAJourAt] = SYSUTCDATETIME()
WHERE [ScenarioId] IN (SELECT [Id] FROM simulation.sim.[Scenarios] WHERE [EstBase] = 1);
GO
