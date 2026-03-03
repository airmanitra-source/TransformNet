using MachineLearning.Web.Models.Simulation.Config;

namespace MachineLearning.Web.Models.Simulation.Companies;

/// <summary>
/// Calibre les paramètres export du simulateur pour coïncider avec les données INSTAT.
///
/// Problème résolu :
///   Étant donné un mois INSTAT (FOB par catégorie en millions MGA),
///   trouver les paramètres du simulateur (TauxExportateurs, FOBJour/catégorie,
///   MargeBeneficiaire, TauxTaxeExport) tels que la simulation produise
///   des totaux FOB mensuels proches des données réelles.
///
/// Pipeline :
///   1. Génération dataset : N simulations de 30 jours avec paramètres aléatoires
///      → extraire le FOB réel par catégorie (via GetFOBParCategorie())
///   2. Entraînement MLP surrogate : paramètres → FOB par catégorie
///   3. Optimisation CMA-ES : minimiser l'erreur relative (MAPE) vs INSTAT
///
/// Unités :
///   - INSTAT : millions MGA / mois
///   - Simulateur : MGA brut / jour → converti en millions MGA / mois (× 30 / 1e6)
/// </summary>
public class ExportCalibrator
{
    // ═══════════════════════════════════════════════
    // PARAMÈTRES À CALIBRER
    // ═══════════════════════════════════════════════

    /// <summary>
    /// 10 paramètres focalisés sur ce qui drive les exports :
    /// - TauxExportateurs : nombre d'exportateurs
    /// - FOBJour × 7 catégories : CA export par catégorie
    /// - MargeBeneficiaire : rentabilité des exportateurs
    /// - TauxTaxeExport : fiscalité export
    /// </summary>
    public static readonly CalibrationParam[] Parametres =
    [
        new("TauxExportateurs",   0.05,    0.50,    0.10),
        new("FOB_BiensAlim",      50,      1_000,   280),      // millions MGA / jour / exportateur
        new("FOB_Vanille",        1,       800,     150),
        new("FOB_Crevettes",      50,      1_000,   320),
        new("FOB_Cafe",           1,       300,     80),
        new("FOB_Girofle",        1,       500,     120),
        new("FOB_Miniers",        100,     2_000,   500),
        new("FOB_ZonesFranches",  50,      1_000,   250),
        new("MargeBeneficiaire",  0.05,    0.50,    0.20),
        new("TauxTaxeExport",     0.00,    0.10,    0.03),
    ];

    private const int INPUT_DIM = 10;
    private const int OUTPUT_DIM = 7;  // 7 catégories FOB
    private const int JOURS_SIM = 30;  // 1 mois simulé

    private readonly Random _rng = new(42);

    // Réseau MLP
    private double[][] _w1 = [];
    private double[] _b1 = [];
    private double[][] _w2 = [];
    private double[] _b2 = [];
    private int _hiddenDim;
    private double[] _lastHidden = [];

    // Normalisation z-score
    private double[] _inputMean = new double[INPUT_DIM];
    private double[] _inputStd = new double[INPUT_DIM];
    private double[] _outputMean = new double[OUTPUT_DIM];
    private double[] _outputStd = new double[OUTPUT_DIM];

    public CalibrationProgress Progress { get; } = new();

    // ═══════════════════════════════════════════════
    // PHASE 1 : GÉNÉRATION DU DATASET
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Génère un dataset en exécutant N simulations de 30 jours.
    /// Chaque simulation utilise des paramètres tirés aléatoirement dans les bornes.
    /// La sortie est le FOB mensuel réel par catégorie (millions MGA).
    /// </summary>
    public CalibrationDataset GenererDataset(int nbSimulations = 300)
    {
        var dataset = new CalibrationDataset();
        Progress.Phase = "Génération du dataset";
        Progress.Total = nbSimulations;

        for (int i = 0; i < nbSimulations; i++)
        {
            Progress.Current = i + 1;

            // Tirer des paramètres aléatoires
            double[] paramValues = new double[INPUT_DIM];
            for (int p = 0; p < INPUT_DIM; p++)
                paramValues[p] = Parametres[p].Min + _rng.NextDouble() * (Parametres[p].Max - Parametres[p].Min);

            // Créer la config et exécuter la simulation
            var config = CreerConfig(paramValues);
            var sim = new EconomicSimulator();
            sim.Initialiser(config);
            sim.SimulerNJoursSync(JOURS_SIM);

            // Extraire le FOB réel par catégorie depuis le simulateur
            var fobParCat = sim.GetFOBParCategorie();

            // Convertir en millions MGA / mois (FOB cumulé sur 30 jours / 1e6)
            double[] output = CategoriesVersVecteur(fobParCat);

            dataset.Inputs.Add(paramValues);
            dataset.Outputs.Add(output);
        }

        Progress.Phase = "Dataset généré";
        return dataset;
    }

