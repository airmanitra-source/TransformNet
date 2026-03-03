namespace MachineLearning.Web.Models.Simulation.Household;

/// <summary>
/// Modèle de distribution salariale log-normale calibré pour Madagascar.
///
/// Fondements :
/// - La distribution des revenus dans les pays en développement suit une loi log-normale
/// - Madagascar : Gini ~0.43, ~90% secteur informel, forte asymétrie droite
/// - Sources : INSTAT Madagascar, Banque Mondiale, enquêtes ménages EPM
///
/// Quintiles estimés (MGA/mois) :
///   Q1 (0-20%)  : 60 000 – 100 000   (informel, agriculture de subsistance)
///   Q2 (20-40%) : 100 000 – 160 000   (informel urbain, petit commerce)
///   Q3 (40-60%) : 160 000 – 250 000   (formel bas, ouvriers, SMIG)
///   Q4 (60-80%) : 250 000 – 450 000   (formel qualifié, techniciens)
///   Q5 (80-100%): 450 000 – 10 000 000 (cadres, dirigeants, professions libérales)
/// </summary>
public class SalaryDistribution
{
    /// <summary>Salaire médian mensuel en MGA (~170 000 MGA pour Madagascar)</summary>
    public double SalaireMedian { get; set; } = 170_000;

    /// <summary>
    /// Paramètre sigma de la loi log-normale (dispersion/inégalité).
    /// Plus sigma est élevé, plus les inégalités sont fortes.
    /// σ ≈ 0.85 correspond à un Gini ~0.43 pour Madagascar.
    /// Référence : Gini ≈ 2Φ(σ/√2) - 1 pour une loi log-normale.
    /// </summary>
    public double Sigma { get; set; } = 0.85;

    /// <summary>Salaire plancher (SMIG Madagascar ~200 000 MGA, mais secteur informel en-dessous)</summary>
    public double SalairePlancher { get; set; } = 50_000;

    /// <summary>Salaire plafond réaliste (top dirigeants, PDG grandes entreprises)</summary>
    public double SalairePlafond { get; set; } = 10_000_000;

    /// <summary>Part du secteur informel (~85-90% à Madagascar)</summary>
    public double PartSecteurInformel { get; set; } = 0.85;

    /// <summary>
    /// Mu de la loi log-normale, dérivé de la médiane.
    /// Pour une log-normale : médiane = exp(μ), donc μ = ln(médiane)
    /// </summary>
    public double Mu => Math.Log(SalaireMedian);

    /// <summary>
    /// Moyenne théorique de la distribution = exp(μ + σ²/2).
    /// Toujours supérieure à la médiane (asymétrie droite).
    /// </summary>
    public double MoyenneTheorique => Math.Exp(Mu + Sigma * Sigma / 2.0);

    /// <summary>
    /// Coefficient de Gini approximé pour une loi log-normale.
    /// Gini = 2Φ(σ/√2) - 1 où Φ est la CDF de la loi normale standard.
    /// </summary>
    public double GiniTheorique => 2.0 * NormalCDF(Sigma / Math.Sqrt(2.0)) - 1.0;

    /// <summary>
    /// Génère un échantillon de salaires suivant la distribution log-normale.
    /// </summary>
    public double[] GenererEchantillon(int taille, Random random)
    {
        var salaires = new double[taille];
        double mu = Mu;

        for (int i = 0; i < taille; i++)
        {
            // Box-Muller transform pour générer une variable normale standard
            double z = BoxMullerNormal(random);

            // Transformation log-normale : X = exp(μ + σZ)
            double salaire = Math.Exp(mu + Sigma * z);

            // Appliquer plancher et plafond
            salaire = Math.Clamp(salaire, SalairePlancher, SalairePlafond);

            salaires[i] = salaire;
        }

        // Trier pour faciliter l'analyse par quintiles
        Array.Sort(salaires);
        return salaires;
    }

    /// <summary>
    /// Tire un seul salaire de la distribution.
    /// </summary>
    public double TirerSalaire(Random random)
    {
        double z = BoxMullerNormal(random);
        double salaire = Math.Exp(Mu + Sigma * z);
        return Math.Clamp(salaire, SalairePlancher, SalairePlafond);
    }

    /// <summary>
    /// Détermine la classe socio-économique d'un ménage en fonction de son salaire.
    /// </summary>
    public ClasseSocioEconomique DeterminerClasse(double salaireMensuel)
    {
        // Seuils basés sur les quintiles de Madagascar
        double q1Seuil = Math.Exp(Mu + Sigma * -0.842); // 20e percentile (z = -0.842)
        double q2Seuil = Math.Exp(Mu + Sigma * -0.253); // 40e percentile (z = -0.253)
        double q3Seuil = Math.Exp(Mu + Sigma * 0.253);  // 60e percentile
        double q4Seuil = Math.Exp(Mu + Sigma * 0.842);  // 80e percentile

        return salaireMensuel switch
        {
            _ when salaireMensuel <= q1Seuil => ClasseSocioEconomique.Subsistance,
            _ when salaireMensuel <= q2Seuil => ClasseSocioEconomique.InformelBas,
            _ when salaireMensuel <= q3Seuil => ClasseSocioEconomique.FormelBas,
            _ when salaireMensuel <= q4Seuil => ClasseSocioEconomique.FormelQualifie,
            _ => ClasseSocioEconomique.Cadre
        };
    }

