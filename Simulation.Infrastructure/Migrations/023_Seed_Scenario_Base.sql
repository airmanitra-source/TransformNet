-- ============================================================
-- 023 - Seed : scénario de base Madagascar
-- Insère le scénario de référence (baseline Madagascar) et
-- l'ensemble des paramètres par défaut dans toutes les tables.
-- À exécuter après les migrations 001-022.
-- ============================================================

-- Guard : n'insérer que si le scénario baseline n'existe pas encore
IF NOT EXISTS (SELECT 1 FROM simulation.sim.[Scenarios] WHERE [EstBase] = 1)
BEGIN
    -- ── Scénario master ──────────────────────────────────────
    INSERT INTO simulation.sim.[Scenarios] ([Nom],[Description],[Version],[EstBase],[CreePar])
    VALUES (
        N'Scénario de base Madagascar',
        N'Paramètres macroéconomiques calibrés sur les réalités de Madagascar (INSTAT, BCM, Banque Mondiale 2024)',
        1, 1, N'seed'
    );

    DECLARE @sid INT = SCOPE_IDENTITY();

    -- ── ParamMacro ────────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamMacro] ([ScenarioId],[DureeJours],[NombreMenages],[NombreEntreprises],
        [PartMenagesUrbains],[AideInternationaleJour],[SubventionJiramaJour],[DepensesCapitalJour],
        [InteretsDetteJour],[DettePubliqueInitiale],[DepensesPubliquesJour],[TauxRedistribution],
        [NombreFonctionnaires],[SalaireMoyenFonctionnaireMensuel],[TauxReinvestissementPrive])
    VALUES (@sid,365,100000,50000,0.30,3704000000,1370000000,13526000000,
        1678000000,29250000000000,3218000000,0.15,
        350000,863000,0.25);

    -- ── ParamFiscalite ────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamFiscalite] ([ScenarioId],[TauxIS],[TauxTVA],[TauxDirecteur],
        [TauxDroitsDouane],[TauxAccise],[TauxTaxeExport],
        [TauxCotisationsPatronalesCNaPS],[TauxCotisationsSalarialesCNaPS])
    VALUES (@sid,0.20,0.20,0.09,0.12,0.10,0.03,0.18,0.01);

    -- ── ParamIRSATranches ─────────────────────────────────────
    INSERT INTO simulation.sim.[ParamIRSATranches] ([ScenarioId],[Ordre],[SeuilMin],[Taux],[Description]) VALUES
        (@sid,1,0,       0.00,N'Exonéré (≤ 350 000 MGA)'),
        (@sid,2,350001,  0.05,N'5% (350 001 – 400 000)'),
        (@sid,3,400001,  0.10,N'10% (400 001 – 500 000)'),
        (@sid,4,500001,  0.15,N'15% (500 001 – 600 000)'),
        (@sid,5,600001,  0.20,N'20% (> 600 000)');

    -- ── ParamDistributionSalariale ────────────────────────────
    INSERT INTO simulation.sim.[ParamDistributionSalariale] ([ScenarioId],[SalaireMedian],[Sigma],
        [SalairePlancher],[SalairePlafond],[PartSecteurInformel])
    VALUES (@sid,170000,0.85,50000,10000000,0.85);

    -- ── ParamComportementMenage ───────────────────────────────
    -- Subsistance
    INSERT INTO simulation.sim.[ParamComportementMenage]
        ([ScenarioId],[Classe],[ClasseLibelle],[TauxEpargneMin],[TauxEpargneMax],
         [PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
         [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
         [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
         [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax]) VALUES
        (@sid,0,'Subsistance',0.02,0.05,0.92,0.98,20000,
         2000,3500,500,1300,0.70,0.80,1,3,8);
    -- InformelBas
    INSERT INTO simulation.sim.[ParamComportementMenage]
        ([ScenarioId],[Classe],[ClasseLibelle],[TauxEpargneMin],[TauxEpargneMax],
         [PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
         [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
         [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
         [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax]) VALUES
        (@sid,1,'InformelBas',0.04,0.08,0.85,0.93,50000,
         3000,5000,800,2000,0.78,0.88,1,5,13);
    -- FormelBas
    INSERT INTO simulation.sim.[ParamComportementMenage]
        ([ScenarioId],[Classe],[ClasseLibelle],[TauxEpargneMin],[TauxEpargneMax],
         [PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
         [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
         [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
         [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax],
         [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],[ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax]) VALUES
        (@sid,2,'FormelBas',0.08,0.14,0.75,0.85,120000,
         4000,6500,1500,3000,0.88,0.96,2,5,17,
         3000,5000,0.15,0.25);
    -- FormelQualifie
    INSERT INTO simulation.sim.[ParamComportementMenage]
        ([ScenarioId],[Classe],[ClasseLibelle],[TauxEpargneMin],[TauxEpargneMax],
         [PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
         [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
         [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
         [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax],
         [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],[BudgetVacancesMin],[BudgetVacancesMax],
         [ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax],
         [FrequenceVacancesJours],[ProbabiliteVacancesMin],[ProbabiliteVacancesMax],
         [DureeVacancesJoursMin],[DureeVacancesJoursMax]) VALUES
        (@sid,3,'FormelQualifie',0.12,0.20,0.65,0.77,400000,
         5000,8000,2500,5500,0.92,0.98,3,8,23,
         10000,20000,15000,25000,0.40,0.60,
         180,0.40,0.60,3,5);
    -- Cadre
    INSERT INTO simulation.sim.[ParamComportementMenage]
        ([ScenarioId],[Classe],[ClasseLibelle],[TauxEpargneMin],[TauxEpargneMax],
         [PropensionConsommationMin],[PropensionConsommationMax],[EpargneInitialeMax],
         [DepensesAlimentairesJourMin],[DepensesAlimentairesJourMax],[DepensesDiversJourMin],[DepensesDiversJourMax],
         [ProbabiliteEmploiMin],[ProbabiliteEmploiMax],[ModeTransportPreferentiel],
         [DistanceDomicileTravailKmMin],[DistanceDomicileTravailKmMax],
         [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],[BudgetVacancesMin],[BudgetVacancesMax],
         [ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax],
         [FrequenceVacancesJours],[ProbabiliteVacancesMin],[ProbabiliteVacancesMax],
         [DureeVacancesJoursMin],[DureeVacancesJoursMax]) VALUES
        (@sid,4,'Cadre',0.18,0.30,0.50,0.65,2500000,
         8000,15000,5000,15000,0.95,0.99,3,10,30,
         25000,50000,40000,70000,0.60,0.85,
         90,0.65,0.85,4,7);

    -- ── ParamBanque ───────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamBanque] ([ScenarioId],[TauxInteretDepots],[TauxInteretCredits],
        [TauxReserveObligatoire],[PartDepotsAVue],[PartMonnaieCirculationDansM1],[RatioM3SurM2],
        [CroissanceCreditJour],[PartCreditEntreprises],[ProbabiliteDefautCreditJour],[TauxRecouvrementNPLJour],
        [AvoirsExterieursNetsInitiaux],[CreancesNettesEtatInitiales],[SCBInitial],
        [IntensiteInterventionBFM],[RatioExcedentSCBCible])
    VALUES (@sid,0.045,0.16,0.13,0.55,0.45,1.10,0.00041,0.75,0.0003,0.002,
        11250000000000,3000000000000,2430000000000,0.50,0.055);

    -- ── ParamPrix ─────────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamPrix] ([ScenarioId],[PrixCarburantLitre],[PrixCarburantReference],
        [ElasticitePrixParCarburant],[VolatiliteAleatoireMarche],
        [PartRevenuAlimentaire],[ElasticiteComportementMenage])
    VALUES (@sid,5500,5500,0.70,0.10,0.40,0.65);

    -- ── ParamInflation ────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamInflation] ([ScenarioId],[TauxInflationInitial],[InflationEndogeneActivee],
        [NAIRU],[CoefficientPhillips],[ElasticiteCarburantInflation],[ElasticiteImportInflation],
        [ElasticiteChangeInflation],[ElasticiteSalairesInflation],[CoefficientMonetaire],
        [PoidsAnticipationsAdaptatives],[PoidsAncrageInflation])
    VALUES (@sid,0.08,1,0.175,0.25,0.15,0.25,0.30,0.10,0.20,0.70,0.30);

    -- ── ParamTauxChange ───────────────────────────────────────
    INSERT INTO simulation.sim.[ParamTauxChange] ([ScenarioId],[TauxChangeDynamiqueActive],[TauxChangeMGAParUSD],
        [ReservesBCMUSD],[ElasticiteChangeBalanceCommerciale],[PoidsChangePPA],
        [IntensiteInterventionBCM],[ReservesMinimalesMoisImports],[DepreciationStructurelleAnnuelle],
        [InflationEtrangere],[ElasticiteRemittancesChange])
    VALUES (@sid,1,4500,2500000000,0.50,0.30,0.50,3.0,0.05,0.03,0.50);

    -- ── ParamAgriculture ──────────────────────────────────────
    INSERT INTO simulation.sim.[ParamAgriculture] ([ScenarioId],[ProbabiliteSecheresseJourSaison],
        [PartMenagesRurauxAffectes],[DureeSecheresseJoursBase],[ReductionProductionAgricole],
        [AideAlimentaireJourParMenage],[ProbabiliteMigrationSaison],[ValeurAutoconsommationJourBase])
    VALUES (@sid,0.001,0.08,120,0.60,3000,0.12,2500);

    -- ── ParamCyclone ──────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamCyclone] ([ScenarioId],[ProbabiliteCycloneJourSaison],
        [ProbabiliteCycloneJourHorsSaison],[DureeCycloneJoursMin],[DureeCycloneJoursMax],
        [BudgetTotalReconstructionBase],[DureeReconstructionJoursMin],[DureeReconstructionJoursMax],
        [PartMenagesAffectesMin],[PartMenagesAffectesMax],[DelaiMinEntreDeuxCyclones])
    VALUES (@sid,0.003,0.0002,3,7,650000,45,120,0.05,0.30,30);

    -- ── ParamTransport ────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamTransport] ([ScenarioId],[PartInformelTransportPublic],
        [PartFormelCarburant],[PartInformelEntretien],[CoutTaxiBe],[EntretienVoitureJour],
        [EntretienFractionRevenuVoiture],[ConsoMotoLitrePour100km],[ConsoVoitureLitrePour100km],
        [CoutTransportPaiementJirama])
    VALUES (@sid,0.70,0.60,0.90,600,500,0.15,3.0,8.0,1200);

    -- ── ParamSante ────────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamSante] ([ScenarioId],[TauxOccupationHopitaux],
        [CoutConsultationBase],[CoutHospitalisationBase],[PartFormelleDepenseSante],
        [ProbabiliteHospitalisationBase])
    VALUES (@sid,0.68,8000,45000,0.70,0.10);

    -- ── ParamEducation ────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamEducation] ([ScenarioId],[NombreEnfantsMoyenParMenage],
        [PartEnfantsScolarises],[DureeDepenseEducationJours],[CoutEducationJournalierParEnfant],
        [PartFormelleDepenseEducation])
    VALUES (@sid,2.3,0.72,180,900,0.75);

    -- ── ParamLoisirs ──────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamLoisirs] ([ScenarioId],[Classe],[ClasseLibelle],
        [BudgetSortieWeekendMin],[BudgetSortieWeekendMax],
        [BudgetVacancesMin],[BudgetVacancesMax],
        [ProbabiliteSortieWeekendMin],[ProbabiliteSortieWeekendMax],
        [FrequenceVacancesJours],[ProbabiliteVacancesMin],[ProbabiliteVacancesMax],
        [DureeVacancesJoursMin],[DureeVacancesJoursMax],[SensibiliteInflation],[SeuilInflationReaction]) VALUES
        (@sid,0,'Subsistance',        0,0,0,0,0,0,0,0,0,0,0,0.15,8.0),
        (@sid,1,'InformelBas',        0,0,0,0,0,0,0,0,0,0,0,0.15,8.0),
        (@sid,2,'FormelBas',          3000,5000,0,0,0.15,0.25,0,0,0,0,0,0.20,8.0),
        (@sid,3,'FormelQualifie',     10000,20000,15000,25000,0.40,0.60,180,0.40,0.60,3,5,0.25,8.0),
        (@sid,4,'Cadre',              25000,50000,40000,70000,0.60,0.85,90,0.65,0.85,4,7,0.30,8.0);

    -- ── ParamEntreprises ──────────────────────────────────────
    INSERT INTO simulation.sim.[ParamEntreprises] ([ScenarioId],[PartEntreprisesAgricoles],
        [PartEntreprisesConstruction],[PartEntreprisesHotellerieTourisme],
        [MargeBeneficiaireEntreprise],[ProductiviteParEmployeJourDefaut],[PartB2B],
        [FacteurProductiviteInformelMin],[FacteurProductiviteInformelMax],
        [SeuilJoursStressTresorerie],[SeuilJoursDemandeExcedentaire],[SalaireMoyenMensuelDefaut])
    VALUES (@sid,0.30,0.05,0.03,0.20,15000,0.30,0.30,0.60,30,15,200000);

    -- ── ParamSecteurActivite ──────────────────────────────────
    INSERT INTO simulation.sim.[ParamSecteurActivite] ([ScenarioId],[Secteur],[SecteurLibelle],
        [ProductiviteJourMoyenne],[ProductiviteJourBasse],[TresorerieInitiale],[NombreEmployesDefaut]) VALUES
        (@sid,0,'Agriculture',       3500,  2000,  500000,  5),
        (@sid,1,'Textiles',         41000, 27000,4000000, 50),
        (@sid,2,'Commerces',        91000, 73000,8000000, 10),
        (@sid,3,'Services',         91000, 73000,6000000, 10),
        (@sid,4,'SecteurMinier',   274000,200000,20000000,300),
        (@sid,5,'Construction',     25000, 15000,2000000, 15),
        (@sid,6,'HotellerieTourisme',60000,40000,5000000, 20);

    -- ── ParamJirama ───────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamJirama] ([ScenarioId],[PrixElectriciteArKWh],[ConsommationElecMenageKWhJour],
        [ConsommationElecParEmployeKWhJour],[ConsommationElecEtatKWhJour],[PartProductionHydraulique],
        [TauxPertesDistribution],[PartConsommationElecMenages],[TarifEauJourMenage],
        [TauxAccesEau],[TauxAccesElectricite])
    VALUES (@sid,653,1.03,2.5,44400,0.516,0.289,0.474,500,0.25,0.30);

    -- ── ParamCommerce ─────────────────────────────────────────
    INSERT INTO simulation.sim.[ParamCommerce] ([ScenarioId],[RemittancesJour],
        [ConsommationRizAnnuelleKgParPersonne],[PrixRizLocalKg],[PrixRizImporteKg],[PartRizImporte])
    VALUES (@sid,7400000000,130,2400,2800,0.18);

    -- ── ParamImmobilier ───────────────────────────────────────
    INSERT INTO simulation.sim.[ParamImmobilier] ([ScenarioId],[LoyerImputeJourParMenage],[TauxMenagesProprietaires],
        [LoyerJourLocataire],[ProbabiliteConstructionMaisonLocataire],[DureeConstructionMaisonJours],
        [BudgetConstructionMaisonJour],[PartBudgetConstructionBTP],[PartBudgetConstructionQuincaillerie],
        [PartBudgetConstructionTransportInformel])
    VALUES (@sid,1000,0.65,3500,0.08,240,7500,0.55,0.30,0.15);

END
GO
