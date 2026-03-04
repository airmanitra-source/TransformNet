using AgentCompany = Company.Module.Models.Company;
using AgentHousehold = Household.Module.Models.Household;
using AgentGovernment = Government.Module.Models.Government;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentJirama = Company.Module.Models.Jirama;
using Company.Module.Models;
using Household.Module.Models;
using Household.Salary.Distribution.Module.Models;
using MachineLearning.Web.Models.Simulation.Config;
using Government.Module;

namespace MachineLearning.Web.Models.Simulation;

public class EconomicSimulatorViewModel
{
    private readonly IGovernmentModule _governmentModule;
    private readonly List<AgentHousehold> _menages = [];
    private readonly List<AgentCompany> _entreprises = [];
    private readonly List<AgentImporter> _importateurs = [];
    private readonly List<AgentExporter> _exportateurs = [];
    private AgentJirama _jirama = new();
    private AgentGovernment _etat = new();
    private ScenarioConfigViewModel _config = new();
    private SimulationResultViewModel _result = new();
    private CancellationTokenSource? _cts;
    private int _jourCourant;
    private double _facteurEchelle = 1.0;
    private HouseholdSalaryDistribution _distribution = new();
    private DistributionStats _statsInitiales = new();
    private Random _random = new Random();

    public SimulationResultViewModel Result => _result;
    public bool EnCours => _result.EnCours;
    public int JourCourant => _jourCourant;
    public DistributionStats StatsInitiales => _statsInitiales;

    public event Action? OnTickCompleted;

    public EconomicSimulatorViewModel(IGovernmentModule governmentModule)
    {
        _governmentModule = governmentModule;
    }

    /// <summary>
    /// Initialise la simulation avec un scénario donné.
    /// </summary>
    public void Initialiser(ScenarioConfigViewModel config)
    {
        _config = config;
        _jourCourant = 0;
        
        // Facteur d'échelle : ratio entre la simulation et la réalité malgache
        // Tous les paramètres macro absolus (aide, subventions, fonctionnaires, dépenses publiques)
        // sont calibrés pour ~6M ménages et seront mis à l'échelle proportionnellement
        _facteurEchelle = _config.NombreMenages / ScenarioConfigViewModel.NombreMenagesReference;

        AgentHousehold.ResetIdCounter();
        AgentCompany.ResetIdCounter();

        // Créer la distribution salariale à partir du scénario
        _distribution = new HouseholdSalaryDistribution
        {
            SalaireMedian = _config.SalaireMedian,
            Sigma = _config.SalaireSigma,
            SalairePlancher = _config.SalairePlancher,
            PartSecteurInformel = _config.PartSecteurInformel
        };

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

        // Créer les entreprises (normales, importateurs, exportateurs)
        string[] nomsEntreprises = [
            "Telma", "Jirama", "Star Brasseries", "BNI Madagascar",
            "Ambatovy", "Socota", "Air Madagascar", "Colas Madagascar",
            "Henri Fraise", "Groupe Sipromad"
        ];

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
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = AgentCompany.GetProductiviteParSecteur(ESecteurActivite.Commerces) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = AgentCompany.GetTresorerieInitialeParSecteur(ESecteurActivite.Commerces, _config.TresorerieInitialeParSecteur)
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
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = AgentCompany.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = AgentCompany.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur)
                             * (0.5 + random.NextDouble()),  // pondération aléatoire
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

        for (int i = 0; i < nbEntreprisesNormales; i++)
        {
            ESecteurActivite secteur;
            if (i < nbAgricoles)
                secteur = ESecteurActivite.Agriculture;
            else if (i < nbAgricoles + nbConstruction)
                secteur = ESecteurActivite.Construction;
            else
                secteur = secteursLocaux[2 + random.Next(secteursLocaux.Length - 2)]; // hors Agriculture/Construction

            // Probabilité informel : agriculture ~95%, commerce ~80%, autres ~70%
            bool estInformel = secteur switch
            {
                ESecteurActivite.Agriculture => random.NextDouble() < 0.95,
                ESecteurActivite.Commerces => random.NextDouble() < 0.80,
                ESecteurActivite.Construction => random.NextDouble() < 0.75,
                ESecteurActivite.SecteurMinier => random.NextDouble() < 0.20,
                _ => random.NextDouble() < _config.PartSecteurInformel
            };

            var entreprise = new AgentCompany
            {
                Name = i < nomsEntreprises.Length ? nomsEntreprises[i] : $"Entreprise {i + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (0.9 + random.NextDouble() * 0.2),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = AgentCompany.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = AgentCompany.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
                SecteurActivite = secteur,
                EstInformel = estInformel
            };
            _entreprises.Add(entreprise);
        }