    /// <summary>
    /// Calcule les statistiques de distribution pour un ensemble de salaires.
    /// </summary>
    public static DistributionStats CalculerStats(double[] salaires)
    {
        if (salaires.Length == 0)
            return new DistributionStats();

        var sorted = salaires.OrderBy(s => s).ToArray();
        int n = sorted.Length;

        var stats = new DistributionStats
        {
            Moyenne = sorted.Average(),
            Mediane = sorted[n / 2],
            Min = sorted[0],
            Max = sorted[^1],
            EcartType = CalculerEcartType(sorted),
            // Quintiles
            Q1Moyenne = sorted.Take(n / 5).Average(),
            Q2Moyenne = sorted.Skip(n / 5).Take(n / 5).Average(),
            Q3Moyenne = sorted.Skip(2 * n / 5).Take(n / 5).Average(),
            Q4Moyenne = sorted.Skip(3 * n / 5).Take(n / 5).Average(),
            Q5Moyenne = sorted.Skip(4 * n / 5).Average(),
        };

        // Gini empirique
        stats.Gini = CalculerGini(sorted);

        // Ratio interdécile D9/D1
        int d1Index = n / 10;
        int d9Index = 9 * n / 10;
        stats.RatioD9D1 = sorted[d9Index] / Math.Max(sorted[d1Index], 1);

        return stats;
    }

    /// <summary>
    /// Adapte les paramètres comportementaux du ménage selon sa classe socio-économique.
    /// Les ménages pauvres consomment presque tout, les riches épargnent davantage.
    /// </summary>
    public static HouseholdBehavior ComportementParClasse(ClasseSocioEconomique classe, Random random)
    {
        return classe switch
        {
            ClasseSocioEconomique.Subsistance => new HouseholdBehavior
            {
                TauxEpargne = 0.02 + random.NextDouble() * 0.03,         // 2-5%
                PropensionConsommation = 0.92 + random.NextDouble() * 0.06, // 92-98%
                DepensesAlimentairesJour = 2_000 + random.NextDouble() * 1_500,
                DepensesDiversJour = 500 + random.NextDouble() * 800,
                EpargneInitiale = random.NextDouble() * 20_000,
                ProbabiliteEmploi = 0.70 + random.NextDouble() * 0.10,    // 70-80%
                Transport = ModeTransport.TransportPublic,                 // marche ou taxi-be
                DistanceDomicileTravailKm = 3 + random.NextDouble() * 5,
            },
            ClasseSocioEconomique.InformelBas => new HouseholdBehavior
            {
                TauxEpargne = 0.04 + random.NextDouble() * 0.04,         // 4-8%
                PropensionConsommation = 0.85 + random.NextDouble() * 0.08, // 85-93%
                DepensesAlimentairesJour = 3_000 + random.NextDouble() * 2_000,
                DepensesDiversJour = 800 + random.NextDouble() * 1_200,
                EpargneInitiale = random.NextDouble() * 50_000,
                ProbabiliteEmploi = 0.78 + random.NextDouble() * 0.10,
                Transport = ModeTransport.TransportPublic,                 // taxi-be
                DistanceDomicileTravailKm = 5 + random.NextDouble() * 8,
            },
            ClasseSocioEconomique.FormelBas => new HouseholdBehavior
            {
                TauxEpargne = 0.08 + random.NextDouble() * 0.06,         // 8-14%
                PropensionConsommation = 0.75 + random.NextDouble() * 0.10, // 75-85%
                DepensesAlimentairesJour = 4_000 + random.NextDouble() * 2_500,
                DepensesDiversJour = 1_500 + random.NextDouble() * 1_500,
                EpargneInitiale = 20_000 + random.NextDouble() * 100_000,
                ProbabiliteEmploi = 0.88 + random.NextDouble() * 0.08,
                Transport = random.NextDouble() > 0.4 ? ModeTransport.Moto : ModeTransport.TransportPublic,
                DistanceDomicileTravailKm = 5 + random.NextDouble() * 12,
            },
            ClasseSocioEconomique.FormelQualifie => new HouseholdBehavior
            {
                TauxEpargne = 0.12 + random.NextDouble() * 0.08,         // 12-20%
                PropensionConsommation = 0.65 + random.NextDouble() * 0.12, // 65-77%
                DepensesAlimentairesJour = 5_000 + random.NextDouble() * 3_000,
                DepensesDiversJour = 2_500 + random.NextDouble() * 3_000,
                EpargneInitiale = 100_000 + random.NextDouble() * 300_000,
                ProbabiliteEmploi = 0.92 + random.NextDouble() * 0.06,
                Transport = random.NextDouble() > 0.5 ? ModeTransport.Voiture : ModeTransport.Moto,
                DistanceDomicileTravailKm = 8 + random.NextDouble() * 15,
            },
            ClasseSocioEconomique.Cadre => new HouseholdBehavior
            {
                TauxEpargne = 0.18 + random.NextDouble() * 0.12,         // 18-30%
                PropensionConsommation = 0.50 + random.NextDouble() * 0.15, // 50-65%
                DepensesAlimentairesJour = 8_000 + random.NextDouble() * 7_000,
                DepensesDiversJour = 5_000 + random.NextDouble() * 10_000,
                EpargneInitiale = 500_000 + random.NextDouble() * 2_000_000,
                ProbabiliteEmploi = 0.95 + random.NextDouble() * 0.04,
                Transport = ModeTransport.Voiture,                          // voiture
                DistanceDomicileTravailKm = 10 + random.NextDouble() * 20,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(classe))
        };
    }

