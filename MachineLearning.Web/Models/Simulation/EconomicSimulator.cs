using MachineLearning.Web.Models.Simulation.Companies;
using MachineLearning.Web.Models.Simulation.Config;
using MachineLearning.Web.Models.Simulation.Household;

namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Moteur de simulation économique.
/// Orchestre les interactions entre ménages, entreprises et État.
/// 1 tick = 1 jour dans la vie économique.
/// </summary>
public class EconomicSimulator
{
    private readonly List<Household> _menages = [];
    private readonly List<Company> _entreprises = [];
    private readonly List<Importer> _importateurs = [];
    private readonly List<Exporter> _exportateurs = [];
    private Government _etat = new();
    private ScenarioConfig _config = new();
    private SimulationResult _result = new();
    private CancellationTokenSource? _cts;
    private int _jourCourant;
    private SalaryDistribution _distribution = new();
    private DistributionStats _statsInitiales = new();

    public SimulationResult Result => _result;
    public bool EnCours => _result.EnCours;
    public int JourCourant => _jourCourant;
    public DistributionStats StatsInitiales => _statsInitiales;

    public event Action? OnTickCompleted;

    /// <summary>
    /// Initialise la simulation avec un scénario donné.
    /// </summary>
    public void Initialiser(ScenarioConfig config)
    {
        _config = config;
        _jourCourant = 0;

        Household.ResetIdCounter();
        Company.ResetIdCounter();

        // Créer la distribution salariale à partir du scénario
        _distribution = new SalaryDistribution
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

        // Créer les entreprises (normales, importateurs, exportateurs)
        string[] nomsEntreprises = [
            "Telma", "Jirama", "Star Brasseries", "BNI Madagascar",
            "Ambatovy", "Socota", "Air Madagascar", "Colas Madagascar",
            "Henri Fraise", "Groupe Sipromad"
        ];

        string[] nomsImportateurs = [
            "Galana", "Jovenna", "Total Énergies MG", "SMTP Import",
            "Galaxy Électronique", "CFAO Motors", "Madarail Import"
        ];

        string[] nomsExportateurs = [
            "Trimeta Agro", "Bourbon Madagascar", "Aquamen (crevettes)",
            "SIRAMA Vanille", "Ambatovy Mining", "Cotona Textile"
        ];

        int nbImportateurs = Math.Max(1, (int)(_config.NombreEntreprises * _config.TauxImportateurs));
        int nbExportateurs = Math.Max(1, (int)(_config.NombreEntreprises * _config.TauxExportateurs));
        int nbEntreprisesNormales = _config.NombreEntreprises - nbImportateurs - nbExportateurs;
        if (nbEntreprisesNormales < 1) nbEntreprisesNormales = 1;

        int totalEntreprises = nbEntreprisesNormales + nbImportateurs + nbExportateurs;
        int employesParEntreprise = _config.NombreMenages / Math.Max(totalEntreprises, 1);
        var random = new Random(42);

        var categoriesImport = Enum.GetValues<CategorieImport>();
        var categoriesExport = Enum.GetValues<CategorieExport>();

        // Importateurs
        for (int i = 0; i < nbImportateurs; i++)
        {
            var imp = new Importer
            {
                Name = i < nomsImportateurs.Length ? nomsImportateurs[i] : $"Importateur {i + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                ProductiviteParEmployeJour = _config.ProductiviteParEmploye * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = 8_000_000 * (0.5 + random.NextDouble()),
                Categorie = categoriesImport[random.Next(categoriesImport.Length)],
                ValeurCIFJour = 300_000 + random.NextDouble() * 700_000,
                MargeReventeImport = 0.20 + random.NextDouble() * 0.15
            };
            _importateurs.Add(imp);
        }

        // Exportateurs — distribués par catégorie selon la config
        var repartition = _config.RepartitionExportCategories;
        double totalPoids = repartition.Values.Sum();
        int exporteurCree = 0;

        foreach (var (categorie, poids) in repartition)
        {
            int nbPourCategorie = Math.Max(1, (int)(nbExportateurs * poids / totalPoids));
            if (exporteurCree + nbPourCategorie > nbExportateurs)
                nbPourCategorie = nbExportateurs - exporteurCree;
            if (nbPourCategorie <= 0) continue;

            double fobJourCategorie = _config.FOBJourParCategorie.GetValueOrDefault(categorie, 200);

            for (int i = 0; i < nbPourCategorie; i++)
            {
                var exp = new Exporter
                {
                    Name = exporteurCree < nomsExportateurs.Length
                        ? nomsExportateurs[exporteurCree]
                        : $"Export-{categorie}-{i + 1}",
                    NombreEmployes = employesParEntreprise,
                    SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                    MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                    ProductiviteParEmployeJour = _config.ProductiviteParEmploye * (0.8 + random.NextDouble() * 0.4),
                    Tresorerie = 6_000_000 * (0.5 + random.NextDouble()),
                    Categorie = categorie,
                    // Config en millions MGA/jour → Exporter en MGA brut/jour
                    ValeurFOBJour = fobJourCategorie * 1_000_000 * (0.8 + random.NextDouble() * 0.4),
                    PartExport = 0.50 + random.NextDouble() * 0.40
                };
                _exportateurs.Add(exp);
                exporteurCree++;
            }
        }

        // Compléter si on n'a pas atteint le nb total d'exportateurs
        while (exporteurCree < nbExportateurs)
        {
            var cat = categoriesExport[random.Next(categoriesExport.Length)];
            double fobBase = _config.FOBJourParCategorie.GetValueOrDefault(cat, 200);
            var exp = new Exporter
            {
                Name = $"Exportateur {exporteurCree + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                ProductiviteParEmployeJour = _config.ProductiviteParEmploye * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = 6_000_000 * (0.5 + random.NextDouble()),
                Categorie = cat,
                // Config en millions MGA/jour → Exporter en MGA brut/jour
                ValeurFOBJour = fobBase * 1_000_000 * (0.8 + random.NextDouble() * 0.4),
                PartExport = 0.50 + random.NextDouble() * 0.40
            };
            _exportateurs.Add(exp);
            exporteurCree++;
        }

        // Entreprises normales (marché local)
        for (int i = 0; i < nbEntreprisesNormales; i++)
        {
            var entreprise = new Company
            {
                Name = i < nomsEntreprises.Length ? nomsEntreprises[i] : $"Entreprise {i + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (0.9 + random.NextDouble() * 0.2),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = _config.ProductiviteParEmploye * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = 5_000_000 * (0.5 + random.NextDouble())
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
        var toutesEntreprises = new List<Company>();
        toutesEntreprises.AddRange(_entreprises);
        toutesEntreprises.AddRange(_importateurs);
        toutesEntreprises.AddRange(_exportateurs);
        int entrepriseIndex = 0;

        for (int i = 0; i < _config.NombreMenages; i++)
        {
            bool estHomme = random.NextDouble() > 0.5;
            var prenoms = estHomme ? prenomsHommes : prenomsFemmes;

            // Tirer un salaire de la distribution log-normale
            double salaire = _distribution.TirerSalaire(random);
            salairesGeneres[i] = salaire;

            // Déterminer la classe socio-économique
            var classe = _distribution.DeterminerClasse(salaire);
            var comportement = SalaryDistribution.ComportementParClasse(classe, random);

            var menage = new Household
            {
                Name = $"{prenoms[random.Next(prenoms.Length)]} {noms[random.Next(noms.Length)]}",
                SalaireMensuel = salaire,
                Classe = classe,
                TauxEpargne = comportement.TauxEpargne,
                PropensionConsommation = comportement.PropensionConsommation,
                Epargne = comportement.EpargneInitiale,
                EstEmploye = random.NextDouble() < comportement.ProbabiliteEmploi,
                EmployeurId = toutesEntreprises[entrepriseIndex].Id,
                DepensesAlimentairesJour = comportement.DepensesAlimentairesJour,
                DepensesDiversJour = comportement.DepensesDiversJour,
                Transport = comportement.Transport,
                DistanceDomicileTravailKm = comportement.DistanceDomicileTravailKm
            };
            _menages.Add(menage);

            entrepriseIndex = (entrepriseIndex + 1) % toutesEntreprises.Count;
        }

        // Calculer les statistiques initiales de la distribution
        _statsInitiales = SalaryDistribution.CalculerStats(salairesGeneres);

        // Initialiser l'État
        _etat = new Government
        {
            TauxIS = _config.TauxIS,
            TauxTVA = _config.TauxTVA,
            TauxDirecteur = _config.TauxDirecteur,
            TauxInflation = _config.TauxInflation,
            DepensesPubliquesJour = _config.DepensesPubliquesJour
        };

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
    /// Retourne le FOB total cumulé par catégorie d'export (en MGA brut).
    /// Utilisé par le calibrateur pour comparer avec les données INSTAT.
    /// </summary>
    public Dictionary<CategorieExport, double> GetFOBParCategorie()
    {
        var result = new Dictionary<CategorieExport, double>();
        foreach (CategorieExport cat in Enum.GetValues<CategorieExport>())
            result[cat] = 0;

        foreach (var exp in _exportateurs)
            result[exp.Categorie] += exp.TotalExportationsFOB;

        return result;
    }

    /// <summary>FOB total tous exportateurs confondus.</summary>
    public double GetFOBTotal() => _exportateurs.Sum(e => e.TotalExportationsFOB);

    /// <summary>
    /// Simule un seul jour de l'économie.
    /// </summary>
    private void SimulerUnJour()
    {
        _jourCourant++;

        // 1. Les ménages travaillent et consomment
        var resultsMenages = new List<DailyHouseholdResult>();
        double demandeConsommationTotale = 0;

        foreach (var menage in _menages)
        {
            var r = menage.SimulerJournee(_etat, _config.PrixCarburantLitre);
            resultsMenages.Add(r);
            demandeConsommationTotale += r.Consommation;
        }

        // 2. Les entreprises produisent et vendent
        var resultsEntreprises = new List<DailyCompanyResult>();
        var resultsImportateurs = new List<DailyImporterResult>();
        var resultsExportateurs = new List<DailyExporterResult>();

        int totalEntreprises = _entreprises.Count + _importateurs.Count + _exportateurs.Count;
        double demandeParEntreprise = demandeConsommationTotale / Math.Max(totalEntreprises, 1);

        foreach (var entreprise in _entreprises)
        {
            var r = entreprise.SimulerJournee(
                demandeParEntreprise,
                _etat.TauxIS,
                _etat.TauxTVA,
                _etat.TauxInflation,
                _etat.TauxDirecteur
            );
            resultsEntreprises.Add(r);
        }

        // Importateurs
        foreach (var importateur in _importateurs)
        {
            var r = importateur.SimulerJourneeImport(
                demandeParEntreprise,
                _etat.TauxIS,
                _etat.TauxTVA,
                _etat.TauxInflation,
                _etat.TauxDirecteur,
                _config.TauxDroitsDouane,
                _config.TauxAccise
            );
            resultsImportateurs.Add(r);
        }

        // Exportateurs
        foreach (var exportateur in _exportateurs)
        {
            var r = exportateur.SimulerJourneeExport(
                demandeParEntreprise,
                _etat.TauxIS,
                _etat.TauxTVA,
                _etat.TauxInflation,
                _etat.TauxDirecteur,
                _config.TauxTaxeExport
            );
            resultsExportateurs.Add(r);
        }

        // 3. L'État consolide (entreprises + importateurs + exportateurs)
        var resultEtat = _etat.SimulerJournee(resultsMenages, resultsEntreprises, resultsImportateurs, resultsExportateurs);

        // 4. Redistribution des transferts sociaux aux ménages les plus pauvres
        double transfertParMenage = resultEtat.TransfertsSociaux / _menages.Count;
        foreach (var menage in _menages)
        {
            menage.Epargne += transfertParMenage;
        }

        // 5. Créer le snapshot
        // Calculer les métriques de distribution des épargnes
        var epargnesSorted = _menages.OrderBy(m => m.Epargne).Select(m => m.Epargne).ToArray();
        var distStats = SalaryDistribution.CalculerStats(epargnesSorted);
        int n = _menages.Count;

        // Agréger toutes les entreprises pour les métriques
        var toutesEntreprisesRef = new List<Company>();
        toutesEntreprisesRef.AddRange(_entreprises);
        toutesEntreprisesRef.AddRange(_importateurs);
        toutesEntreprisesRef.AddRange(_exportateurs);

        var tousResultsEntreprises = new List<DailyCompanyResult>();
        tousResultsEntreprises.AddRange(resultsEntreprises);
        tousResultsEntreprises.AddRange(resultsImportateurs);
        tousResultsEntreprises.AddRange(resultsExportateurs);

        var snapshot = new DailySnapshot
        {
            Jour = _jourCourant,

            // Ménages
            EpargneMoyenneMenages = _menages.Average(m => m.Epargne),
            EpargneTotaleMenages = _menages.Sum(m => m.Epargne),
            ConsommationTotaleMenages = resultsMenages.Sum(r => r.Consommation),
            RevenuTotalMenages = resultsMenages.Sum(r => r.RevenuBrut),
            TauxEmploi = _menages.Count(m => m.EstEmploye) / (double)_menages.Count,

            // Entreprises (toutes confondues)
            ChiffreAffairesTotalEntreprises = tousResultsEntreprises.Sum(r => r.VentesB2C + r.VentesB2B),
            TresorerieMoyenneEntreprises = toutesEntreprisesRef.Average(e => e.Tresorerie),
            BeneficeTotalEntreprises = tousResultsEntreprises.Sum(r => r.BeneficeAvantImpot),
            VentesB2CTotales = tousResultsEntreprises.Sum(r => r.VentesB2C),
            VentesB2BTotales = tousResultsEntreprises.Sum(r => r.VentesB2B),

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
            SoldeBudgetaire = _etat.SoldeBudgetaire,
            DettePublique = _etat.DettePublique,

            // PIB proxy = C + I + G + (X - M)
            PIBProxy = resultsMenages.Sum(r => r.Consommation)
                     + tousResultsEntreprises.Sum(r => r.VentesB2B)
                     + resultEtat.DepensesPubliques
                     + resultEtat.BalanceCommerciale,

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
            NbCadre = _menages.Count(m => m.Classe == ClasseSocioEconomique.Cadre)
        };

        _result.Snapshots.Add(snapshot);
        _result.JoursSimules = _jourCourant;
    }
}