    // ═══════════════════════════════════════════════
    // PHASE 2 : ENTRAÎNEMENT DU SURROGATE MLP
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Entraîne un MLP :
    ///   Input  [10] : paramètres export
    ///   Output [7]  : FOB mensuel par catégorie (millions MGA)
    /// </summary>
    public SurrogateTrainingResult EntrainerSurrogate(
        CalibrationDataset dataset,
        int hiddenDim = 64,
        int epochs = 300,
        double lr = 0.001)
    {
        _hiddenDim = hiddenDim;
        Progress.Phase = "Entraînement du surrogate";
        Progress.Total = epochs;

        NormaliserDataset(dataset);
        InitialiserReseau();

        int n = dataset.Inputs.Count;
        int nTrain = (int)(n * 0.8);
        var indices = Enumerable.Range(0, n).OrderBy(_ => _rng.Next()).ToList();
        var trainIdx = indices.Take(nTrain).ToList();
        var testIdx = indices.Skip(nTrain).ToList();

        var result = new SurrogateTrainingResult { NbEchantillons = n };
        double bestTestLoss = double.MaxValue;

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            Progress.Current = epoch + 1;
            double trainLoss = 0;
            trainIdx = trainIdx.OrderBy(_ => _rng.Next()).ToList();

            foreach (int idx in trainIdx)
            {
                double[] input = NormaliserInput(dataset.Inputs[idx]);
                double[] target = NormaliserOutput(dataset.Outputs[idx]);
                double[] pred = Forward(input);

                double loss = 0;
                double[] grad = new double[OUTPUT_DIM];
                for (int o = 0; o < OUTPUT_DIM; o++)
                {
                    double diff = pred[o] - target[o];
                    loss += diff * diff;
                    grad[o] = 2.0 * diff / OUTPUT_DIM;
                }
                trainLoss += loss / OUTPUT_DIM;
                Backward(input, grad, lr);
            }
            trainLoss /= Math.Max(trainIdx.Count, 1);

            double testLoss = 0;
            foreach (int idx in testIdx)
            {
                double[] input = NormaliserInput(dataset.Inputs[idx]);
                double[] target = NormaliserOutput(dataset.Outputs[idx]);
                double[] pred = Forward(input);
                for (int o = 0; o < OUTPUT_DIM; o++)
                    testLoss += (pred[o] - target[o]) * (pred[o] - target[o]) / OUTPUT_DIM;
            }
            testLoss /= Math.Max(testIdx.Count, 1);
            if (testLoss < bestTestLoss) bestTestLoss = testLoss;

            if (epoch % 10 == 0 || epoch == epochs - 1)
            {
                result.HistoriqueLoss.Add(new LossEntry
                {
                    Epoch = epoch,
                    TrainLoss = trainLoss,
                    TestLoss = testLoss
                });
            }
        }

