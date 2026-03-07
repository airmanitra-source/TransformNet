-- Mise à jour directe via une sous-requête pour éviter les problèmes de portée (scope)
BEGIN TRANSACTION;

    -- 1. Mise à jour ParamEntreprises
    UPDATE simulation.sim.[ParamEntreprises]
    SET
        [MargeReventeImport]      = 0.25,
        [PartExporteurProduction] = 0.70,
        [MisAJourAt]              = SYSUTCDATETIME()
    WHERE [ScenarioId] = (SELECT [Id] FROM simulation.sim.[Scenarios] WHERE [EstBase] = 1);

    -- 2. Mise à jour ParamSecteurActivite
    UPDATE simulation.sim.[ParamSecteurActivite]
    SET [ProbabiliteInformel] = CASE [Secteur]
        WHEN 0 THEN 0.95   -- Agriculture
        WHEN 1 THEN 0.65   -- Textiles
        WHEN 2 THEN 0.80   -- Commerces
        WHEN 3 THEN 0.70   -- Services
        WHEN 4 THEN 0.20   -- SecteurMinier
        WHEN 5 THEN 0.75   -- Construction
        WHEN 6 THEN 0.40   -- HotellerieTourisme
        ELSE 0.70
    END,
    [MisAJourAt] = SYSUTCDATETIME()
    WHERE [ScenarioId] = (SELECT [Id] FROM simulation.sim.[Scenarios] WHERE [EstBase] = 1);

COMMIT;
GO