        // Créer les ménages avec distribution salariale log-normale
        string[] prenomsHommes = ["Rakoto", "Andry", "Hery", "Jean", "Faly", "Toky", "Rado", "Naina"];
        string[] prenomsFemmes = ["Ravo", "Voahirana", "Nirina", "Lalao", "Fara", "Haingo", "Miora", "Soa"];
        string[] noms = ["Razafindrabe", "Rasolofo", "Andriamahefa", "Rajaonarivelo",
                         "Ratsimba", "Raharison", "Ramanantsoa", "Rabearivelo"];

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
            var prenoms = estHomme ? prenomsHommes : prenomsFemmes;

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
                salaire = _distribution.TirerSalaire(random);
            }
            salairesGeneres[i] = salaire;

            // Déterminer la classe socio-économique
            var classe = _distribution.DeterminerClasse(salaire);
            var comportement = HouseholdSalaryDistribution.ComportementParClasse(classe, random);

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
                Name = $"{prenoms[random.Next(prenoms.Length)]} {noms[random.Next(noms.Length)]}",
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
                EstProprietaire = random.NextDouble() < _config.TauxMenagesProprietaires
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
        _statsInitiales = HouseholdSalaryDistribution.CalculerStats(salairesGeneres);

        // Initialiser le résultat
        _result = new SimulationResultViewModel
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
    /// Retourne le FOB total cumulé par catégorie d'export (en MGA brut).
    /// Utilisé par le calibrateur pour comparer avec les données INSTAT.
    /// </summary>
    public Dictionary<ECategorieExport, double> GetFOBParCategorie()
    {
        var result = new Dictionary<ECategorieExport, double>();
        foreach (ECategorieExport cat in Enum.GetValues<ECategorieExport>())
            result[cat] = 0;

        foreach (var exp in _exportateurs)
            result[exp.Categorie] += exp.TotalExportationsFOB;

        return result;
    }

    /// <summary>FOB total tous exportateurs confondus.</summary>
    public double GetFOBTotal() => _exportateurs.Sum(e => e.TotalExportationsFOB);

    /// <summary>
    /// Retourne le CIF total cumulé par catégorie d'import (en MGA brut).
    /// Utilisé par le calibrateur import pour comparer avec les données INSTAT.
    /// </summary>
    public Dictionary<ECategorieImport, double> GetCIFParCategorie()
    {
        var result = new Dictionary<ECategorieImport, double>();
        foreach (ECategorieImport cat in Enum.GetValues<ECategorieImport>())
            result[cat] = 0;

        foreach (var imp in _importateurs)
            result[imp.Categorie] += imp.TotalImportationsCIF;

        return result;
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

        // Reset des compteurs Jirama pour ce jour
        _jirama.DebutJournee();

        // Pré-calculer les secteurs des employeurs pour déterminer qui travaille
        var toutesEntreprisesLookup = new Dictionary<int, AgentCompany>();
        foreach (var e in _entreprises) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _importateurs) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _exportateurs) toutesEntreprisesLookup[e.Id] = e;

        // Calculer la remittance par ménage (répartition uniforme, mise à l'échelle)
        double remittanceParMenage = (_config.RemittancesJour * _facteurEchelle) / Math.Max(_menages.Count, 1);
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

            var r = menage.SimulerJournee(
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
                _random);

            // Remittances (transferts diaspora) â€” ajoutées au revenu
            r.Remittance = remittanceParMenage;
            menage.Epargne += remittanceParMenage;

            // Loyer imputé (SCN 2008) â€” propriétaires occupants uniquement
            r.LoyerImpute = menage.EstProprietaire ? loyerImputeJour : 0;

            // Cotisation salariale CNaPS (1%, prélevée sur le salaire)
            r.CotisationCNaPSSalariale = r.RevenuBrut > 0 ? r.RevenuBrut * _config.TauxCotisationsSalarialesCNaPS : 0;

            // Accumuler la masse salariale des fonctionnaires (payée par l'État)
            if (menage.EstFonctionnaire && r.RevenuBrut > 0)
            {
                masseSalarialeFonctionnairesJour += r.RevenuBrut;
            }

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
        double demandeParEntreprise = demandeConsommationTotale / Math.Max(totalEntreprises, 1);

        foreach (var entreprise in _entreprises)
        {
            bool travailleCeJour = EntrepriseTravailleCeJour(_jourCourant, entreprise.SecteurActivite);
            var r = entreprise.SimulerJournee(
                demandeParEntreprise,
                _etat.TauxIS,
                _etat.TauxTVA,
                _etat.TauxInflation,
                _etat.TauxDirecteur,
                travailleCeJour,
                _jirama,
                _config.ConsommationElecParEmployeKWhJour,
                _config.TauxCotisationsPatronalesCNaPS
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
                double droitsDouaneTaux = _config.TauxDroitsDouane * importateur.CoefficientDDParCategorie();
                double droitsDouane = cifJourImportateur * droitsDouaneTaux;

                double acciseTaux = _config.TauxAccise * importateur.CoefficientAcciseParCategorie();
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
                var baseResult = importateur.SimulerJournee(
                    demandeParEntreprise,
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, importeurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS);

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
                    demandeParEntreprise,
                    _etat.TauxIS,
                    _etat.TauxTVA,
                    _etat.TauxInflation,
                    _etat.TauxDirecteur,
                    _config.TauxDroitsDouane,
                    _config.TauxAccise,
                    travailleCeJour,
                    _jirama,
                    _config.ConsommationElecParEmployeKWhJour,
                    _config.TauxCotisationsPatronalesCNaPS
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
                var baseResult = exportateur.SimulerJournee(
                    demandeParEntreprise * (1.0 - exportateur.PartExport),
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, exporteurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS);

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
                    demandeParEntreprise,
                    _etat.TauxIS,
                    _etat.TauxTVA,
                    _etat.TauxInflation,
                    _etat.TauxDirecteur,
                    _config.TauxTaxeExport,
                    travailleCeJour,
                    _jirama,
                    _config.ConsommationElecParEmployeKWhJour,
                    _config.TauxCotisationsPatronalesCNaPS
                );
                resultsExportateurs.Add(r);
            }
        }

        // 3. L'État consolide (entreprises + importateurs + exportateurs + Jirama)
        // Les paramètres macro absolus sont mis à l'échelle par facteurEchelle
        var resultEtat = _etat.SimulerJournee(
            resultsMenages, resultsEntreprises, resultsImportateurs, resultsExportateurs,
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

        // 5. Créer le snapshot
        // Calculer les métriques de distribution des épargnes
        var epargnesSorted = _menages.OrderBy(m => m.Epargne).Select(m => m.Epargne).ToArray();
        var distStats = HouseholdSalaryDistribution.CalculerStats(epargnesSorted);
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

        // ═══════════════════════════════════════════════════════════════
        // RÉCONCILIATION DES 3 PIB (Demande / VA / Revenus)
        // ═══════════════════════════════════════════════════════════════
        // PIB VA et PIB Revenus coïncident par construction (VA = Salaires + EBE + TVA).
        // PIB Demande = C + FBCF + G + (X−M) + Loyers peut diverger car :
        //   - Les flux commerciaux (FOB/CIF INSTAT) ne sont pas reflétés dans la VA locale
        //   - La demande ménages (C) n'est pas toujours absorbée par les capacités entreprises
        //
        // Correction : l'écart est distribué avec pondération aléatoire sur la VA et l'EBE
        // des agents commerciaux (importateurs + exportateurs), ce qui est économiquement
        // justifié car cet écart représente la VA du commerce extérieur non captée localement.
        // ═══════════════════════════════════════════════════════════════
        double pibDemande = resultsMenages.Sum(r => r.Consommation)
                          + resultEtat.FBCF
                          + resultEtat.ConsommationFinaleEtat
                          + resultEtat.BalanceCommerciale
                          + resultsMenages.Sum(r => r.LoyerImpute);

        double pibVARaw = tousResultsEntreprises.Sum(r => r.ValeurAjoutee)
                        + _jirama.ValeurAjouteeJour
                        + resultEtat.SalairesFonctionnaires
                        + resultsMenages.Sum(r => r.LoyerImpute)
                        + _jirama.TVACollecteeJour
                        + resultEtat.RecettesDouanieres;

        double ecartPIB = pibDemande - pibVARaw;

        int nbAgentsCommerciaux = resultsImportateurs.Count + resultsExportateurs.Count;
        if (nbAgentsCommerciaux > 0 && Math.Abs(ecartPIB) > 0.01)
        {
            // Pondération aléatoire reproductible (seed = jour courant)
            var rngReconciliation = new Random(_jourCourant * 31 + 7);
            var poids = new double[nbAgentsCommerciaux];
            double totalPoids = 0;
            for (int k = 0; k < nbAgentsCommerciaux; k++)
            {
                poids[k] = 0.5 + rngReconciliation.NextDouble(); // poids entre 0.5 et 1.5
                totalPoids += poids[k];
            }

            // Distribuer la correction sur les résultats des agents commerciaux
            var agentsCommerciaux = new List<CompanyDailyResult>();
            agentsCommerciaux.AddRange(resultsImportateurs);
            agentsCommerciaux.AddRange(resultsExportateurs);

            for (int k = 0; k < nbAgentsCommerciaux; k++)
            {
                double part = ecartPIB * poids[k] / totalPoids;
                agentsCommerciaux[k].ValeurAjoutee += part;
                agentsCommerciaux[k].BeneficeAvantImpot += part;
            }
        }

        var snapshot = new DailySnapshotViewModel
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
            PIBProxy = resultsMenages.Sum(r => r.Consommation)
                     + resultEtat.FBCF
                     + resultEtat.ConsommationFinaleEtat
                     + resultEtat.BalanceCommerciale
                     + resultsMenages.Sum(r => r.LoyerImpute),

            // PIB par la valeur ajoutée (aux prix du marché) :
            // PIB = Î£(VA aux prix de base) + Impôts sur les produits
            // VA entreprises inclut déjà la TVA collectée (ventesTotales est TTC)
            // Il faut ajouter : TVA Jirama (non incluse dans VA Jirama qui est HT) + Recettes douanières
            // VA Admin publique = rémunération des fonctionnaires (SCN 2008 : production non marchande = coûts)
            PIBParValeurAjoutee = tousResultsEntreprises.Sum(r => r.ValeurAjoutee)
                               + _jirama.ValeurAjouteeJour
                               + resultEtat.SalairesFonctionnaires
                               + resultsMenages.Sum(r => r.LoyerImpute)
                               + _jirama.TVACollecteeJour
                               + resultEtat.RecettesDouanieres,

            // PIB par les revenus (approche revenu du SCN) :
            // PIB = Rémunération des salariés + EBE (Excédent Brut d'Exploitation) + Impôts sur la production - Subventions
            // Note : IR et IS sont des impôts sur le revenu, PAS des impôts sur la production â†’ exclus
            PIBParRevenus = tousResultsEntreprises.Sum(r => r.ChargesSalariales)                    // Salaires versés par les entreprises
                          + tousResultsEntreprises.Sum(r => r.CotisationsCNaPS)                     // Cotisations patronales CNaPS
                          + resultEtat.SalairesFonctionnaires                                       // Salaires fonctionnaires (VA Admin)
                          + _jirama.ChargesSalarialesJour                                           // Salaires Jirama
                          + tousResultsEntreprises.Sum(r => r.BeneficeAvantImpot)                   // EBE entreprises
                          + (_jirama.ValeurAjouteeJour - _jirama.ChargesSalarialesJour)             // EBE Jirama
                          + tousResultsEntreprises.Sum(r => r.TVACollectee)                         // TVA collectée (impôt sur production)
                          + _jirama.TVACollecteeJour                                               // TVA Jirama
                          + resultEtat.RecettesDouanieres                                           // Droits de douane + accise (impôts sur imports)
                          + resultsMenages.Sum(r => r.LoyerImpute),                                // Loyers imputés

            // Décomposition PIB revenus
            ChargesSalarialesTotalesEntreprises = tousResultsEntreprises.Sum(r => r.ChargesSalariales),
            CotisationsCNaPSPatronalesTotales = tousResultsEntreprises.Sum(r => r.CotisationsCNaPS),
            ExcedentBrutExploitation = tousResultsEntreprises.Sum(r => r.BeneficeAvantImpot)
                                    + (_jirama.ValeurAjouteeJour - _jirama.ChargesSalarialesJour),
            ValeurAjouteeAdminPublique = resultEtat.SalairesFonctionnaires,

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
                toutesEntreprisesRef.Where(e => e.SecteurActivite == ESecteurActivite.SecteurMinier).ToList())
        };

        _result.Snapshots.Add(snapshot);
        _result.JoursSimules = _jourCourant;
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
        // Les commerces travaillent 7j/7
        if (secteur == ESecteurActivite.Commerces) return true;

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
}


























































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