        result.BestTestLoss = bestTestLoss;
        Progress.Phase = "Surrogate entraîné";
        return result;
    }

    // ═══════════════════════════════════════════════
    // PHASE 3 : OPTIMISATION CMA-ES (erreur relative)
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Calibre les paramètres en minimisant le MAPE (Mean Absolute Percentage Error)
    /// entre les prédictions du surrogate et les données INSTAT.
    /// </summary>
    public ExportCalibrationResult Calibrer(
        ExportHistoricalData cible,
        int iterations = 500,
        int populationSize = 100)
    {
        Progress.Phase = "Optimisation CMA-ES";
        Progress.Total = iterations;

        // Cible : FOB par catégorie en millions MGA/mois (déjà en millions dans INSTAT)
        double[] targetFOB =
        [
            cible.BiensAlimentaires,
            cible.Vanille,
            cible.Crevettes,
            cible.Cafe,
            cible.Girofle,
            cible.ProduitsMiniers,
            cible.ZonesFranches
        ];

        // μ et σ de la distribution CMA-ES (en espace [0,1])
        double[] mean = new double[INPUT_DIM];
        double[] sigma = new double[INPUT_DIM];
        for (int p = 0; p < INPUT_DIM; p++)
        {
            mean[p] = (Parametres[p].Default - Parametres[p].Min)
                    / (Parametres[p].Max - Parametres[p].Min);
            sigma[p] = 0.25;
        }

        double bestFitness = double.MaxValue;
        double[] bestParamsNorm = new double[INPUT_DIM];

        for (int iter = 0; iter < iterations; iter++)
        {
            Progress.Current = iter + 1;

            var population = new (double[] norm, double fitness)[populationSize];

            for (int i = 0; i < populationSize; i++)
            {
                double[] normParams = new double[INPUT_DIM];
                for (int p = 0; p < INPUT_DIM; p++)
                {
                    normParams[p] = mean[p] + sigma[p] * GaussianRandom();
                    normParams[p] = Math.Clamp(normParams[p], 0.0, 1.0);
                }

                // Convertir en valeurs réelles pour le surrogate
                double[] realParams = NormVersReel(normParams);
                double[] surrogateInput = NormaliserInput(realParams);
                double[] predNorm = Forward(surrogateInput);
                double[] predFOB = DenormaliserOutput(predNorm);

                // Fitness = MAPE (Mean Absolute Percentage Error)
                // Pondéré par l'importance de chaque catégorie dans le total INSTAT
                double fitness = 0;
                double totalCible = targetFOB.Sum();
                for (int o = 0; o < OUTPUT_DIM; o++)
                {
                    double poidsCategorie = totalCible > 0 ? targetFOB[o] / totalCible : 1.0 / OUTPUT_DIM;
                    double denominateur = Math.Max(Math.Abs(targetFOB[o]), 1.0); // éviter /0
                    double erreurRelative = Math.Abs(predFOB[o] - targetFOB[o]) / denominateur;
                    fitness += poidsCategorie * erreurRelative;
                }

                population[i] = (normParams, fitness);

                if (fitness < bestFitness)
                {
                    bestFitness = fitness;
                    Array.Copy(normParams, bestParamsNorm, INPUT_DIM);
                }
            }

            // Sélection élitiste : top 20%
            var elite = population.OrderBy(p => p.fitness).Take(populationSize / 5).ToArray();

            for (int p = 0; p < INPUT_DIM; p++)
            {
                double newMean = elite.Average(e => e.norm[p]);
                double variance = elite.Average(e => (e.norm[p] - newMean) * (e.norm[p] - newMean));
                mean[p] = newMean;
                sigma[p] = Math.Max(Math.Sqrt(variance), 0.003);
            }
        }

        // ── Construire le résultat ──
        double[] optimalReal = NormVersReel(bestParamsNorm);
        double[] optPred = DenormaliserOutput(Forward(NormaliserInput(optimalReal)));

        var result = new ExportCalibrationResult
        {
            Fitness = bestFitness,
            MAPE = bestFitness * 100, // en %
            Cible = cible,
            PredictionFOB = optPred,
        };

        for (int p = 0; p < INPUT_DIM; p++)
            result.ParametresOptimaux[Parametres[p].Nom] = optimalReal[p];

        result.ConfigCalibree = CreerConfigFromResult(result.ParametresOptimaux);

        // Calculer les écarts par catégorie
        for (int o = 0; o < OUTPUT_DIM; o++)
        {
            double cibleVal = targetFOB[o];
            double predVal = optPred[o];
            double ecart = cibleVal > 0 ? (predVal - cibleVal) / cibleVal * 100 : 0;
            result.EcartsParCategorie.Add(ecart);
        }

        Progress.Phase = "Calibration terminée";
        return result;
    }

    // ═══════════════════════════════════════════════
    // VALIDATION : relancer la simulation avec les params calibrés
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Vérifie la calibration en relançant une vraie simulation avec les paramètres optimaux.
    /// Compare le résultat réel (pas le surrogate) avec la cible INSTAT.
    /// </summary>
    public double[] ValiderParSimulation(ExportCalibrationResult calibResult)
    {
        var config = calibResult.ConfigCalibree;
        config.DureeJours = JOURS_SIM;
        config.NombreMenages = 200;

        var sim = new EconomicSimulator();
        sim.Initialiser(config);
        sim.SimulerNJoursSync(JOURS_SIM);

        var fobParCat = sim.GetFOBParCategorie();
        return CategoriesVersVecteur(fobParCat);
    }

    // ═══════════════════════════════════════════════
    // RÉSEAU MLP (2 couches, ReLU)
    // ═══════════════════════════════════════════════

    private void InitialiserReseau()
    {
        double scale1 = Math.Sqrt(2.0 / INPUT_DIM);
        _w1 = new double[_hiddenDim][];
        _b1 = new double[_hiddenDim];
        for (int h = 0; h < _hiddenDim; h++)
        {
            _w1[h] = new double[INPUT_DIM];
            for (int i = 0; i < INPUT_DIM; i++)
                _w1[h][i] = (_rng.NextDouble() - 0.5) * scale1;
        }
        double scale2 = Math.Sqrt(2.0 / _hiddenDim);
        _w2 = new double[OUTPUT_DIM][];
        _b2 = new double[OUTPUT_DIM];
        for (int o = 0; o < OUTPUT_DIM; o++)
        {
            _w2[o] = new double[_hiddenDim];
            for (int h = 0; h < _hiddenDim; h++)
                _w2[o][h] = (_rng.NextDouble() - 0.5) * scale2;
        }
    }

    private double[] Forward(double[] input)
    {
        _lastHidden = new double[_hiddenDim];
        for (int h = 0; h < _hiddenDim; h++)
        {
            double sum = _b1[h];
            for (int i = 0; i < INPUT_DIM; i++)
                sum += _w1[h][i] * input[i];
            _lastHidden[h] = Math.Max(0, sum);
        }
        double[] output = new double[OUTPUT_DIM];
        for (int o = 0; o < OUTPUT_DIM; o++)
        {
            output[o] = _b2[o];
            for (int h = 0; h < _hiddenDim; h++)
                output[o] += _w2[o][h] * _lastHidden[h];
        }
        return output;
    }

    private void Backward(double[] input, double[] gradOutput, double lr)
    {
        double[] gradHidden = new double[_hiddenDim];
        for (int o = 0; o < OUTPUT_DIM; o++)
        {
            for (int h = 0; h < _hiddenDim; h++)
            {
                gradHidden[h] += gradOutput[o] * _w2[o][h];
                _w2[o][h] -= lr * gradOutput[o] * _lastHidden[h];
            }
            _b2[o] -= lr * gradOutput[o];
        }
        for (int h = 0; h < _hiddenDim; h++)
        {
            if (_lastHidden[h] <= 0) continue;
            for (int i = 0; i < INPUT_DIM; i++)
                _w1[h][i] -= lr * gradHidden[h] * input[i];
            _b1[h] -= lr * gradHidden[h];
        }
    }

    // ═══════════════════════════════════════════════
    // NORMALISATION
    // ═══════════════════════════════════════════════

    private void NormaliserDataset(CalibrationDataset ds)
    {
        for (int p = 0; p < INPUT_DIM; p++)
        {
            _inputMean[p] = ds.Inputs.Average(x => x[p]);
            double v = ds.Inputs.Average(x => (x[p] - _inputMean[p]) * (x[p] - _inputMean[p]));
            _inputStd[p] = Math.Max(Math.Sqrt(v), 1e-8);
        }
        for (int o = 0; o < OUTPUT_DIM; o++)
        {
            _outputMean[o] = ds.Outputs.Average(x => x[o]);
            double v = ds.Outputs.Average(x => (x[o] - _outputMean[o]) * (x[o] - _outputMean[o]));
            _outputStd[o] = Math.Max(Math.Sqrt(v), 1e-8);
        }
    }

    private double[] NormaliserInput(double[] raw)
    {
        double[] r = new double[INPUT_DIM];
        for (int i = 0; i < INPUT_DIM; i++)
            r[i] = (raw[i] - _inputMean[i]) / _inputStd[i];
        return r;
    }

    private double[] NormaliserOutput(double[] raw)
    {
        double[] r = new double[OUTPUT_DIM];
        for (int o = 0; o < Math.Min(raw.Length, OUTPUT_DIM); o++)
            r[o] = (raw[o] - _outputMean[o]) / _outputStd[o];
        return r;
    }

    private double[] DenormaliserOutput(double[] norm)
    {
        double[] r = new double[OUTPUT_DIM];
        for (int o = 0; o < OUTPUT_DIM; o++)
            r[o] = norm[o] * _outputStd[o] + _outputMean[o];
        return r;
    }

    // ═══════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════

    /// <summary>Convertit les FOB par catégorie du simulateur en millions MGA mensuels.</summary>
    private static double[] CategoriesVersVecteur(Dictionary<CategorieExport, double> fob)
    {
        // Le sim cumule sur 30 jours en MGA brut → convertir en millions MGA
        return
        [
            fob.GetValueOrDefault(CategorieExport.BiensAlimentaires, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.Vanille, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.Crevettes, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.Cafe, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.Girofle, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.ProduitsMiniers, 0) / 1e6,
            fob.GetValueOrDefault(CategorieExport.ZonesFranches, 0) / 1e6,
        ];
    }

    private double[] NormVersReel(double[] norm)
    {
        double[] real = new double[INPUT_DIM];
        for (int p = 0; p < INPUT_DIM; p++)
            real[p] = Parametres[p].Min + norm[p] * (Parametres[p].Max - Parametres[p].Min);
        return real;
    }

    private ScenarioConfig CreerConfig(double[] p)
    {
        return new ScenarioConfig
        {
            NombreMenages = 200,
            NombreEntreprises = 1_000,
            DureeJours = JOURS_SIM,
            TauxExportateurs = p[0],
            MargeBeneficiaireEntreprise = p[8],
            TauxTaxeExport = p[9],
            FOBJourParCategorie = new()
            {
                [CategorieExport.BiensAlimentaires] = p[1],
                [CategorieExport.Vanille] = p[2],
                [CategorieExport.Crevettes] = p[3],
                [CategorieExport.Cafe] = p[4],
                [CategorieExport.Girofle] = p[5],
                [CategorieExport.ProduitsMiniers] = p[6],
                [CategorieExport.ZonesFranches] = p[7],
            },
        };
    }

    private static ScenarioConfig CreerConfigFromResult(Dictionary<string, double> pars)
    {
        return new ScenarioConfig
        {
            Name = "Calibré INSTAT (ML)",
            Description = "Paramètres export calibrés par MLP surrogate + CMA-ES sur données réelles INSTAT",
            TauxExportateurs = pars.GetValueOrDefault("TauxExportateurs", 0.10),
            MargeBeneficiaireEntreprise = pars.GetValueOrDefault("MargeBeneficiaire", 0.20),
            TauxTaxeExport = pars.GetValueOrDefault("TauxTaxeExport", 0.03),
            FOBJourParCategorie = new()
            {
                [CategorieExport.BiensAlimentaires] = pars.GetValueOrDefault("FOB_BiensAlim", 280),
                [CategorieExport.Vanille] = pars.GetValueOrDefault("FOB_Vanille", 150),
                [CategorieExport.Crevettes] = pars.GetValueOrDefault("FOB_Crevettes", 320),
                [CategorieExport.Cafe] = pars.GetValueOrDefault("FOB_Cafe", 80),
                [CategorieExport.Girofle] = pars.GetValueOrDefault("FOB_Girofle", 120),
                [CategorieExport.ProduitsMiniers] = pars.GetValueOrDefault("FOB_Miniers", 500),
                [CategorieExport.ZonesFranches] = pars.GetValueOrDefault("FOB_ZonesFranches", 250),
            },
        };
    }

    private double GaussianRandom()
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = _rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}