    // --- Méthodes utilitaires privées ---

    /// <summary>Box-Muller transform pour générer une variable normale standard N(0,1).</summary>
    private static double BoxMullerNormal(Random random)
    {
        double u1 = 1.0 - random.NextDouble(); // (0, 1] pour éviter log(0)
        double u2 = random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    /// <summary>CDF de la loi normale standard (approximation d'Abramowitz & Stegun).</summary>
    private static double NormalCDF(double x)
    {
        double t = 1.0 / (1.0 + 0.2316419 * Math.Abs(x));
        double d = 0.3989422804014327; // 1/sqrt(2π)
        double p = d * Math.Exp(-x * x / 2.0) *
                   (t * (0.3193815 + t * (-0.3565638 + t * (1.781478 + t * (-1.8212560 + t * 1.3302744)))));
        return x >= 0 ? 1.0 - p : p;
    }

    private static double CalculerEcartType(double[] values)
    {
        double mean = values.Average();
        double sumSquares = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumSquares / values.Length);
    }

    /// <summary>Calcule le coefficient de Gini empirique.</summary>
    private static double CalculerGini(double[] sorted)
    {
        int n = sorted.Length;
        double sum = sorted.Sum();
        if (sum == 0) return 0;

        double cumulativeSum = 0;
        double giniNumerator = 0;
        for (int i = 0; i < n; i++)
        {
            cumulativeSum += sorted[i];
            giniNumerator += (2.0 * (i + 1) - n - 1) * sorted[i];
        }
        return giniNumerator / (n * sum);
    }
}

/// <summary>
/// Classes socio-économiques pour Madagascar.
/// </summary>
public enum ClasseSocioEconomique
{
    /// <summary>Agriculture de subsistance, journaliers (~Q1)</summary>
    Subsistance,
    /// <summary>Secteur informel bas, petit commerce (~Q2)</summary>
    InformelBas,
    /// <summary>Secteur formel bas, ouvriers, SMIG (~Q3)</summary>
    FormelBas,
    /// <summary>Secteur formel qualifié, techniciens (~Q4)</summary>
    FormelQualifie,
    /// <summary>Cadres, dirigeants, professions libérales (~Q5)</summary>
    Cadre
}

/// <summary>
/// Comportement économique d'un ménage déterminé par sa classe socio-économique.
/// </summary>
public class HouseholdBehavior
{
    public double TauxEpargne { get; set; }
    public double PropensionConsommation { get; set; }
    public double DepensesAlimentairesJour { get; set; }
    public double DepensesDiversJour { get; set; }
    public double EpargneInitiale { get; set; }
    public double ProbabiliteEmploi { get; set; }
    /// <summary>Mode de transport attribué selon la classe socio-économique</summary>
    public ModeTransport Transport { get; set; } = ModeTransport.TransportPublic;
    /// <summary>Distance domicile-travail en km</summary>
    public double DistanceDomicileTravailKm { get; set; } = 10;
}

/// <summary>
/// Statistiques descriptives d'une distribution de salaires.
/// </summary>
public class DistributionStats
{
    public double Moyenne { get; set; }
    public double Mediane { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double EcartType { get; set; }
    public double Gini { get; set; }
    public double RatioD9D1 { get; set; }

    // Moyennes par quintile
    public double Q1Moyenne { get; set; }
    public double Q2Moyenne { get; set; }
    public double Q3Moyenne { get; set; }
    public double Q4Moyenne { get; set; }
    public double Q5Moyenne { get; set; }
}
