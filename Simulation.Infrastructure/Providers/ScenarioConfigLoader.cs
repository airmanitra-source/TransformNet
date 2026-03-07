using Household.Module.Models;
using Simulation.Module;
using Simulation.Module.Config;
using Simulation.Module.Models.Data;

namespace Simulation.Infrastructure.Providers;

/// <summary>
/// Charge un <see cref="ScenarioConfig"/> complet depuis la base de données
/// en lisant toutes les tables de paramétrage via <see cref="IScenarioRepository"/>
/// et en mappant les entités vers les propriétés de ScenarioConfig.
/// </summary>
public sealed class ScenarioConfigLoader : IScenarioConfigLoader
{
    private readonly IScenarioRepository _repo;

    public ScenarioConfigLoader(IScenarioRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<IScenarioReadModel>> ListScenariosAsync()
    {
        var scenarios = await _repo.GetAllScenariosAsync();
        return scenarios.Select(s => (IScenarioReadModel)new ScenarioReadModel(
            s.Id, s.Nom, s.Description, s.Version, s.EstBase, s.EstActif, s.CreePar, s.CreeAt, s.MisAJourAt));
    }

    public async Task<ScenarioConfig> LoadAsync(int scenarioId)
    {
        var scenario = await _repo.GetScenarioByIdAsync(scenarioId);
        if (scenario == null)
            throw new ArgumentException($"Scénario {scenarioId} introuvable.", nameof(scenarioId));

        // Démarrer avec la config de base Madagascar (toutes les valeurs par défaut)
        var config = ScenarioConfig.BaseMadagascar();
        config.Name = scenario.Nom;
        config.Description = scenario.Description;

        // ── Macro ────────────────────────────────────────────────────
        var macro = await _repo.GetParamMacroAsync(scenarioId);
        if (macro != null)
        {
            config.DureeJours = macro.DureeJours;
            config.NombreMenages = macro.NombreMenages;
            config.NombreEntreprises = macro.NombreEntreprises;
            config.PartMenagesUrbains = macro.PartMenagesUrbains;
            config.AideInternationaleJour = macro.AideInternationaleJour;
            config.SubventionJiramaJour = macro.SubventionJiramaJour;
            config.DepensesCapitalJour = macro.DepensesCapitalJour;
            config.InteretsDetteJour = macro.InteretsDetteJour;
            config.DettePubliqueInitiale = macro.DettePubliqueInitiale;
            config.DepensesPubliquesJour = macro.DepensesPubliquesJour;
            config.NombreFonctionnaires = macro.NombreFonctionnaires;
            config.SalaireMoyenFonctionnaireMensuel = macro.SalaireMoyenFonctionnaireMensuel;
            config.TauxReinvestissementPrive = macro.TauxReinvestissementPrive;
            config.InvestissementProductifActive = macro.InvestissementProductifActive;
            config.TauxDepreciationCapitalAnnuel = macro.TauxDepreciationCapitalAnnuel;
            config.SeuilUtilisationInvestissement = macro.SeuilUtilisationInvestissement;
            config.ElasticiteCapitalProductivite = macro.ElasticiteCapitalProductivite;
            config.InputOutputActivee = macro.InputOutputActivee;
            config.ValeurAutoconsommationJourBase = macro.ValeurAutoconsommationJourBase;
        }

        // ── Fiscalité ────────────────────────────────────────────────
        var fisc = await _repo.GetParamFiscaliteAsync(scenarioId);
        if (fisc != null)
        {
            config.TauxIS = fisc.TauxIS;
            config.TauxTVA = fisc.TauxTVA;
            config.TauxDirecteur = fisc.TauxDirecteur;
            config.TauxDroitsDouane = fisc.TauxDroitsDouane;
            config.TauxAccise = fisc.TauxAccise;
            config.TauxTaxeExport = fisc.TauxTaxeExport;
            config.TauxCotisationsPatronalesCNaPS = fisc.TauxCotisationsPatronalesCNaPS;
            config.TauxCotisationsSalarialesCNaPS = fisc.TauxCotisationsSalarialesCNaPS;
        }

        // ── Distribution Salariale ───────────────────────────────────
        var dist = await _repo.GetParamDistributionSalarialeAsync(scenarioId);
        if (dist != null)
        {
            config.SalaireMedian = dist.SalaireMedian;
            config.SalaireSigma = dist.Sigma;
            config.SalairePlancher = dist.SalairePlancher;
            config.PartSecteurInformel = dist.PartSecteurInformel;
        }

        // ── Banque ───────────────────────────────────────────────────
        var bank = await _repo.GetParamBanqueAsync(scenarioId);
        if (bank != null)
        {
            config.TauxInteretDepots = bank.TauxInteretDepots;
            config.TauxInteretCredits = bank.TauxInteretCredits;
            config.TauxReserveObligatoire = bank.TauxReserveObligatoire;
            config.PartDepotsAVue = bank.PartDepotsAVue;
            config.PartMonnaieCirculationDansM1 = bank.PartMonnaieCirculationDansM1;
            config.RatioM3SurM2 = bank.RatioM3SurM2;
            config.CroissanceCreditJour = bank.CroissanceCreditJour;
            config.PartCreditEntreprises = bank.PartCreditEntreprises;
            config.ProbabiliteDefautCreditJour = bank.ProbabiliteDefautCreditJour;
            config.TauxRecouvrementNPLJour = bank.TauxRecouvrementNPLJour;
            config.AvoirsExterieursNetsInitiaux = bank.AvoirsExterieursNetsInitiaux;
            config.CreancesNettesEtatInitiales = bank.CreancesNettesEtatInitiales;
            config.SCBInitial = bank.SCBInitial;
            config.IntensiteInterventionBFM = bank.IntensiteInterventionBFM;
            config.RatioExcedentSCBCible = bank.RatioExcedentSCBCible;
        }

        // ── Prix ─────────────────────────────────────────────────────
        var prix = await _repo.GetParamPrixAsync(scenarioId);
        if (prix != null)
        {
            config.PrixCarburantLitre = prix.PrixCarburantLitre;
            config.PrixCarburantReference = prix.PrixCarburantReference;
            config.ElasticitePrixParCarburant = prix.ElasticitePrixParCarburant;
            config.VolatiliteAleatoireMarche = prix.VolatiliteAleatoireMarche;
            config.PartRevenuAlimentaire = prix.PartRevenuAlimentaire;
            config.ElasticiteComportementMenage = prix.ElasticiteComportementMenage;
        }

        // ── Inflation ────────────────────────────────────────────────
        var infl = await _repo.GetParamInflationAsync(scenarioId);
        if (infl != null)
        {
            config.TauxInflation = infl.TauxInflationInitial;
            config.InflationEndogeneActivee = infl.InflationEndogeneActivee;
            config.NAIRU = infl.NAIRU;
            config.CoefficientPhillips = infl.CoefficientPhillips;
            config.ElasticiteCarburantInflation = infl.ElasticiteCarburantInflation;
            config.ElasticiteImportInflation = infl.ElasticiteImportInflation;
            config.ElasticiteChangeInflation = infl.ElasticiteChangeInflation;
            config.ElasticiteSalairesInflation = infl.ElasticiteSalairesInflation;
            config.CoefficientMonetaire = infl.CoefficientMonetaire;
            config.VitesseAdaptationAnticipations = infl.PoidsAnticipationsAdaptatives;
            config.InflationAncrage = infl.PoidsAncrageInflation;
        }

        // ── Taux de Change ───────────────────────────────────────────
        var change = await _repo.GetParamTauxChangeAsync(scenarioId);
        if (change != null)
        {
            config.TauxChangeDynamiqueActive = change.TauxChangeDynamiqueActive;
            config.TauxChangeMGAParUSD = change.TauxChangeMGAParUSD;
            config.ReservesBCMUSD = change.ReservesBCMUSD;
            config.ElasticiteChangeBalanceCommerciale = change.ElasticiteChangeBalanceCommerciale;
            config.PoidsChangePPA = change.PoidsChangePPA;
            config.IntensiteInterventionBCM = change.IntensiteInterventionBCM;
            config.ReservesMinimalesMoisImports = change.ReservesMinimalesMoisImports;
            config.DepreciationStructurelleAnnuelle = change.DepreciationStructurelleAnnuelle;
            config.InflationEtrangere = change.InflationEtrangere;
            config.ElasticiteRemittancesChange = change.ElasticiteRemittancesChange;
        }

        // ── Agriculture ──────────────────────────────────────────────
        var agri = await _repo.GetParamAgricultureAsync(scenarioId);
        if (agri != null)
        {
            config.ProbabiliteSecheresseJourSaison = agri.ProbabiliteSecheresseJourSaison;
            config.PartMenagesAffectesSecheresse = agri.PartMenagesRurauxAffectes;
            config.DureeSecheresseJours = agri.DureeSecheresseJoursBase;
            config.ReductionProductionAgricoleSecheresse = agri.ReductionProductionAgricole;
            config.AideAlimentaireSecheresseJourParMenage = agri.AideAlimentaireJourParMenage;
            config.ProbabiliteMigrationSecheresse = agri.ProbabiliteMigrationSaison;
        }

        // ── Cyclone ──────────────────────────────────────────────────
        var cyclone = await _repo.GetParamCycloneAsync(scenarioId);
        if (cyclone != null)
        {
            config.ProbabiliteCycloneJourSaison = cyclone.ProbabiliteCycloneJourSaison;
            config.ProbabiliteCycloneJourHorsSaison = cyclone.ProbabiliteCycloneJourHorsSaison;
        }

        // ── Transport ────────────────────────────────────────────────
        var transport = await _repo.GetParamTransportAsync(scenarioId);
        if (transport != null)
        {
            config.CoutTransportPaiementJirama = transport.CoutTransportPaiementJirama;
            config.PartInformelTransportPublic = transport.PartInformelTransportPublic;
            config.PartFormelCarburant = transport.PartFormelCarburant;
            config.PartInformelEntretien = transport.PartInformelEntretien;
            config.EntretienVoitureJour = transport.EntretienVoitureJour;
            config.EntretienFractionRevenuVoiture = transport.EntretienFractionRevenuVoiture;
        }

        // ── Santé ────────────────────────────────────────────────────
        var sante = await _repo.GetParamSanteAsync(scenarioId);
        if (sante != null)
        {
            config.TauxOccupationHopitaux = sante.TauxOccupationHopitaux;
            config.CoutConsultationSanteBase = sante.CoutConsultationBase;
            config.CoutHospitalisationSanteBase = sante.CoutHospitalisationBase;
            config.PartFormelleDepenseSante = sante.PartFormelleDepenseSante;
        }

        // ── Éducation ────────────────────────────────────────────────
        var edu = await _repo.GetParamEducationAsync(scenarioId);
        if (edu != null)
        {
            config.NombreEnfantsMoyenParMenage = edu.NombreEnfantsMoyenParMenage;
            config.PartEnfantsScolarises = edu.PartEnfantsScolarises;
            config.DureeDepenseEducationJours = edu.DureeDepenseEducationJours;
            config.CoutEducationJournalierParEnfant = edu.CoutEducationJournalierParEnfant;
            config.PartFormelleDepenseEducation = edu.PartFormelleDepenseEducation;
        }

        // ── Entreprises ──────────────────────────────────────────────
        var ent = await _repo.GetParamEntreprisesAsync(scenarioId);
        if (ent != null)
        {
            config.PartEntreprisesAgricoles = ent.PartEntreprisesAgricoles;
            config.PartEntreprisesConstruction = ent.PartEntreprisesConstruction;
            config.PartEntreprisesHotellerieTourisme = ent.PartEntreprisesHotellerieTourisme;
            config.MargeBeneficiaireEntreprise = ent.MargeBeneficiaireEntreprise;
            config.ProductiviteParEmploye = ent.ProductiviteParEmployeJourDefaut;
            config.MargeReventeImport = ent.MargeReventeImport;
            config.PartExporteurProduction = ent.PartExporteurProduction;
        }

        // ── JIRAMA ───────────────────────────────────────────────────
        var jirama = await _repo.GetParamJiramaAsync(scenarioId);
        if (jirama != null)
        {
            config.PrixElectriciteArKWh = jirama.PrixElectriciteArKWh;
            config.ConsommationElecMenageKWhJour = jirama.ConsommationElecMenageKWhJour;
            config.ConsommationElecParEmployeKWhJour = jirama.ConsommationElecParEmployeKWhJour;
            config.ConsommationElecEtatKWhJour = jirama.ConsommationElecEtatKWhJour;
            config.PartProductionHydraulique = jirama.PartProductionHydraulique;
            config.TauxPertesDistribution = jirama.TauxPertesDistribution;
            config.PartConsommationElecMenages = jirama.PartConsommationElecMenages;
            config.TarifEauJourMenage = jirama.TarifEauJourMenage;
            config.TauxAccesEau = jirama.TauxAccesEau;
            config.TauxAccesElectricite = jirama.TauxAccesElectricite;
            config.JiramaTresorerieInitiale = jirama.TresorerieInitiale;
            config.JiramaNombreEmployesBase = jirama.NombreEmployesBase;
            config.JiramaSalaireMoyenMensuelEmploye = jirama.SalaireMoyenMensuelEmploye;
        }

        // ── Commerce ─────────────────────────────────────────────────
        var commerce = await _repo.GetParamCommerceAsync(scenarioId);
        if (commerce != null)
        {
            config.RemittancesJour = commerce.RemittancesJour;
            config.ConsommationRizAnnuelleKgParPersonne = commerce.ConsommationRizAnnuelleKgParPersonne;
            config.PrixRizLocalKg = commerce.PrixRizLocalKg;
            config.PrixRizImporteKg = commerce.PrixRizImporteKg;
            config.PartRizImporte = commerce.PartRizImporte;
        }

        // ── Immobilier ───────────────────────────────────────────────
        var immo = await _repo.GetParamImmobilierAsync(scenarioId);
        if (immo != null)
        {
            config.LoyerImputeJourParMenage = immo.LoyerImputeJourParMenage;
            config.TauxMenagesProprietaires = immo.TauxMenagesProprietaires;
            config.LoyerJourLocataire = immo.LoyerJourLocataire;
            config.ProbabiliteConstructionMaisonLocataire = immo.ProbabiliteConstructionMaisonLocataire;
            config.DureeConstructionMaisonJours = immo.DureeConstructionMaisonJours;
            config.BudgetConstructionMaisonJour = immo.BudgetConstructionMaisonJour;
            config.PartBudgetConstructionBTP = immo.PartBudgetConstructionBTP;
            config.PartBudgetConstructionQuincaillerie = immo.PartBudgetConstructionQuincaillerie;
            config.PartBudgetConstructionTransportInformel = immo.PartBudgetConstructionTransportInformel;
        }

        // ── Secteurs d'activité (productivité, trésorerie, probabilité informel) ─────────
        var secteurs = await _repo.GetParamSecteursActiviteAsync(scenarioId);
        var secteursListe = secteurs.ToList();
        if (secteursListe.Count > 0)
        {
            foreach (var s in secteursListe)
            {
                var secteur = (Company.Module.Models.ESecteurActivite)s.Secteur;
                config.ProbabiliteInformelParSecteur[secteur] = s.ProbabiliteInformel;
                config.MargeBeneficiaireParSecteur[secteur] = s.MargeBeneficiaire;
                if (s.TresorerieInitiale > 0)
                    config.TresorerieInitialeParSecteur[secteur] = s.TresorerieInitiale;
            }
        }

        // ── Comportements par Classe ─────────────────────────────────
        var comportements = await _repo.GetParamComportementMenageAsync(scenarioId);
        var comportementsList = comportements.ToList();
        if (comportementsList.Count > 0)
        {
            config.ComportementsParClasse = comportementsList.Select(c => new ComportementClasseConfig
            {
                Classe = (ClasseSocioEconomique)c.Classe,
                TauxEpargneMin = c.TauxEpargneMin,
                TauxEpargneMax = c.TauxEpargneMax,
                PropensionConsommationMin = c.PropensionConsommationMin,
                PropensionConsommationMax = c.PropensionConsommationMax,
                EpargneInitialeMax = c.EpargneInitialeMax,
                DepensesAlimentairesJourMin = c.DepensesAlimentairesJourMin,
                DepensesAlimentairesJourMax = c.DepensesAlimentairesJourMax,
                DepensesDiversJourMin = c.DepensesDiversJourMin,
                DepensesDiversJourMax = c.DepensesDiversJourMax,
                ProbabiliteEmploiMin = c.ProbabiliteEmploiMin,
                ProbabiliteEmploiMax = c.ProbabiliteEmploiMax,
                ModeTransportPreferentiel = (Company.Module.Models.ModeTransport)c.ModeTransportPreferentiel,
                DistanceDomicileTravailKmMin = c.DistanceDomicileTravailKmMin,
                DistanceDomicileTravailKmMax = c.DistanceDomicileTravailKmMax,
                BudgetSortieWeekendMin = c.BudgetSortieWeekendMin,
                BudgetSortieWeekendMax = c.BudgetSortieWeekendMax,
                BudgetVacancesMin = c.BudgetVacancesMin,
                BudgetVacancesMax = c.BudgetVacancesMax,
                ProbabiliteSortieWeekendMin = c.ProbabiliteSortieWeekendMin,
                ProbabiliteSortieWeekendMax = c.ProbabiliteSortieWeekendMax,
                FrequenceVacancesJours = c.FrequenceVacancesJours,
                ProbabiliteVacancesMin = c.ProbabiliteVacancesMin,
                ProbabiliteVacancesMax = c.ProbabiliteVacancesMax,
                DureeVacancesJoursMin = c.DureeVacancesJoursMin,
                DureeVacancesJoursMax = c.DureeVacancesJoursMax,
            }).ToList();
        }

        return config;
    }
}