// ═══════════════════════════════════════════════
// CLASSES DE SUPPORT
// ═══════════════════════════════════════════════

public record CalibrationParam(string Nom, double Min, double Max, double Default);

public class CalibrationDataset
{
    public List<double[]> Inputs { get; set; } = [];
    public List<double[]> Outputs { get; set; } = [];
}

public class SurrogateTrainingResult
{
    public double BestTestLoss { get; set; }
    public int NbEchantillons { get; set; }
    public List<LossEntry> HistoriqueLoss { get; set; } = [];
}

public class LossEntry
{
    public int Epoch { get; set; }
    public double TrainLoss { get; set; }
    public double TestLoss { get; set; }
}

public class ExportCalibrationResult
{
    public double Fitness { get; set; }
    public double MAPE { get; set; }
    public ExportHistoricalData Cible { get; set; } = new();
    public Dictionary<string, double> ParametresOptimaux { get; set; } = [];
    public double[] PredictionFOB { get; set; } = new double[7];
    public double[] ValidationFOB { get; set; } = new double[7];
    public List<double> EcartsParCategorie { get; set; } = [];
    public ScenarioConfig ConfigCalibree { get; set; } = new();

    public string[] NomsCategories =>
        ["Biens alim.", "Vanille", "Crevettes", "Café", "Girofle", "Prod. miniers", "Zones franches"];
}

public class CalibrationProgress
{
    public string Phase { get; set; } = "En attente";
    public int Current { get; set; }
    public int Total { get; set; }
    public double Pourcentage => Total > 0 ? (double)Current / Total * 100 : 0;
}
