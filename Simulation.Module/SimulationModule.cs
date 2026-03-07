using AgentCompany = Company.Module.Models.Company;
using AgentHousehold = Household.Module.Models.Household;
using AgentGovernment = Government.Module.Models.Government;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentJirama = Company.Module.Models.Jirama;
using Company.Module.Models;
using Household.Module.Models;
using Simulation.Module.Config;
using Government.Module;
using Household.Module;
using Household.Salary.Distribution.Module;
using Company.Module;
using Household.Leisure.Spending.Module;
using Household.Remittance.Module;
using Price.Module;
using Bank.Module;
using Transportation.Module;
using Simulation.Module.Models;

namespace Simulation.Module;

public class SimulationModule : ISimulationModule
{
    private readonly IGovernmentModule _governmentModule;
    private readonly IHouseholdModule _householdModule;
    private readonly ICompanyModule _companyModule;
    private readonly IPriceModule _priceModule;
    private readonly IHouseholdSalaryDistributionModule _householdSalaryDistributionModule;
    private readonly IHouseholdLeisureSpendingModule _householdLeisureSpendingModule;
    private readonly IHouseholdRemittanceModule _householdRemittanceModule;
    private readonly IBankModule _bankModule;
    private readonly ITransportationModule _transportationModule;
    private readonly List<AgentHousehold> _menages = [];
    private readonly List<AgentCompany> _entreprises = [];
    private readonly List<AgentImporter> _importateurs = [];
    private readonly List<AgentExporter> _exportateurs = [];
    private AgentJirama _jirama = new();
    private AgentGovernment _etat = new();
    private Bank.Module.Models.Bank _banque = new();
    private ScenarioConfig _config = new();
    private SimulationResult _result = new();
    private readonly List<AgentCompany> _entreprisesTourisme = [];
    private CancellationTokenSource? _cts;
    private int _jourCourant;
    private double _facteurEchelle = 1.0;
    private DistributionStats _statsInitiales = new();
    private Random _random = new Random();

    public SimulationResult Result => _result;
    public bool EnCours => _result.EnCours;
    public int JourCourant => _jourCourant;
    public DistributionStats StatsInitiales => _statsInitiales;

    public event Action? OnTickCompleted;

    public SimulationModule(
        IGovernmentModule governmentModule,
        IHouseholdModule householdModule,
        IHouseholdSalaryDistributionModule householdSalaryDistributionModule,
        ICompanyModule companyModule,
        IPriceModule priceModule,
        IHouseholdLeisureSpendingModule householdLeisureSpendingModule,
        IHouseholdRemittanceModule householdRemittanceModule,
        IBankModule bankModule,
        ITransportationModule transportationModule)
    {
        _governmentModule = governmentModule;
        _householdModule  = householdModule;
        _householdSalaryDistributionModule = householdSalaryDistributionModule;
        _companyModule    = companyModule;
        _priceModule      = priceModule;
        _householdLeisureSpendingModule = householdLeisureSpendingModule;
        _householdRemittanceModule = householdRemittanceModule;
        _bankModule = bankModule;
        _transportationModule = transportationModule;
    }

