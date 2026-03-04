using MachineLearning.Web.Models.Agents.Companies;
using MachineLearning.Web.Models.Agents.Government;
using MachineLearning.Web.Models.Agents.Household;
using MachineLearning.Web.Models.Simulation.Config;
using MachineLearning.Web.Models.Simulation.Models;

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
    private Jirama _jirama = new();
    private Government _etat = new();
    private ScenarioConfig _config = new();
    private SimulationResult _result = new();
    private CancellationTokenSource? _cts;
    private int _jourCourant;
    private double _facteurEchelle = 1.0;
    private HouseholdSalaryDistribution _distribution = new();
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

        // Facteur d'échelle : ratio entre la simulation et la réalité malgache
        // Tous les paramètres macro absolus (aide, subventions, fonctionnaires, dépenses publiques)
        // sont calibrés pour ~6M ménages et seront mis à l'échelle proportionnellement
        _facteurEchelle = _config.NombreMenages / ScenarioConfig.NombreMenagesReference;

        Household.ResetIdCounter();
        Company.ResetIdCounter();

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

        // Créer la JIRAMA (compagnie monopole eau + électricité)
        _jirama = new Jirama
        {
            Name = "JIRAMA",
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

        var categoriesImport = Enum.GetValues<ECategorieImport>();
        var categoriesExport = Enum.GetValues<ECategorieExport>();
        var secteursLocaux = new[]
        {
            ESecteurActivite.Agriculture,
            ESecteurActivite.Services,
            ESecteurActivite.Commerces,
            ESecteurActivite.Textiles,
            ESecteurActivite.SecteurMinier,
            ESecteurActivite.Construction
        };

        // Importateurs
        for (int i = 0; i < nbImportateurs; i++)
        {
            var categorie = categoriesImport[random.Next(categoriesImport.Length)];

            // Utiliser le CIF configuré par catégorie (en millions MGA â†’ MGA brut, mis à l'échelle)
            double cifJourCategorie = _config.CIFJourParCategorie.GetValueOrDefault(categorie, 500);
            double cifJourMGA = cifJourCategorie * 1_000_000 * _facteurEchelle * (0.8 + random.NextDouble() * 0.4);

            var imp = new Importer
            {
                Name = i < nomsImportateurs.Length ? nomsImportateurs[i] : $"Importateur {i + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                // Importateurs = Commerces
                ProductiviteParEmployeJour = Company.GetProductiviteParSecteur(ESecteurActivite.Commerces) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = Company.GetTresorerieInitialeParSecteur(ESecteurActivite.Commerces, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
                Categorie = categorie,
                SecteurActivite = ESecteurActivite.Commerces,
                ValeurCIFJour = cifJourMGA,
                MargeReventeImport = 0.20 + random.NextDouble() * 0.15
            };
            _importateurs.Add(imp);
        }

        // Exportateurs â€” distribués par catégorie selon la config
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
                var secteur = SecteurDepuisCategorieExport(categorie);
                var exp = new Exporter
                {
                    Name = exporteurCree < nomsExportateurs.Length
                        ? nomsExportateurs[exporteurCree]
                        : $"Export-{categorie}-{i + 1}",
                    NombreEmployes = employesParEntreprise,
                    SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                    MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                    // Utiliser productivité réaliste (Textile ou Minier)
                    ProductiviteParEmployeJour = Company.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                    Tresorerie = Company.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
                    Categorie = categorie,
                    SecteurActivite = secteur,
                    // Config en millions MGA/jour â†’ Exporter en MGA brut/jour (mis à l'échelle)
                    ValeurFOBJour = fobJourCategorie * 1_000_000 * _facteurEchelle * (0.8 + random.NextDouble() * 0.4),
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
            var secteur = SecteurDepuisCategorieExport(cat);
            double fobBase = _config.FOBJourParCategorie.GetValueOrDefault(cat, 200);
            var exp = new Exporter
            {
                Name = $"Exportateur {exporteurCree + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (1.0 + random.NextDouble() * 0.3),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise * (0.9 + random.NextDouble() * 0.3),
                // Utiliser productivité réaliste du secteur
                ProductiviteParEmployeJour = Company.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = Company.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
                Categorie = cat,
                SecteurActivite = secteur,
                // Config en millions MGA/jour â†’ Exporter en MGA brut/jour (mis à l'échelle)
                ValeurFOBJour = fobBase * 1_000_000 * _facteurEchelle * (0.8 + random.NextDouble() * 0.4),
                PartExport = 0.50 + random.NextDouble() * 0.40
            };
            _exportateurs.Add(exp);
            exporteurCree++;
        }

        // Entreprises normales (marché local)
        int nbAgricoles = (int)(nbEntreprisesNormales * _config.PartEntreprisesAgricoles);
        int nbConstruction = (int)(nbEntreprisesNormales * _config.PartEntreprisesConstruction);
        int nbAutres = nbEntreprisesNormales - nbAgricoles - nbConstruction;

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

            var entreprise = new Company
            {
                Name = i < nomsEntreprises.Length ? nomsEntreprises[i] : $"Entreprise {i + 1}",
                NombreEmployes = employesParEntreprise,
                SalaireMoyenMensuel = _distribution.TirerSalaire(random) * (0.9 + random.NextDouble() * 0.2),
                MargeBeneficiaire = _config.MargeBeneficiaireEntreprise,
                ProductiviteParEmployeJour = Company.GetProductiviteParSecteur(secteur) * (0.8 + random.NextDouble() * 0.4),
                Tresorerie = Company.GetTresorerieInitialeParSecteur(secteur, _config.TresorerieInitialeParSecteur) * (0.5 + random.NextDouble()),
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
        var toutesEntreprises = new List<Company>();
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

        for (int i = 0; i < _config.NombreMenages; i++)
        {
            bool estHomme = random.NextDouble() > 0.5;
            var prenoms = estHomme ? prenomsHommes : prenomsFemmes;

            // Tirer un salaire de la distribution log-normale
            double salaire = _distribution.TirerSalaire(random);
            salairesGeneres[i] = salaire;

            // Déterminer la classe socio-économique
            var classe = _distribution.DeterminerClasse(salaire);
            var comportement = HouseholdSalaryDistribution.ComportementParClasse(classe, random);

            bool estEmploye = random.NextDouble() < comportement.ProbabiliteEmploi;
            int? employeurId = null;
            if (estEmploye)
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

            // Accès eau et électricité JIRAMA (probabilité selon config)
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

            var menage = new Household
            {
                Name = $"{prenoms[random.Next(prenoms.Length)]} {noms[random.Next(noms.Length)]}",
                SalaireMensuel = salaire,
                Classe = classe,
                TauxEpargne = comportement.TauxEpargne,
                PropensionConsommation = comportement.PropensionConsommation,
                Epargne = comportement.EpargneInitiale,
                EstEmploye = estEmploye,
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

        // Mettre à jour les compteurs JIRAMA
        _jirama.NbAbonnesEau = nbAbonnesEau;
        _jirama.NbAbonnesElectricite = nbAbonnesElectricite;

        // Calculer les statistiques initiales de la distribution
        _statsInitiales = HouseholdSalaryDistribution.CalculerStats(salairesGeneres);

        // Initialiser l'État
        _etat = new Government
        {
            TauxIS = _config.TauxIS,
            TauxTVA = _config.TauxTVA,
            TauxDirecteur = _config.TauxDirecteur,
            TauxInflation = _config.TauxInflation,
            DepensesPubliquesJour = _config.DepensesPubliquesJour * _facteurEchelle
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

    /// <summary>CIF total tous importateurs confondus.</summary>
    public double GetCIFTotal() => _importateurs.Sum(i => i.TotalImportationsCIF);

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

        // Reset des compteurs JIRAMA pour ce jour
        _jirama.DebutJournee();

        // Pré-calculer les secteurs des employeurs pour déterminer qui travaille
        var toutesEntreprisesLookup = new Dictionary<int, Company>();
        foreach (var e in _entreprises) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _importateurs) toutesEntreprisesLookup[e.Id] = e;
        foreach (var e in _exportateurs) toutesEntreprisesLookup[e.Id] = e;

        // Calculer la remittance par ménage (répartition uniforme, mise à l'échelle)
        double remittanceParMenage = (_config.RemittancesJour * _facteurEchelle) / Math.Max(_menages.Count, 1);
        double loyerImputeJour = _config.LoyerImputeJourParMenage;

        foreach (var menage in _menages)
        {
            // Déterminer si le ménage travaille aujourd'hui :
            // - Jour ouvrable (lun-ven) â†’ tout le monde travaille
            // - Weekend â†’ seuls les employés du commerce/agriculture travaillent
            bool estJourDeTravail = jourOuvrable;
            if (!jourOuvrable && menage.EmployeurId.HasValue)
            {
                if (toutesEntreprisesLookup.TryGetValue(menage.EmployeurId.Value, out var employeur))
                {
                    estJourDeTravail = employeur.SecteurActivite == ESecteurActivite.Commerces;
                }
            }

            var r = menage.SimulerJournee(_etat, _config.PrixCarburantLitre, _jirama, estJourDeTravail,
                _jourCourant, _config.CoutTransportPaiementJirama);

            // Remittances (transferts diaspora) â€” ajoutées au revenu
            r.Remittance = remittanceParMenage;
            menage.Epargne += remittanceParMenage;

            // Loyer imputé (SCN 2008) â€” propriétaires occupants uniquement
            r.LoyerImpute = menage.EstProprietaire ? loyerImputeJour : 0;

            // Cotisation salariale CNaPS (1%, prélevée sur le salaire)
            r.CotisationCNaPSSalariale = r.RevenuBrut > 0 ? r.RevenuBrut * _config.TauxCotisationsSalarialesCNaPS : 0;

            resultsMenages.Add(r);
            demandeConsommationTotale += r.Consommation;
        }

        // (FinJournee JIRAMA sera appelé après que tous les secteurs aient payé)

        // 2. Les entreprises produisent et vendent (selon leur secteur)
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

        // Importateurs
        if (_config.UseImportCalibresDirectement && _config.CIFCalibresJour.Count > 0)
        {
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // MODE INJECTION DIRECTE DES IMPORTS (INSTAT)
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // On ne simule PAS l'approvisionnement import : on répartit les CIF INSTAT
            // entre les importateurs existants (au prorata de leur catégorie).
            // Les importateurs conservent leur rôle de vendeurs locaux (B2C/B2B).
            //
            // CIFCalibresJour : millions MGA/jour par catégorie (= mensuel INSTAT / 30)
            // On convertit en MGA brut puis on répartit entre les importateurs de la catégorie.
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

            var importateursParCategorie = _importateurs
                .GroupBy(e => e.Categorie)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var importateur in _importateurs)
            {
                // CIF calibré pour la catégorie (millions MGA/jour â†’ MGA brut, mis à l'échelle)
                // Les données INSTAT sont au niveau national (~6M ménages) â†’ proportionnel au nb simulé
                double cifCalibreCategorie = _config.CIFCalibresJour
                    .GetValueOrDefault(importateur.Categorie, 0) * 1_000_000 * _facteurEchelle;

                // Répartir entre les importateurs de la même catégorie
                int nbDansCategorie = importateursParCategorie
                    .GetValueOrDefault(importateur.Categorie)?.Count ?? 1;
                double cifJourImportateur = cifCalibreCategorie / nbDansCategorie;

                // Calculer les droits et taxes comme d'habitude
                double droitsDouaneTaux = _config.TauxDroitsDouane * importateur.CoefficientDDParCategorie();
                double droitsDouane = cifJourImportateur * droitsDouaneTaux;

                double acciseTaux = _config.TauxAccise * importateur.CoefficientAcciseParCategorie();
                double accise = (cifJourImportateur + droitsDouane) * acciseTaux;

                double tvaImport = (cifJourImportateur + droitsDouane + accise) * _etat.TauxTVA;
                double redevance = cifJourImportateur * 0.02;

                double coutTotal = cifJourImportateur + droitsDouane + accise + tvaImport + redevance;
                double reventeJour = cifJourImportateur * (1.0 + importateur.MargeReventeImport);

                // Mettre à jour les cumuls
                importateur.TotalImportationsCIF += cifJourImportateur;
                importateur.TotalDroitsDouane += droitsDouane;
                importateur.TotalAccise += accise;
                importateur.TotalTVAImport += tvaImport;
                importateur.TotalRedevanceStatistique += redevance;

                // Simuler la partie locale (ventes B2C/B2B, salaires, TVA locale)
                bool importeurTravaille = EntrepriseTravailleCeJour(_jourCourant, importateur.SecteurActivite);
                var baseResult = importateur.SimulerJournee(
                    demandeParEntreprise,
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, importeurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS);

                var r = new DailyImporterResult
                {
                    // Ventes locales simulées normalement
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

                    // Import = valeurs calibrées injectées directement
                    ValeurCIF = cifJourImportateur,
                    DroitsDouane = droitsDouane,
                    Accise = accise,
                    TVAImport = tvaImport,
                    RedevanceStatistique = redevance,
                    CoutTotalImport = coutTotal,
                    ReventeImport = reventeJour
                };

                // Ajuster la trésorerie
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

        // Exportateurs
        if (_config.UseExportCalibresDirectement && _config.FOBCalibresJour.Count > 0)
        {
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // MODE INJECTION DIRECTE DES EXPORTS CALIBRÉS (ML/INSTAT)
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
            // On ne simule PAS la production export : on répartit les FOB calibrés
            // entre les exportateurs existants (au prorata de leur catégorie).
            // Les exportateurs conservent leur rôle de vendeurs locaux (B2C/B2B).
            //
            // FOBCalibresJour : millions MGA/jour par catégorie (= mensuel INSTAT / 30)
            // On convertit en MGA brut puis on répartit entre les exportateurs de la catégorie.
            // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

            // Regrouper les exportateurs par catégorie
            var exportateursParCategorie = _exportateurs
                .GroupBy(e => e.Categorie)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var exportateur in _exportateurs)
            {
                // FOB calibré pour la catégorie (millions MGA/jour â†’ MGA brut, mis à l'échelle)
                // Les données INSTAT sont au niveau national (~6M ménages) â†’ proportionnel au nb simulé
                double fobCalibreCategorie = _config.FOBCalibresJour
                    .GetValueOrDefault(exportateur.Categorie, 0) * 1_000_000 * _facteurEchelle;

                // Répartir entre les exportateurs de la même catégorie
                int nbDansCategorie = exportateursParCategorie
                    .GetValueOrDefault(exportateur.Categorie)?.Count ?? 1;
                double fobJourExportateur = fobCalibreCategorie / nbDansCategorie;

                // Calculer les taxes et devises comme d'habitude
                double taxeTaux = _config.TauxTaxeExport;
                double taxeExport = fobJourExportateur * taxeTaux;
                double redevance = fobJourExportateur * 0.015; // ~1.5% moyen
                double devisesNettes = fobJourExportateur - taxeExport - redevance;

                // Mettre à jour les cumuls de l'exportateur
                exportateur.TotalExportationsFOB += fobJourExportateur;
                exportateur.TotalTaxeExport += taxeExport;
                exportateur.TotalRedevanceExport += redevance;
                exportateur.TotalDevisesRapatriees += devisesNettes;

                // Créer le résultat export (partie export = calibrée, partie locale = simulée)
                bool exporteurTravaille = EntrepriseTravailleCeJour(_jourCourant, exportateur.SecteurActivite);
                var baseResult = exportateur.SimulerJournee(
                    demandeParEntreprise * (1.0 - exportateur.PartExport),
                    _etat.TauxIS, _etat.TauxTVA, _etat.TauxInflation, _etat.TauxDirecteur, exporteurTravaille,
                    _jirama, _config.ConsommationElecParEmployeKWhJour, _config.TauxCotisationsPatronalesCNaPS);

                var r = new DailyExporterResult
                {
                    // Ventes locales simulées normalement
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

                    // Export = valeurs calibrées injectées directement
                    ValeurFOB = fobJourExportateur,
                    TaxeExport = taxeExport,
                    RedevanceExport = redevance,
                    DevisesRapatriees = devisesNettes
                };

                // Ajuster la trésorerie avec les devises rapatriées
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

        // 3. L'État consolide (entreprises + importateurs + exportateurs + JIRAMA)
        // Les paramètres macro absolus sont mis à l'échelle par facteurEchelle
        var resultEtat = _etat.SimulerJournee(
            resultsMenages, resultsEntreprises, resultsImportateurs, resultsExportateurs,
            _jirama, _config.ConsommationElecEtatKWhJour * _facteurEchelle,
            _config.AideInternationaleJour * _facteurEchelle,
            _config.SubventionJiramaJour * _facteurEchelle,
            Math.Max(1, (int)(_config.NombreFonctionnaires * _facteurEchelle)),
            _config.SalaireMoyenFonctionnaireMensuel,
            _config.TauxReinvestissementPrive,
            _config.PartInvestissementPublic);

        // 3b. Finaliser les compteurs JIRAMA (production totale + VA)
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
        var toutesEntreprisesRef = new List<Company>();
        toutesEntreprisesRef.AddRange(_entreprises);
        toutesEntreprisesRef.AddRange(_importateurs);
        toutesEntreprisesRef.AddRange(_exportateurs);

        var tousResultsEntreprises = new List<CompanyDailyResult>();
        tousResultsEntreprises.AddRange(resultsEntreprises);
        tousResultsEntreprises.AddRange(resultsImportateurs);
        tousResultsEntreprises.AddRange(resultsExportateurs);

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

            // JIRAMA
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
            // Il faut ajouter : TVA JIRAMA (non incluse dans VA JIRAMA qui est HT) + Recettes douanières
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
                          + _jirama.ChargesSalarialesJour                                           // Salaires JIRAMA
                          + tousResultsEntreprises.Sum(r => r.BeneficeAvantImpot)                   // EBE entreprises
                          + (_jirama.ValeurAjouteeJour - _jirama.ChargesSalarialesJour)             // EBE JIRAMA
                          + tousResultsEntreprises.Sum(r => r.TVACollectee)                         // TVA collectée (impôt sur production)
                          + _jirama.TVACollecteeJour                                               // TVA JIRAMA
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
    private double CalculerCAMoyenParEmploye(List<Company> entreprises)
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