    /// <summary>
    /// Initialise la simulation avec un scénario donné.
    /// </summary>
    public void Initialiser(ScenarioConfig config)
    {
        _config = config;
        _jourCourant = 0;
        
        // Facteur d'échelle : ratio entre la simulation et la réalité malgache
        // Tous les paramètres macro absolus (aide, subventions, fonctionnaires, dépenses publiques)
        // sont calibrés pour ~6M ménages et seront mis à l'échelle proportionnellement
        _facteurEchelle = _config.NombreMenages / ScenarioConfig.NombreMenagesReference;

        AgentHousehold.ResetIdCounter();
        AgentCompany.ResetIdCounter();

        _householdSalaryDistributionModule.ConfigurerDistributionSalariale(
            _config.SalaireMedian,
            _config.SalaireSigma,
            _config.SalairePlancher,
            _config.PartSecteurInformel);

        // Créer les agents
        _menages.Clear();
        _entreprises.Clear();
        _importateurs.Clear();
        _exportateurs.Clear();

        // Créer la Jirama (compagnie monopole eau + électricité)
        _jirama = new AgentJirama
        {
            Name = "Jiram-Agent",
            TarifEauJour = _config.TarifEauJourMenage,
            PrixElectriciteArKWh = _config.PrixElectriciteArKWh,
            ConsommationElecMenageKWhJour = _config.ConsommationElecMenageKWhJour,
            PartProductionHydraulique = _config.PartProductionHydraulique,
            TauxPertesDistribution = _config.TauxPertesDistribution,
            PartConsommationMenages = _config.PartConsommationElecMenages,
            SecteurActivite = ESecteurActivite.Services,
            Tresorerie = 10_000_000 * _facteurEchelle,
            NombreEmployes = Math.Max(1, (int)(5_000 * _facteurEchelle)),
            SalaireMoyenMensuel = 350_000
        };

        // Créer l'État avec les paramètres TOFE
        _etat = new AgentGovernment
        {
            TauxIS = _config.TauxIS,
            TauxTVA = _config.TauxTVA,
            TauxDirecteur = _config.TauxDirecteur,
            TauxInflation = _config.TauxInflation,
            DepensesPubliquesJour = _config.DepensesPubliquesJour * _facteurEchelle,
            DettePublique = _config.DettePubliqueInitiale * _facteurEchelle
        };

        // Créer la Banque
        _banque = new Bank.Module.Models.Bank 
        {
            TauxReserveObligatoire = _config.TauxReserveObligatoire,
            TotalCreditsAccordes = 0
        };

        // Créer les entreprises (normales, importateurs, exportateurs)
      

        // 1 importateur agrégé par catégorie INSTAT, 1 exportateur agrégé par catégorie INSTAT
        // Chaque agent porte la moyenne globale INSTAT de sa catégorie → cohérence macroéconomique
        var categoriesImport = Enum.GetValues<ECategorieImport>();
        var categoriesExport = Enum.GetValues<ECategorieExport>();
        int nbEntreprisesNormales = _config.NombreEntreprises;

        int totalEntreprises = nbEntreprisesNormales + categoriesImport.Length + categoriesExport.Length;
        int employesParEntreprise = _config.NombreMenages / Math.Max(totalEntreprises, 1);
        var random = new Random(42);
        var secteursLocaux = new[]
        {
            ESecteurActivite.Agriculture,
            ESecteurActivite.Services,
            ESecteurActivite.Commerces,
            ESecteurActivite.Textiles,
            ESecteurActivite.SecteurMinier,
            ESecteurActivite.Construction
        };

        // 1 importateur agrégé par catégorie INSTAT
        foreach (var categorie in categoriesImport)
        {
            double cifJourCategorie = _config.CIFJourParCategorie.GetValueOrDefault(categorie, 500d);
            double cifJourMGA = cifJourCategorie * 1_000_000 * _facteurEchelle;

            var imp = new AgentImporter
            {
                Name = $"Import-{categorie}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _householdSalaryDistributionModule.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                // was: AgentCompany.GetProductiviteParSecteur(ESecteurActivite.Commerces)
                ProductiviteParEmployeJour = _companyModule.GetProductiviteParSecteur(ESecteurActivite.Commerces) * (0.8 + random.NextDouble() * 0.4),
                // was: AgentCompany.GetTresorerieInitialeParSecteur(ESecteurActivite.Commerces, _config.TresorerieInitialeParSecteur)
                Tresorerie = _companyModule.GetTresorerieInitiale(ESecteurActivite.Commerces, _config.TresorerieInitialeParSecteur)
                             * (0.5 + random.NextDouble()),  // pondération aléatoire
                Categorie = categorie,
                SecteurActivite = ESecteurActivite.Commerces,
                ValeurCIFJour = cifJourMGA,
                MargeReventeImport = 0.25
            };
            _importateurs.Add(imp);
        }

        // 1 exportateur agrégé par catégorie INSTAT
        foreach (var categorie in categoriesExport)
        {
            var secteur = SecteurDepuisCategorieExport(categorie);
            double fobJourCategorie = _config.FOBJourParCategorie.GetValueOrDefault(categorie, 200d);
            double fobJourMGA = fobJourCategorie * 1_000_000 * _facteurEchelle;

            var exp = new AgentExporter
            {
                Name = $"Export-{categorie}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _householdSalaryDistributionModule.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                // was: AgentCompany.GetProductiviteParSecteur(secteur)
                ProductiviteParEmployeJour = _companyModule.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                // was: AgentCompany.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur)
                Tresorerie = _companyModule.GetTresorerieInitiale(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),  // pondération aléatoire
                Categorie = categorie,
                SecteurActivite = secteur,
                ValeurFOBJour = fobJourMGA,
                PartExport = 0.70
            };
            _exportateurs.Add(exp);
        }

        // Entreprises normales (marché local)
        int nbAgricoles = (int)(nbEntreprisesNormales * _config.PartEntreprisesAgricoles);
        int nbConstruction = (int)(nbEntreprisesNormales * _config.PartEntreprisesConstruction);
        int nbHotellerieTourisme = Math.Max(1, (int)(nbEntreprisesNormales * _config.PartEntreprisesHotellerieTourisme));

        _entreprisesTourisme.Clear();

        for (int i = 0; i < nbEntreprisesNormales; i++)
        {
            ESecteurActivite secteur;
            if (i < nbAgricoles)
                secteur = ESecteurActivite.Agriculture;
            else if (i < nbAgricoles + nbConstruction)
                secteur = ESecteurActivite.Construction;
            else if (i < nbAgricoles + nbConstruction + nbHotellerieTourisme)
                secteur = ESecteurActivite.HotellerieTourisme;
            else
                secteur = secteursLocaux[2 + random.Next(secteursLocaux.Length - 2)]; // hors Agriculture/Construction

            // Probabilité informel : agriculture ~95%, commerce ~80%, tourisme ~40%, autres ~70%
            bool estInformel = secteur switch
            {
                ESecteurActivite.Agriculture => random.NextDouble() < 0.95,
                ESecteurActivite.Commerces => random.NextDouble() < 0.80,
                ESecteurActivite.Construction => random.NextDouble() < 0.75,
                ESecteurActivite.SecteurMinier => random.NextDouble() < 0.20,
                ESecteurActivite.HotellerieTourisme => random.NextDouble() < 0.40, // mix formel/informel
                _ => random.NextDouble() < _config.PartSecteurInformel
            };

            int indexTourisme = i - nbAgricoles - nbConstruction;
            var nomEntreprise = $"Entreprise {i + 1}";


            var entreprise = new AgentCompany
            {
                Name = nomEntreprise,
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _householdSalaryDistributionModule.TirerSalaire(random) * (0.9 + random.NextDouble() * 0.2),
                MargeBeneficiaire = secteur == ESecteurActivite.HotellerieTourisme ? 0.25 : _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = _companyModule.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = _companyModule.GetTresorerieInitiale(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
                SecteurActivite = secteur,
                EstInformel = estInformel
            };
            _entreprises.Add(entreprise);

            // Collecter les entreprises tourisme pour le routage des loisirs
            if (secteur == ESecteurActivite.HotellerieTourisme)
                _entreprisesTourisme.Add(entreprise);
        }

        
        var salairesGeneres = new double[_config.NombreMenages];
        // Toutes les entreprises (normales + import + export) pour l'affectation des ménages
        var toutesEntreprises = new List<AgentCompany>();
        toutesEntreprises.AddRange(_entreprises);
        toutesEntreprises.AddRange(_importateurs);
        toutesEntreprises.AddRange(_exportateurs);
        int entrepriseIndex = 0;
        var salairesParEmployeur = new Dictionary<int, List<double>>();

        // Pré-calculer le coût quotidien du riz par ménage (taille ménage ~4.5 personnes)
        double taillesMenage = 4.5;
        double consoRizJourKgParPersonne = _config.ConsommationRizAnnuelleKgParPersonne / 365.0;
        double prixRizMoyenKg = _config.PrixRizLocalKg * (1.0 - _config.PartRizImporte)
                              + _config.PrixRizImporteKg * _config.PartRizImporte;
        double depensesRizJourParMenage = consoRizJourKgParPersonne * taillesMenage * prixRizMoyenKg;

        int nbAbonnesEau = 0;
        int nbAbonnesElectricite = 0;

        // Nombre de fonctionnaires à l'échelle de la simulation
        int nbFonctionnairesSimulation = Math.Max(1, (int)(_config.NombreFonctionnaires * _facteurEchelle));
        int fonctionnairesAffectes = 0;

        for (int i = 0; i < _config.NombreMenages; i++)
        {
            bool estHomme = random.NextDouble() > 0.5;

            // Déterminer si ce ménage sera fonctionnaire
            // Les fonctionnaires sont affectés en premier, avec le salaire TOFE
            bool estFonctionnaire = fonctionnairesAffectes < nbFonctionnairesSimulation;

            // Tirer un salaire de la distribution log-normale (ou TOFE pour fonctionnaires)
            double salaire;
            if (estFonctionnaire)
            {
                // Salaire fonctionnaire : moyenne TOFE ± 30% de variation
                salaire = _config.SalaireMoyenFonctionnaireMensuel * (0.7 + random.NextDouble() * 0.6);
                fonctionnairesAffectes++;
            }
            else
            {
                salaire = _householdSalaryDistributionModule.TirerSalaire(random);
            }
            salairesGeneres[i] = salaire;

            // Déterminer la classe socio-économique
            var classe = _householdSalaryDistributionModule.DeterminerClasse(salaire);
            var comportement = _householdSalaryDistributionModule.GetComportementParClasse(classe, random);

            // Les fonctionnaires sont toujours employés ; les autres suivent la probabilité
            bool estEmploye = estFonctionnaire || random.NextDouble() < comportement.ProbabiliteEmploi;
            int? employeurId = null;
            if (estEmploye && !estFonctionnaire)
            {
                var employeur = toutesEntreprises[entrepriseIndex];
                employeurId = employeur.Id;

                if (!salairesParEmployeur.TryGetValue(employeur.Id, out var salairesEmployeur))
                {
                    salairesEmployeur = [];
                    salairesParEmployeur[employeur.Id] = salairesEmployeur;
                }
                salairesEmployeur.Add(salaire);
                entrepriseIndex = (entrepriseIndex + 1) % toutesEntreprises.Count;
            }

            // Accès eau et électricité Jirama (probabilité selon config)
            bool accesEau = random.NextDouble() < _config.TauxAccesEau;
            bool accesElec = random.NextDouble() < _config.TauxAccesElectricite;
            if (accesEau) nbAbonnesEau++;
            if (accesElec) nbAbonnesElectricite++;

            // Consommation électrique par ménage en KWh/jour
            // Variation par classe : subsistance consomme moins, cadres plus
            double facteurConsoElec = classe switch
            {
                ClasseSocioEconomique.Subsistance => 0.5 + random.NextDouble() * 0.3,    // 0.5-0.8×
                ClasseSocioEconomique.InformelBas => 0.7 + random.NextDouble() * 0.3,     // 0.7-1.0×
                ClasseSocioEconomique.FormelBas => 0.9 + random.NextDouble() * 0.2,       // 0.9-1.1×
                ClasseSocioEconomique.FormelQualifie => 1.1 + random.NextDouble() * 0.4,  // 1.1-1.5×
                ClasseSocioEconomique.Cadre => 1.5 + random.NextDouble() * 1.0,           // 1.5-2.5×
                _ => 1.0
            };
            double consoElecKWhJour = accesElec
                ? _config.ConsommationElecMenageKWhJour * facteurConsoElec
                : 0;

            var menage = new AgentHousehold
            {
                SalaireMensuel = salaire,
                Classe = classe,
                TauxEpargne = comportement.TauxEpargne,
                PropensionConsommation = comportement.PropensionConsommation,
                Epargne = comportement.EpargneInitiale,
                EstEmploye = estEmploye,
                EstFonctionnaire = estFonctionnaire,
                EmployeurId = employeurId,
                DepensesAlimentairesJour = comportement.DepensesAlimentairesJour,
                DepensesDiversJour = comportement.DepensesDiversJour,
                Transport = comportement.Transport,
                DistanceDomicileTravailKm = comportement.DistanceDomicileTravailKm,
                DepensesRizJour = depensesRizJourParMenage,
                AccesEau = accesEau,
                AccesElectricite = accesElec,
                DepensesEauJour = accesEau ? _config.TarifEauJourMenage : 0,
                ConsommationElecKWhJour = consoElecKWhJour,
                DepensesElectriciteJour = consoElecKWhJour * _config.PrixElectriciteArKWh,
                EstProprietaire = random.NextDouble() < _config.TauxMenagesProprietaires,
                // Loisirs et vacances
                BudgetSortieWeekend = comportement.BudgetSortieWeekend,
                BudgetVacances = comportement.BudgetVacances,
                ProbabiliteSortieWeekend = comportement.ProbabiliteSortieWeekend,
                FrequenceVacancesJours = comportement.FrequenceVacancesJours,
                ProbabiliteVacances = comportement.ProbabiliteVacances,
                DureeVacancesJours = comportement.DureeVacancesJours
            };
            _menages.Add(menage);
        }

        foreach (var entreprise in toutesEntreprises)
        {
            if (salairesParEmployeur.TryGetValue(entreprise.Id, out var salairesEmployeur) && salairesEmployeur.Count > 0)
            {
                entreprise.NombreEmployes = salairesEmployeur.Count;
                entreprise.SalaireMoyenMensuel = salairesEmployeur.Average();
            }
            else
            {
                entreprise.NombreEmployes = 0;
            }
        }

        // Mettre à jour les compteurs Jirama
        _jirama.NbAbonnesEau = nbAbonnesEau;
        _jirama.NbAbonnesElectricite = nbAbonnesElectricite;

        // Calculer les statistiques initiales de la distribution
        _statsInitiales = _householdSalaryDistributionModule.CalculerStats(salairesGeneres);

        // Initialiser le résultat
        _result = new SimulationResult
        {
            Scenario = _config,
            JoursSimules = 0,
            EstTerminee = false,
            EnCours = false
        };
    }

    /// <summary>
    /// Démarre la simulation en temps réel (1 seconde = 1 jour).
    /// </summary>
    public async Task DemarrerAsync()
    {
        if (_result.EnCours) return;

        _cts = new CancellationTokenSource();
        _result.EnCours = true;

        try
        {
            while (_jourCourant < _config.DureeJours && !_cts.Token.IsCancellationRequested)
            {
                SimulerUnJour();
                OnTickCompleted?.Invoke();

                // 1 seconde = 1 jour
                await Task.Delay(1000, _cts.Token);
            }

            _result.EstTerminee = true;
        }
        catch (OperationCanceledException)
        {
            // Simulation arrêtée par l'utilisateur
        }
        finally
        {
            _result.EnCours = false;
            OnTickCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Exécute la simulation complète d'un coup (mode rapide).
    /// </summary>
    public async Task ExecuterRapideAsync(int delaiEntreJoursMs = 50)
    {
        if (_result.EnCours) return;

        _cts = new CancellationTokenSource();
        _result.EnCours = true;

        try
        {
            while (_jourCourant < _config.DureeJours && !_cts.Token.IsCancellationRequested)
            {
                SimulerUnJour();

                // Notifier l'UI à intervalles pour fluidité
                if (_jourCourant % 5 == 0 || _jourCourant == _config.DureeJours)
                {
                    OnTickCompleted?.Invoke();
                    await Task.Delay(delaiEntreJoursMs, _cts.Token);
                }
            }

            _result.EstTerminee = true;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _result.EnCours = false;
            OnTickCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Arrête la simulation en cours.
    /// </summary>
    public void Arreter()
    {
        _cts?.Cancel();
    }

    /// <summary>
    /// Exécute un seul jour de simulation de manière synchrone, sans événement UI.
    /// Utilisé par le calibrateur ML pour générer des données d'entraînement rapidement.
    /// </summary>
    public void SimulerUnJourSync()
    {
        if (_jourCourant < _config.DureeJours)
        {
            SimulerUnJour();
            _result.JoursSimules = _jourCourant;
        }
    }

    /// <summary>
    /// Exécute N jours de simulation de manière synchrone (batch, sans UI).
    /// </summary>
    public void SimulerNJoursSync(int nbJours)
    {
        for (int i = 0; i < nbJours && _jourCourant < _config.DureeJours; i++)
        {
            SimulerUnJour();
        }
        _result.JoursSimules = _jourCourant;
    }

    /// <summary>
    /// Simule un seul jour de l'économie.
    /// </summary>
    private void SimulerUnJour()
    {
        _jourCourant++;
        bool jourOuvrable = EstJourOuvrable(_jourCourant);

        // 1. Les ménages consomment tous les jours, mais ne travaillent que les jours ouvrables
        // Exception : les employés du commerce travaillent 7j/7
        var resultsMenages = new List<DailyHouseholdResult>();
        double demandeConsommationTotale = 0;

        // Accumulateurs pour le routage des dépenses de transport vers les entreprises
        double totalTransportInformel = 0;
        double totalTransportFormel = 0;
        double totalTransportCarburant = 0;

        // Reset des compteurs Jirama pour ce jour
        _jirama.DebutJournee();

        // Pré-calculer les secteurs des employeurs pour déterminer qui travaille
        var toutesEntreprisesLookup = new Dictionary<int, AgentCompany>();
        foreach (var e in _entreprises) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _importateurs) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _exportateurs) toutesEntreprisesLookup[e.Id] = e;

        // Calculer la remittance par ménage (répartition uniforme, mise à l'échelle)
        double remittanceParMenage = _householdRemittanceModule.CalculerRemittanceParMenage(
            _config.RemittancesJour,
            _facteurEchelle,
            _menages.Count);
        double loyerImputeJour = _config.LoyerImputeJourParMenage;

        // Accumuler la masse salariale des fonctionnaires (micro -> macro)
        double masseSalarialeFonctionnairesJour = 0;

        foreach (var menage in _menages)
        {
            // Déterminer si le ménage travaille aujourd'hui :
            // - Fonctionnaires : lun-ven uniquement (secteur public)
            // - Jour ouvrable (lun-ven) : tout le monde travaille
            // - Weekend : seuls les employes du commerce travaillent
            bool estJourDeTravail;
            if (menage.EstFonctionnaire)
            {
                estJourDeTravail = jourOuvrable;
            }
            else if (!jourOuvrable && menage.EmployeurId.HasValue)
            {
                estJourDeTravail = toutesEntreprisesLookup.TryGetValue(menage.EmployeurId.Value, out var emp)
                    && emp.SecteurActivite == ESecteurActivite.Commerces;
            }
            else
            {
                estJourDeTravail = jourOuvrable;
            }

            var r = _householdModule.SimulerJournee(
                menage,
                _governmentModule.CalculerIRSAJournalier(_etat, menage.SalaireMensuel),
                _governmentModule.TauxEffectifIRSA(_etat, menage.SalaireMensuel),
                _etat.TauxInflation,
                _etat.TauxTVA,
                _config.PrixCarburantLitre,
                _jirama,
                estJourDeTravail,
                _jourCourant,
                _config.CoutTransportPaiementJirama,
                _config.PrixCarburantReference,
                _config.ElasticitePrixParCarburant,
                _config.VolatiliteAleatoireMarche,
                _config.ElasticiteComportementMenage,
                _config.PartRevenuAlimentaire,
                0,  // depensesEducation
                0,  // depensesSante
                0,  // depensesLoyerLocatif
                0,  // depensesConstructionMaison
                0,  // depensesConstructionBTP
                0,  // depensesConstructionQuincaillerie
                0,  // depensesConstructionTransportInformel
                _random);

            // Remittances (transferts diaspora) — ajoutées au revenu
            var remittanceResult = _householdRemittanceModule.AppliquerRemittance(menage.Epargne, remittanceParMenage);
            r.Remittance = remittanceResult.Remittance;
            menage.Epargne = remittanceResult.EpargneTotale;

            // Loyer imputé (SCN 2008) â€” propriétaires occupants uniquement
            r.LoyerImpute = menage.EstProprietaire ? loyerImputeJour : 0;

            // Cotisation salariale CNaPS (1%, prélevée sur le salaire)
            r.CotisationCNaPSSalariale = r.RevenuBrut > 0 ? r.RevenuBrut * _config.TauxCotisationsSalarialesCNaPS : 0;

            // Accumuler la masse salariale des fonctionnaires (payée par l'État)
            if (menage.EstFonctionnaire && r.RevenuBrut > 0)
            {
                masseSalarialeFonctionnairesJour += r.RevenuBrut;
            }

            // ── Achat alimentaire journalier ──────────────────────────────────────────
            // L'appel à IHouseholdModule décompose la dépense alimentaire en deux flux :
            //   • secteur informel (85 %) → demande vers marchés de quartier / vendeurs
            //   • secteur formel (15 % + TVA 20 %) → demande vers épiceries / supermarchés
            // Le facteur logistique réduit les quantités en cas d'inflation cumulée élevée.
            //
            // Étape 1 : ajuster le prix de base avec IPriceModule (élasticité carburant + aléa)
            // afin que la demande transmise aux entreprises reflète les chocs de prix du carburant.
            // Auparavant, menage.DepensesAlimentairesJour était passé sans ajustement de prix.
            double depenseAlimAjustee = _priceModule.AjusterPrixParCarburant(
                prixBase:               menage.DepensesAlimentairesJour,
                prixCarburantCourant:   _config.PrixCarburantLitre,
                prixCarburantReference: _config.PrixCarburantReference,
                elasticitePrix:         _config.ElasticitePrixParCarburant,
                volatiliteAlea:         _config.VolatiliteAleatoireMarche,
                random:                 _random);

            double revenuDisponible = Math.Max(0, r.RevenuBrut - r.ImpotIR);
            var achat = _householdModule.AcheteProduitsAlimentaires(
                depenseAlimentairesJourBase:    depenseAlimAjustee,   // prix ajusté (was: menage.DepensesAlimentairesJour)
                cumulHaussePrixAlimentaire:     _etat.TauxInflation * 100,
                elasticiteUtilisateur:          _config.ElasticiteComportementMenage,
                revenuDisponible:               revenuDisponible);

            r.DepensesAlimentaires         = achat.CoutTotal;
            r.DepensesAlimentairesInformel = achat.CoutInformel;
            r.DepensesAlimentairesFormel   = achat.CoutFormel;
            r.ReductionQuantiteAlimentaire = achat.QuantiteReduite;
            // ─────────────────────────────────────────────────────────────────────────

            // ── Loisirs et vacances ──────────────────────────────────────────────────
            // Détermine si le ménage fait une sortie weekend ou part en vacances.
            // Chaque ménage est aléatoirement dirigé vers une compagnie tourisme différente.
            bool estWeekend = !jourOuvrable;
            bool estPeriodeVacances = menage.FrequenceVacancesJours > 0
                && (_jourCourant - menage.DerniereVacanceJour) >= menage.FrequenceVacancesJours;
            bool estDejaEnVacances = menage.JoursVacanceRestants > 0;

            // Choisir aléatoirement une compagnie tourisme pour ce ménage
            AgentCompany? compagnieTourismeChoisie = _entreprisesTourisme.Count > 0
                ? _entreprisesTourisme[_random.Next(_entreprisesTourisme.Count)]
                : null;

            var loisirs = _householdLeisureSpendingModule.CalculerDepensesLoisirs(
                classe:                     menage.Classe,
                budgetSortieWeekend:        menage.BudgetSortieWeekend,
                budgetVacances:             menage.BudgetVacances,
                probabiliteSortieWeekend:   menage.ProbabiliteSortieWeekend,
                probabiliteVacances:        menage.ProbabiliteVacances,
                estWeekend:                 estWeekend,
                estPeriodeVacances:         estPeriodeVacances,
                estEnVacancesEnCours:       estDejaEnVacances,
                cumulHaussePrix:            _etat.TauxInflation * 100,
                revenuDisponible:           revenuDisponible,
                compagnieTourisme:          compagnieTourismeChoisie,
                random:                     _random);

            r.DepensesLoisirs = loisirs.DepensesLoisirs;
            r.EstEnSortie = loisirs.EstEnSortie;
            r.EstEnVacances = loisirs.EstEnVacances;
            r.FacteurReductionLoisirs = loisirs.FacteurReduction;

            // Gérer l'état des vacances du ménage
            if (loisirs.EstEnVacances && !estDejaEnVacances)
            {
                // Départ en vacances : initialiser le compteur
                menage.JoursVacanceRestants = menage.DureeVacancesJours;
                menage.DerniereVacanceJour = _jourCourant;
            }
            if (menage.JoursVacanceRestants > 0)
            {
                menage.JoursVacanceRestants--;
            }

            // Dépenser les loisirs (puiser dans le budget discrétionnaire ou l'épargne)
            if (loisirs.DepensesLoisirs > 0)
            {
                menage.TotalDepensesLoisirs += loisirs.DepensesLoisirs;
                menage.Epargne -= Math.Min(loisirs.DepensesLoisirs, menage.Epargne);
            }
            // ─────────────────────────────────────────────────────────────────────────

            // ── Routage transport → entreprises ─────────────────────────────────────
            // Décompose la dépense de transport du ménage en flux informel/formel/carburant
            // pour injection dans la demande des entreprises (auparavant flux sortant perdu).
            var transportRouting = _transportationModule.RouterDepenseTransport(
                menage.Transport,
                r.DepensesTransport,
                r.DepensesTransportJirama);
            totalTransportInformel += transportRouting.PartInformel;
            totalTransportFormel += transportRouting.PartFormel;
            totalTransportCarburant += transportRouting.PartCarburant;
            // ─────────────────────────────────────────────────────────────────────────

            resultsMenages.Add(r);
            demandeConsommationTotale += r.Consommation;
        }

        // (FinJournee Jirama sera appelé après que tous les secteurs aient payé)

        // 2. Les entreprises produisent et vendent (selon leur secteur)
        // RNG journalier reproductible pour la variation des flux commerciaux
        var rngJour = new Random(_jourCourant * 17 + 3);

        var resultsEntreprises = new List<CompanyDailyResult>();
        var resultsImportateurs = new List<DailyImporterResult>();
        var resultsExportateurs = new List<DailyExporterResult>();

        int totalEntreprises = _entreprises.Count + _importateurs.Count + _exportateurs.Count;

        // ── Décomposition de la demande alimentaire informel/formel ──────────────
        // Les achats alimentaires (IHouseholdModule.AcheteProduitsAlimentaires) sont
        // dirigés vers les entreprises selon leur statut :
        //   • CoutInformel → réparti sur les entreprises estInformel=true
        //   • CoutFormel   → réparti sur les entreprises estInformel=false
        //
        // Pour éviter le double-comptage, on retire le composant alimentaire EXACT calculé
        // par SimulerJournee (DepensesAlimentairesSimulee) de la demande totale, puis on
        // re-route proprement via les résultats de AcheteProduitsAlimentaires.
        //   demandeConsommationTotale = nourriture(SimulerJournee) + non-nourriture
        //   demandeHorsAlim           = non-nourriture seule
        //   demande effective  = demandeHorsAlim + bonusAlimInformel | bonusAlimFormel
        double totalAlimInformel = resultsMenages.Sum(r => r.DepensesAlimentairesInformel);
        double totalAlimFormel   = resultsMenages.Sum(r => r.DepensesAlimentairesFormel);

        int nbInformelles = Math.Max(_entreprises.Count(e => e.EstInformel), 1);
        int nbFormelles   = Math.Max(_entreprises.Count(e => !e.EstInformel)
                                   + _importateurs.Count + _exportateurs.Count, 1);

        double bonusDemandeAlimParEntrepriseInformelle = totalAlimInformel / nbInformelles;
        double bonusDemandeAlimParEntrepriseFormelle   = totalAlimFormel   / nbFormelles;

        // Soustraction du composant alimentaire exact issu de SimulerJournee (pas de AcheteProduitsAlimentaires)
        // pour éviter tout double-comptage lors du re-routage ci-dessus.
        double totalAlimSimulee = resultsMenages.Sum(r => r.DepensesAlimentairesSimulee);
        double totalTransport = resultsMenages.Sum(r => r.DepensesTransport + r.DepensesTransportJirama);
        double demandeHorsAlim = Math.Max(0, demandeConsommationTotale - totalAlimSimulee - totalTransport);

        // Split 85 % informel / 15 % formel pour la consommation non-alimentaire et hors transport.
        // Le transport est désormais routé séparément via ITransportationModule (décomposition sectorielle).
        double demandeHorsAlimInformelParEntreprise = (demandeHorsAlim * 0.85 + totalTransportInformel) / Math.Max(nbInformelles, 1);
        double demandeHorsAlimFormelParEntreprise   = (demandeHorsAlim * 0.15 + totalTransportFormel) / Math.Max(nbFormelles, 1);
        // ─────────────────────────────────────────────────────────────────────────

        // ── Routage des dépenses de loisirs vers les compagnies tourisme ────────
        // Les dépenses de loisirs (sorties weekend + vacances) sont déjà comptabilisées
        // directement sur chaque compagnie tourisme via CalculerDepensesLoisirs().
        // Ici on calcule la demande par compagnie tourisme pour SimulerJournee()
        // (production, charges, IS, TVA) en répartissant uniformément.
        double demandeLoisirsTotale = resultsMenages.Sum(r => r.DepensesLoisirs);
        int nbEntreprisesTourisme = Math.Max(_entreprisesTourisme.Count, 1);
        double demandeLoisirParEntrepriseTourisme = demandeLoisirsTotale / nbEntreprisesTourisme;

        foreach (var entreprise in _entreprises)
        {
            bool travailleCeJour = EntrepriseTravailleCeJour(_jourCourant, entreprise.SecteurActivite);

            // Les compagnies tourismes reçoivent les dépenses de loisirs réparties
            // Les autres entreprises reçoivent la demande habituelle (alimentaire + non-alimentaire)
            double demandeEffective;
            if (entreprise.SecteurActivite == ESecteurActivite.HotellerieTourisme)
            {
                demandeEffective = demandeLoisirParEntrepriseTourisme;
            }
            else if (entreprise.EstInformel)
            {
                demandeEffective = demandeHorsAlimInformelParEntreprise + bonusDemandeAlimParEntrepriseInformelle;
            }
            else
            {
                demandeEffective = demandeHorsAlimFormelParEntreprise + bonusDemandeAlimParEntrepriseFormelle;
            }
            var r = _companyModule.SimulerJournee(
                entreprise,
                demandeEffective,
                _etat.TauxIS,
                _etat.TauxTVA,
                _etat.TauxInflation,
                _etat.TauxDirecteur,
                travailleCeJour,
                _jirama,
                _config.ConsommationElecParEmployeKWhJour,
                _config.TauxCotisationsPatronalesCNaPS,
                _config.PrixCarburantLitre,
                _config.PrixCarburantReference,
                _config.ElasticitePrixParCarburant
            );
            resultsEntreprises.Add(r);
        }

        // Importateurs — injection directe des moyennes INSTAT (1 agent par catégorie)
        if (_config.UseImportCalibresDirectement && _config.CIFCalibresJour.Count > 0)
        {
            foreach (var importateur in _importateurs)
            {
                // CIF calibré = moyenne INSTAT ± 15% de variation journaliire
                double cifBase = _config.CIFCalibresJour.GetValueOrDefault(importateur.Categorie, 0d) * 1_000_000 * _facteurEchelle;
                double cifJourImportateur = cifBase * (0.85 + rngJour.NextDouble() * 0.30);

                // Calculer les droits et taxes
                // was: importateur.CoefficientDDParCategorie()
                double droitsDouaneTaux = _config.TauxDroitsDouane * _companyModule.GetCoefficientDroitsDouaneParCategorie(importateur.Categorie);
                double droitsDouane = cifJourImportateur * droitsDouaneTaux;

                // was: importateur.CoefficientAcciseParCategorie()
                double acciseTaux = _config.TauxAccise * _companyModule.GetCoefficientAcciseParCategorie(importateur.Categorie);
                double accise = (cifJourImportateur + droitsDouane) * acciseTaux;

                double tvaImport = (cifJourImportateur + droitsDouane + accise) * _etat.TauxTVA;
                double redevance = cifJourImportateur * 0.02;

                double coutTotal = cifJourImportateur + droitsDouane + accise + tvaImport + redevance;
                double reventeJour = cifJourImportateur * (1.0 + importateur.MargeReventeImport);

                importateur.TotalImportationsCIF += cifJourImportateur;
                importateur.TotalDroitsDouane += droitsDouane;
                importateur.TotalAccise += accise;
                importateur.TotalTVAImport += tvaImport;
                importateur.TotalRedevanceStatistique += redevance;

                bool importeurTravaille = EntrepriseTravailleCeJour(_jourCourant, importateur.SecteurActivite);
                var baseResult = _companyModule.SimulerJournee(
                    importateur,
                    demandeHorsAlimFormelParEntreprise + bonusDemandeAlimParEntrepriseFormelle,
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, importeurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS,
                    _config.PrixCarburantLitre, _config.PrixCarburantReference, _config.ElasticitePrixParCarburant);

                var r = new DailyImporterResult
                {
                    VentesB2C = baseResult.VentesB2C,
                    VentesB2B = baseResult.VentesB2B,
                    ChargesSalariales = baseResult.ChargesSalariales,
                    CotisationsCNaPS = baseResult.CotisationsCNaPS,
                    AchatsB2B = baseResult.AchatsB2B,
                    DepensesElectricite = baseResult.DepensesElectricite,
                    TVACollectee = baseResult.TVACollectee,
                    ImpotIS = baseResult.ImpotIS,
                    BeneficeAvantImpot = baseResult.BeneficeAvantImpot,
                    FluxNetJour = baseResult.FluxNetJour,
                    CoutFinancement = baseResult.CoutFinancement,
                    ValeurAjoutee = baseResult.ValeurAjoutee,
                    ValeurCIF = cifJourImportateur,
                    DroitsDouane = droitsDouane,
                    Accise = accise,
                    TVAImport = tvaImport,
                    RedevanceStatistique = redevance,
                    CoutTotalImport = coutTotal,
                    ReventeImport = reventeJour
                };

                importateur.Tresorerie -= coutTotal;
                importateur.Tresorerie += reventeJour;
                r.Tresorerie = importateur.Tresorerie;

                resultsImportateurs.Add(r);
            }
        }
        else
        {
            // Mode simulation classique
            foreach (var importateur in _importateurs)
            {
                bool travailleCeJour = EntrepriseTravailleCeJour(_jourCourant, importateur.SecteurActivite);
                var r = importateur.SimulerJourneeImport(
                    demandeHorsAlimFormelParEntreprise + bonusDemandeAlimParEntrepriseFormelle,
                    _etat.TauxIS,
                    _etat.TauxTVA,
                    _etat.TauxInflation,
                    _etat.TauxDirecteur,
                    _config.TauxDroitsDouane,
                    _config.TauxAccise,
                    _companyModule,
                    travailleCeJour,
                    _jirama,
                    _config.ConsommationElecParEmployeKWhJour,
                    _config.TauxCotisationsPatronalesCNaPS,
                    _config.PrixCarburantLitre,
                    _config.PrixCarburantReference,
                    _config.ElasticitePrixParCarburant
                );
                resultsImportateurs.Add(r);
            }
        }

        // Exportateurs — injection directe des moyennes INSTAT (1 agent par catégorie)
        if (_config.UseExportCalibresDirectement && _config.FOBCalibresJour.Count > 0)
        {
            foreach (var exportateur in _exportateurs)
            {
                // FOB calibré = moyenne INSTAT ± 15% de variation journalière
                double fobBase = _config.FOBCalibresJour.GetValueOrDefault(exportateur.Categorie, 0d) * 1_000_000 * _facteurEchelle;
                double fobJourExportateur = fobBase * (0.85 + rngJour.NextDouble() * 0.30);

                double taxeTaux = _config.TauxTaxeExport;
                double taxeExport = fobJourExportateur * taxeTaux;
                double redevance = fobJourExportateur * 0.015;
                double devisesNettes = fobJourExportateur - taxeExport - redevance;

                exportateur.TotalExportationsFOB += fobJourExportateur;
                exportateur.TotalTaxeExport += taxeExport;
                exportateur.TotalRedevanceExport += redevance;
                exportateur.TotalDevisesRapatriees += devisesNettes;

                bool exporteurTravaille = EntrepriseTravailleCeJour(_jourCourant, exportateur.SecteurActivite);
                var baseResult = _companyModule.SimulerJournee(
                    exportateur,
                    (demandeHorsAlimFormelParEntreprise + bonusDemandeAlimParEntrepriseFormelle) * (1.0 - exportateur.PartExport),
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, exporteurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS,
                    _config.PrixCarburantLitre, _config.PrixCarburantReference, _config.ElasticitePrixParCarburant);

                var r = new DailyExporterResult
                {
                    VentesB2C = baseResult.VentesB2C,
                    VentesB2B = baseResult.VentesB2B,
                    ChargesSalariales = baseResult.ChargesSalariales,
                    CotisationsCNaPS = baseResult.CotisationsCNaPS,
                    AchatsB2B = baseResult.AchatsB2B,
                    DepensesElectricite = baseResult.DepensesElectricite,
                    TVACollectee = baseResult.TVACollectee,
                    ImpotIS = baseResult.ImpotIS,
                    BeneficeAvantImpot = baseResult.BeneficeAvantImpot,
                    FluxNetJour = baseResult.FluxNetJour,
                    CoutFinancement = baseResult.CoutFinancement,
                    ValeurAjoutee = baseResult.ValeurAjoutee,
                    ValeurFOB = fobJourExportateur,
                    TaxeExport = taxeExport,
                    RedevanceExport = redevance,
                    DevisesRapatriees = devisesNettes
                };

                exportateur.Tresorerie += devisesNettes;
                r.Tresorerie = exportateur.Tresorerie;

                resultsExportateurs.Add(r);
            }
        }
        else
        {
            // Mode simulation classique
            foreach (var exportateur in _exportateurs)
            {
                bool travailleCeJour = EntrepriseTravailleCeJour(_jourCourant, exportateur.SecteurActivite);
                var r = exportateur.SimulerJourneeExport(
                    demandeHorsAlimFormelParEntreprise + bonusDemandeAlimParEntrepriseFormelle,
                    _etat.TauxIS,
                    _etat.TauxTVA,
                    _etat.TauxInflation,
                    _etat.TauxDirecteur,
                    _config.TauxTaxeExport,
                    _companyModule,
                    travailleCeJour,
                    _jirama,
                    _config.ConsommationElecParEmployeKWhJour,
                    _config.TauxCotisationsPatronalesCNaPS,
                    _config.PrixCarburantLitre,
                    _config.PrixCarburantReference,
                    _config.ElasticitePrixParCarburant
                );
                resultsExportateurs.Add(r);
            }
        }

        // 3. L'État consolide (entreprises + importateurs + exportateurs + Jirama)
        // Les paramètres macro absolus sont mis à l'échelle par facteurEchelle
        var resultEtat = _governmentModule.SimulerJournee(
            _etat, resultsMenages, resultsEntreprises, resultsImportateurs, resultsExportateurs,
            _jirama, _config.ConsommationElecEtatKWhJour * _facteurEchelle,
            _config.AideInternationaleJour * _facteurEchelle,
            _config.SubventionJiramaJour * _facteurEchelle,
            masseSalarialeFonctionnairesJour,
            _config.TauxReinvestissementPrive,
            _config.DepensesCapitalJour * _facteurEchelle,
            _config.InteretsDetteJour * _facteurEchelle);

        // 3b. Finaliser les compteurs Jirama (production totale + VA)
        // Après que tous les secteurs (ménages, entreprises, État) ont payé leurs factures.
        _jirama.FinJournee();

        // 4. Redistribution des transferts sociaux aux ménages les plus pauvres
        double transfertParMenage = resultEtat.TransfertsSociaux / _menages.Count;
        foreach (var menage in _menages)
        {
            menage.Epargne += transfertParMenage;
        }

        // --- NOUVEAU: MODULE BANCAIRE ---
        _bankModule.CalculerBilansBancaires(_banque, _menages, _entreprises);
        _bankModule.SimulerOctroiCredit(_banque, _entreprises, _menages, _config.CroissanceCreditJour, _random);

        // 5. Créer le snapshot
        // Calculer les métriques de distribution des épargnes
        var epargnesSorted = _menages.OrderBy(m => m.Epargne).Select(m => m.Epargne).ToArray();
        var distStats = _householdSalaryDistributionModule.CalculerStats(epargnesSorted);
        int n = _menages.Count;

        // Agréger toutes les entreprises pour les métriques
        var toutesEntreprisesRef = new List<AgentCompany>();
        toutesEntreprisesRef.AddRange(_entreprises);
        toutesEntreprisesRef.AddRange(_importateurs);
        toutesEntreprisesRef.AddRange(_exportateurs);

        var tousResultsEntreprises = new List<CompanyDailyResult>();
        tousResultsEntreprises.AddRange(resultsEntreprises);
        tousResultsEntreprises.AddRange(resultsImportateurs);
        tousResultsEntreprises.AddRange(resultsExportateurs);

        var pibResult = _governmentModule.CalculerPIB(
            resultsMenages,
            tousResultsEntreprises,
            resultsImportateurs,
            resultsExportateurs,
            _jirama,
            resultEtat,
            _jourCourant);

        var snapshot = new DailySnapshot
        {
            Jour = _jourCourant,
            JourSemaine = JourDeLaSemaine(_jourCourant),
            NomJour = NomJourSemaine(_jourCourant),
            EstJourOuvrable = jourOuvrable,

            // Ménages
            EpargneMoyenneMenages = _menages.Average(m => m.Epargne),
            EpargneTotaleMenages = _menages.Sum(m => m.Epargne),
            ConsommationTotaleMenages = resultsMenages.Sum(r => r.Consommation),
            RevenuTotalMenages = resultsMenages.Sum(r => r.RevenuBrut),
            TauxEmploi = _menages.Count(m => m.EstEmploye) / (double)_menages.Count,
            RemittancesTotales = resultsMenages.Sum(r => r.Remittance),
            LoyersImputesTotaux = resultsMenages.Sum(r => r.LoyerImpute),
            CotisationsCNaPSSalariales = resultsMenages.Sum(r => r.CotisationCNaPSSalariale),

            // Riz
            DepensesRizTotales = resultsMenages.Sum(r => r.DepensesRiz),

            // Jirama
            RecettesEauJirama = resultsMenages.Sum(r => r.DepensesEau),
            RecettesElectriciteJirama = resultsMenages.Sum(r => r.DepensesElectricite),
            RecettesTotalesJirama = resultsMenages.Sum(r => r.DepensesEau + r.DepensesElectricite)
                                 + tousResultsEntreprises.Sum(r => r.DepensesElectricite)
                                 + resultEtat.DepensesElectriciteEtat,
            NbAbonnesEau = _jirama.NbAbonnesEau,
            NbAbonnesElectricite = _jirama.NbAbonnesElectricite,
            ConsommationMenagesKWh = _jirama.ConsommationMenagesKWhJour,
            ConsommationEntreprisesKWh = _jirama.ConsommationEntreprisesKWhJour,
            ConsommationEtatKWh = _jirama.ConsommationEtatKWhJour,
            ProductionElecKWh = _jirama.ProductionKWhJour,
            PrixElectriciteArKWh = _jirama.PrixElectriciteArKWh,
            FactureElecMoyenneParMenage = _jirama.NbAbonnesElectricite > 0
                ? resultsMenages.Sum(r => r.DepensesElectricite) / _jirama.NbAbonnesElectricite
                : 0,
            TVAJiramaJour = _jirama.TVACollecteeJour,
            DepensesTransportJirama = resultsMenages.Sum(r => r.DepensesTransportJirama),
            ValeurAjouteeJirama = _jirama.ValeurAjouteeJour,

            // Entreprises (toutes confondues)
            ChiffreAffairesTotalEntreprises = tousResultsEntreprises.Sum(r => r.VentesB2C + r.VentesB2B),
            TresorerieMoyenneEntreprises = toutesEntreprisesRef.Average(e => e.Tresorerie),
            BeneficeTotalEntreprises = tousResultsEntreprises.Sum(r => r.BeneficeAvantImpot),
            VentesB2CTotales = tousResultsEntreprises.Sum(r => r.VentesB2C),
            VentesB2BTotales = tousResultsEntreprises.Sum(r => r.VentesB2B),
            DepensesElectriciteEntreprises = tousResultsEntreprises.Sum(r => r.DepensesElectricite),
            CotisationsCNaPSPatronales = tousResultsEntreprises.Sum(r => r.CotisationsCNaPS),
            NbEntreprisesInformelles = toutesEntreprisesRef.Count(e => e.EstInformel),

            // Commerce extérieur
            ImportationsCIF = resultEtat.ImportationsCIF,
            ExportationsFOB = resultEtat.ExportationsFOB,
            BalanceCommerciale = resultEtat.BalanceCommerciale,
            RecettesDouanieres = resultEtat.RecettesDouanieres,
            DroitsDouaneJour = resultEtat.DroitsDouane,
            AcciseJour = resultEtat.Accise,
            NbImportateurs = _importateurs.Count,
            NbExportateurs = _exportateurs.Count,

            // État
            RecettesFiscalesTotales = _etat.TotalRecettesFiscales,
            RecettesIR = resultEtat.RecettesIR,
            RecettesIS = resultEtat.RecettesIS,
            RecettesTVA = resultEtat.RecettesTVA,
            DepensesPubliques = resultEtat.DepensesPubliques,
            DepensesElectriciteEtat = resultEtat.DepensesElectriciteEtat,
            AideInternationale = resultEtat.AideInternationale,
            SubventionsJirama = resultEtat.SubventionsJirama,
            SalairesFonctionnaires = resultEtat.SalairesFonctionnaires,
            NbFonctionnaires = _menages.Count(m => m.EstFonctionnaire),
            SoldeBudgetaire = _etat.SoldeBudgetaire,
            DettePublique = _etat.DettePublique,

            // FBCF (Formation Brute de Capital Fixe)
            FBCF = resultEtat.FBCF,

            // Variation de stocks (Î”S) : pas de suivi physique des inventaires dans cette simulation.
            // L'ancienne formule (ReventeImport - CIF = marge import) double-comptait le CIF
            // puisque la balance commerciale (X - M) le soustrait déjà.
            VariationStocksImportateurs = 0,

            // PIB par la demande = C + I (FBCF) + G + (X - M) + Loyers imputés
            // G = ConsommationFinaleEtat (exclut FBCF publique pour éviter le double-comptage avec FBCF)
            // (X - M) = BalanceCommerciale = FOB - CIF (déjà mis à l'échelle)
            PIBProxy = pibResult.PIBDemande,

            // PIB par la valeur ajoutée (aux prix du marché) :
            // PIB = Î£(VA aux prix de base) + Impôts sur les produits
            // VA entreprises inclut déjà la TVA collectée (ventesTotales est TTC)
            // Il faut ajouter : TVA Jirama (non incluse dans VA Jirama qui est HT) + Recettes douanières
            // VA Admin publique = rémunération des fonctionnaires (SCN 2008 : production non marchande = coûts)
            PIBParValeurAjoutee = pibResult.PIBParValeurAjoutee,

            // PIB par les revenus (approche revenu du SCN) :
            // PIB = Rémunération des salariés + EBE (Excédent Brut d'Exploitation) + Impôts sur la production - Subventions
            // Note : IR et IS sont des impôts sur le revenu, PAS des impôts sur la production â†’ exclus
            PIBParRevenus = pibResult.PIBParRevenus,

            // Décomposition PIB revenus
            ChargesSalarialesTotalesEntreprises = pibResult.ChargesSalarialesTotalesEntreprises,
            CotisationsCNaPSPatronalesTotales = pibResult.CotisationsCNaPSPatronalesTotales,
            ExcedentBrutExploitation = pibResult.ExcedentBrutExploitation,
            ValeurAjouteeAdminPublique = pibResult.ValeurAjouteeAdminPublique,

            // Distribution des revenus/épargne
            Gini = distStats.Gini,
            RatioD9D1 = distStats.RatioD9D1,
            EpargneQ1 = distStats.Q1Moyenne,
            EpargneQ2 = distStats.Q2Moyenne,
            EpargneQ3 = distStats.Q3Moyenne,
            EpargneQ4 = distStats.Q4Moyenne,
            EpargneQ5 = distStats.Q5Moyenne,

            // Classes socio-économiques
            NbSubsistance = _menages.Count(m => m.Classe == ClasseSocioEconomique.Subsistance),
            NbInformelBas = _menages.Count(m => m.Classe == ClasseSocioEconomique.InformelBas),
            NbFormelBas = _menages.Count(m => m.Classe == ClasseSocioEconomique.FormelBas),
            NbFormelQualifie = _menages.Count(m => m.Classe == ClasseSocioEconomique.FormelQualifie),
            NbCadre = _menages.Count(m => m.Classe == ClasseSocioEconomique.Cadre),

            // Validation cohérence CA
            NbEntreprisesAssujettiesTVA = toutesEntreprisesRef.Count(e => e.DoitCollecterTVA(_jourCourant)),
            PourcentageCoherenceCA = toutesEntreprisesRef.Count > 0 
                ? toutesEntreprisesRef.Count(e => 
                {
                    var ratio = e.VerifierCoherenceCA(_jourCourant);
                    return ratio >= 0.5 && ratio <= 2.0;
                }) / (double)toutesEntreprisesRef.Count * 100.0
                : 100.0,

            CAMoyenParEmploye = CalculerCAMoyenParEmploye(toutesEntreprisesRef),
            CAMoyenParEmployeTextile = CalculerCAMoyenParEmploye(
                toutesEntreprisesRef.Where(e => e.SecteurActivite == ESecteurActivite.Textiles).ToList()),
            CAMoyenParEmployeCommerce = CalculerCAMoyenParEmploye(
                toutesEntreprisesRef.Where(e => e.SecteurActivite == ESecteurActivite.Commerces 
                                              || e.SecteurActivite == ESecteurActivite.Services).ToList()),
            CAMoyenParEmployeMinier = CalculerCAMoyenParEmploye(
                toutesEntreprisesRef.Where(e => e.SecteurActivite == ESecteurActivite.SecteurMinier).ToList()),

            // Secteur Bancaire et Masse Monétaire
            MasseMonetaireM3 = _banque.MasseMonetaireM3,
            TotalDepotsBancaires = _banque.TotalDepotsMenages + _banque.TotalDepotsEntreprises,
            TotalCreditsAccordes = _banque.TotalCreditsAccordes,

            // Achat alimentaire journalier (IHouseholdModule.AcheteProduitsAlimentaires)
            DepensesAlimentairesTotales       = resultsMenages.Sum(r => r.DepensesAlimentaires),
            DepensesAlimentairesInformelTotal = resultsMenages.Sum(r => r.DepensesAlimentairesInformel),
            DepensesAlimentairesFormelTotal   = resultsMenages.Sum(r => r.DepensesAlimentairesFormel),
            FacteurReductionAlimentaireMoyen  = resultsMenages.Count > 0
                ? resultsMenages.Average(r => r.ReductionQuantiteAlimentaire)
                : 1.0,

            // Loisirs et vacances
            DepensesLoisirsTotales = resultsMenages.Sum(r => r.DepensesLoisirs),
            NbMenagesEnSortie = resultsMenages.Count(r => r.EstEnSortie),
            NbMenagesEnVacances = resultsMenages.Count(r => r.EstEnVacances),
            FacteurReductionLoisirsMoyen = resultsMenages.Count > 0
                ? resultsMenages.Average(r => r.FacteurReductionLoisirs)
                : 1.0,

            // Transport (ITransportationModule)
            DepensesTransportTotales = resultsMenages.Sum(r => r.DepensesTransport + r.DepensesTransportJirama),
            DepensesTransportInformel = totalTransportInformel,
            DepensesTransportFormel = totalTransportFormel,
            DepensesTransportCarburant = totalTransportCarburant,

            // Tourisme & Emploi formel (recalibration mensuelle)
            RecettesTourismeCumulees = _entreprisesTourisme.Sum(e => e.ChiffreAffairesCumule),
            NbSalariesSecteurFormel = toutesEntreprisesRef
                .Where(e => !e.EstInformel)
                .Sum(e => e.NombreEmployes),
            NbEntreprisesTourisme = _entreprisesTourisme.Count,

            // ─── Secteur informel — Dualité structurelle ───────────────────────
            NbMenagesInformels = _menages.Count(m => m.EstDansSecteurInformel),
            NbMenagesAutoEmploi = _menages.Count(m => m.EstAutoEmploi),
            RevenusInformelsAnnexesJour = _menages.Sum(m => m.RevenuInformelJournalier),

            NbEmployesSecteurInformel = toutesEntreprisesRef
                .Where(e => e.EstInformel)
                .Sum(e => e.NombreEmployes),
            TauxEmploiInformel = ComputeTauxEmploiInformel(toutesEntreprisesRef),

            ChiffreAffairesInformel = ComputeSumBySecteur(tousResultsEntreprises, toutesEntreprisesRef, informel: true, r => r.VentesB2C + r.VentesB2B),
            ChiffreAffairesFormel = ComputeSumBySecteur(tousResultsEntreprises, toutesEntreprisesRef, informel: false, r => r.VentesB2C + r.VentesB2B),

            ValeurAjouteeInformel = ComputeSumBySecteur(tousResultsEntreprises, toutesEntreprisesRef, informel: true, r => r.ValeurAjoutee),
            ValeurAjouteeFormel = ComputeSumBySecteur(tousResultsEntreprises, toutesEntreprisesRef, informel: false, r => r.ValeurAjoutee),

            PartInformelDansPIB = pibResult.PIBDemande > 0
                ? ComputeSumBySecteur(tousResultsEntreprises, toutesEntreprisesRef, informel: true, r => r.ValeurAjoutee) / pibResult.PIBDemande
                : 0,

            ProductiviteMoyenneInformel = ComputeProductiviteMoyenne(toutesEntreprisesRef, informel: true),
            ProductiviteMoyenneFormel = ComputeProductiviteMoyenne(toutesEntreprisesRef, informel: false),

            TresorerieMoyenneInformel = ComputeTresorerieMoyenne(toutesEntreprisesRef, informel: true),
            TresorerieMoyenneFormel = ComputeTresorerieMoyenne(toutesEntreprisesRef, informel: false),

            RevenuMoyenMenagesInformels = ComputeRevenuMoyenMenages(_menages, informel: true),
            RevenuMoyenMenagesFormels = ComputeRevenuMoyenMenages(_menages, informel: false)
        };

        _result.Snapshots.Add(snapshot);
        _result.JoursSimules = _jourCourant;

        // ═══════════════════════════════════════════════════════════════
        //  RECALIBRATION MENSUELLE
        //  À chaque fin de mois (jour 30, 60, 90…), comparer les résultats
        //  simulés aux données macro observées et ajuster les paramètres.
        // ═══════════════════════════════════════════════════════════════
        if (_config.RecalibrationMensuelleActivee
            && _jourCourant > 0
            && _jourCourant % 30 == 0)
        {
            int mois = _jourCourant / 30;
            var cible = _config.CiblesMensuelles.FirstOrDefault(c => c.Mois == mois);
            if (cible != null)
            {
                var evt = RecalibrationEngine.Recalibrer(new RecalibrationContext
                {
                    JourCourant = _jourCourant,
                    Mois = mois,
                    Cible = cible,
                    SnapshotActuel = snapshot,
                    Etat = _etat,
                    Config = _config,
                    Entreprises = _entreprises,
                    Importateurs = _importateurs,
                    Exportateurs = _exportateurs,
                    FacteurEchelle = _facteurEchelle
                });

                if (evt != null)
                {
                    _result.CalibrationEvents.Add(evt);
                }
            }
        }
    }

    private static ESecteurActivite SecteurDepuisCategorieExport(ECategorieExport categorie) => categorie switch
    {
        ECategorieExport.ProduitsMiniers => ESecteurActivite.SecteurMinier,
        ECategorieExport.ZonesFranches => ESecteurActivite.Textiles,
        _ => ESecteurActivite.Commerces
    };

    /// <summary>
    /// Calcule le CA moyen par employé par jour pour un ensemble d'entreprises.
    /// </summary>
    private double CalculerCAMoyenParEmploye(List<AgentCompany> entreprises)
    {
        if (entreprises.Count == 0 || _jourCourant == 0) return 0;

        var totalEmployes = entreprises.Sum(e => e.NombreEmployes);
        var totalCA = entreprises.Sum(e => e.ChiffreAffairesCumule);

        return totalEmployes > 0 ? totalCA / totalEmployes / _jourCourant : 0;
    }

    /// <summary>
    /// Détermine le jour de la semaine (1=Lundi .. 7=Dimanche) à partir du jour courant.
    /// Hypothèse : Jour 1 de la simulation = Lundi.
    /// </summary>
    private static int JourDeLaSemaine(int jourCourant)
    {
        // jourCourant commence à 1
        // 1â†’Lun(1), 2â†’Mar(2), ..., 5â†’Ven(5), 6â†’Sam(6), 7â†’Dim(7), 8â†’Lun(1)...
        int j = ((jourCourant - 1) % 7) + 1;
        return j;
    }

    /// <summary>
    /// Indique si le jour courant est un jour ouvrable (Lundi-Vendredi).
    /// Le samedi (6) et le dimanche (7) sont des jours de repos.
    /// </summary>
    private static bool EstJourOuvrable(int jourCourant)
    {
        int jourSemaine = JourDeLaSemaine(jourCourant);
        return jourSemaine <= 5; // 1-5 = Lun-Ven
    }

    /// <summary>
    /// Indique si une entreprise travaille ce jour selon son secteur.
    /// - Commerces : ouverts 7j/7
    /// - Autres secteurs (Textiles, Services, Minier) : fermés le weekend
    /// </summary>
    private static bool EntrepriseTravailleCeJour(int jourCourant, ESecteurActivite secteur)
    {
        // Les commerces et l'hôtellerie/tourisme travaillent 7j/7
        if (secteur == ESecteurActivite.Commerces || secteur == ESecteurActivite.HotellerieTourisme)
            return true;

        // Les autres secteurs ne travaillent que du lundi au vendredi
        return EstJourOuvrable(jourCourant);
    }

    /// <summary>
    /// Retourne le nom du jour de la semaine.
    /// </summary>
    private static string NomJourSemaine(int jourCourant) => JourDeLaSemaine(jourCourant) switch
    {
        1 => "Lundi",
        2 => "Mardi",
        3 => "Mercredi",
        4 => "Jeudi",
        5 => "Vendredi",
        6 => "Samedi",
        7 => "Dimanche",
        _ => "?"
    };

    // ─── Helpers secteur informel ───────────────────────────────────────────

    private static double ComputeTauxEmploiInformel(List<AgentCompany> entreprises)
    {
        int nbInf = entreprises.Where(e => e.EstInformel).Sum(e => e.NombreEmployes);
        int nbForm = entreprises.Where(e => !e.EstInformel).Sum(e => e.NombreEmployes);
        int total = nbInf + nbForm;
        return total > 0 ? (double)nbInf / total : 0;
    }

    private static double ComputeSumBySecteur(
        List<CompanyDailyResult> results,
        List<AgentCompany> entreprises,
        bool informel,
        Func<CompanyDailyResult, double> selector)
    {
        if (results.Count != entreprises.Count) return 0;
        double sum = 0;
        for (int i = 0; i < results.Count; i++)
        {
            if (entreprises[i].EstInformel == informel)
                sum += selector(results[i]);
        }
        return sum;
    }

    private static double ComputeProductiviteMoyenne(List<AgentCompany> entreprises, bool informel)
    {
        var subset = entreprises.Where(e => e.EstInformel == informel && e.NombreEmployes > 0).ToList();
        return subset.Count > 0
            ? subset.Average(e => e.ProductiviteParEmployeJour)
            : 0;
    }

    private static double ComputeTresorerieMoyenne(List<AgentCompany> entreprises, bool informel)
    {
        var subset = entreprises.Where(e => e.EstInformel == informel).ToList();
        return subset.Count > 0
            ? subset.Average(e => e.Tresorerie)
            : 0;
    }

    private static double ComputeRevenuMoyenMenages(List<AgentHousehold> menages, bool informel)
    {
        var subset = menages.Where(m => m.EstDansSecteurInformel == informel).ToList();
        return subset.Count > 0
            ? subset.Average(m => m.SalaireJournalier + m.RevenuInformelJournalier)
            : 0;
    }

    /// <summary>
    /// Applique les valeurs de propension marginale à consommer par classe
    /// aux ménages déjà initialisés dans la simulation.
    /// </summary>
    public void AppliquerPropensionConsommationParClasse(ScenarioConfig config)
    {
        if (_menages == null || config == null) return;
        foreach (var m in _menages)
        {
            m.PropensionConsommation = config.GetPropensionParClasse(m.Classe);
        }
    }

    /// <summary>
    /// Valide les résultats simulés contre les données macro de référence.
    /// </summary>
    public MacroValidationReport ValiderMacro()
    {
        return new MacroValidationReport
        {
            ScoreGlobal = 0,
            Verdict = "Non implémenté",
            Alertes = new List<string>(),
            Recommandations = new List<string>()
        };
    }
}
































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































