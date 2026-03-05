using Company.Module.Models;
using Household.Module.Models;
using Household.Salary.Distribution.Module;

namespace Household.Module
{
    /// <summary>
    /// Implémentation du module Ménages.
    /// Centralise le comportement économique par classe socio-économique
    /// et la logique d'achat alimentaire.
    /// La distribution salariale (tirage, classification, stats) est déléguée
    /// à <c>HouseholdSalaryDistributionModule</c>.
    /// </summary>
    public class HouseholdModule : IHouseholdModule, IHouseholdSalaryDistributionModule
    {
        private double _salaireMedian = 170_000;
        private double _sigma = 0.85;
        private double _salairePlancher = 50_000;
        private double _salairePlafond = 10_000_000;
        private double _partSecteurInformel = 0.85;

        private double Mu => Math.Log(_salaireMedian);

        public void ConfigurerDistributionSalariale(
            double salaireMedian,
            double sigma,
            double salairePlancher,
            double partSecteurInformel)
        {
            _salaireMedian = salaireMedian;
            _sigma = sigma;
            _salairePlancher = salairePlancher;
            _partSecteurInformel = partSecteurInformel;
        }

        public double TirerSalaire(Random random)
        {
            double z = BoxMullerNormal(random);
            double salaire = Math.Exp(Mu + _sigma * z);
            return Math.Clamp(salaire, _salairePlancher, _salairePlafond);
        }

        public ClasseSocioEconomique DeterminerClasse(double salaireMensuel)
        {
            double q1Seuil = Math.Exp(Mu + _sigma * -0.842);
            double q2Seuil = Math.Exp(Mu + _sigma * -0.253);
            double q3Seuil = Math.Exp(Mu + _sigma * 0.253);
            double q4Seuil = Math.Exp(Mu + _sigma * 0.842);

            return salaireMensuel switch
            {
                _ when salaireMensuel <= q1Seuil => ClasseSocioEconomique.Subsistance,
                _ when salaireMensuel <= q2Seuil => ClasseSocioEconomique.InformelBas,
                _ when salaireMensuel <= q3Seuil => ClasseSocioEconomique.FormelBas,
                _ when salaireMensuel <= q4Seuil => ClasseSocioEconomique.FormelQualifie,
                _ => ClasseSocioEconomique.Cadre
            };
        }

        public HouseholdBehavior GetComportementParClasse(ClasseSocioEconomique classe, Random random)
        {
            return classe switch
            {
                ClasseSocioEconomique.Subsistance => new HouseholdBehavior
                {
                    TauxEpargne = 0.02 + random.NextDouble() * 0.03,
                    PropensionConsommation = 0.92 + random.NextDouble() * 0.06,
                    DepensesAlimentairesJour = 2_000 + random.NextDouble() * 1_500,
                    DepensesDiversJour = 500 + random.NextDouble() * 800,
                    EpargneInitiale = random.NextDouble() * 20_000,
                    ProbabiliteEmploi = 0.70 + random.NextDouble() * 0.10,
                    Transport = ModeTransport.TransportPublic,
                    DistanceDomicileTravailKm = 3 + random.NextDouble() * 5,
                    BudgetSortieWeekend = 0,
                    BudgetVacances = 0,
                    ProbabiliteSortieWeekend = 0,
                    FrequenceVacancesJours = 0,
                    ProbabiliteVacances = 0,
                    DureeVacancesJours = 0,
                },
                ClasseSocioEconomique.InformelBas => new HouseholdBehavior
                {
                    TauxEpargne = 0.04 + random.NextDouble() * 0.04,
                    PropensionConsommation = 0.85 + random.NextDouble() * 0.08,
                    DepensesAlimentairesJour = 3_000 + random.NextDouble() * 2_000,
                    DepensesDiversJour = 800 + random.NextDouble() * 1_200,
                    EpargneInitiale = random.NextDouble() * 50_000,
                    ProbabiliteEmploi = 0.78 + random.NextDouble() * 0.10,
                    Transport = ModeTransport.TransportPublic,
                    DistanceDomicileTravailKm = 5 + random.NextDouble() * 8,
                    BudgetSortieWeekend = 0,
                    BudgetVacances = 0,
                    ProbabiliteSortieWeekend = 0,
                    FrequenceVacancesJours = 0,
                    ProbabiliteVacances = 0,
                    DureeVacancesJours = 0,
                },
                ClasseSocioEconomique.FormelBas => new HouseholdBehavior
                {
                    TauxEpargne = 0.08 + random.NextDouble() * 0.06,
                    PropensionConsommation = 0.75 + random.NextDouble() * 0.10,
                    DepensesAlimentairesJour = 4_000 + random.NextDouble() * 2_500,
                    DepensesDiversJour = 1_500 + random.NextDouble() * 1_500,
                    EpargneInitiale = 20_000 + random.NextDouble() * 100_000,
                    ProbabiliteEmploi = 0.88 + random.NextDouble() * 0.08,
                    Transport = random.NextDouble() > 0.4 ? ModeTransport.Moto : ModeTransport.TransportPublic,
                    DistanceDomicileTravailKm = 5 + random.NextDouble() * 12,
                    BudgetSortieWeekend = 3_000 + random.NextDouble() * 2_000,
                    BudgetVacances = 0,
                    ProbabiliteSortieWeekend = 0.15 + random.NextDouble() * 0.10,
                    FrequenceVacancesJours = 0,
                    ProbabiliteVacances = 0,
                    DureeVacancesJours = 0,
                },
                ClasseSocioEconomique.FormelQualifie => new HouseholdBehavior
                {
                    TauxEpargne = 0.12 + random.NextDouble() * 0.08,
                    PropensionConsommation = 0.65 + random.NextDouble() * 0.12,
                    DepensesAlimentairesJour = 5_000 + random.NextDouble() * 3_000,
                    DepensesDiversJour = 2_500 + random.NextDouble() * 3_000,
                    EpargneInitiale = 100_000 + random.NextDouble() * 300_000,
                    ProbabiliteEmploi = 0.92 + random.NextDouble() * 0.06,
                    Transport = random.NextDouble() > 0.5 ? ModeTransport.Voiture : ModeTransport.Moto,
                    DistanceDomicileTravailKm = 8 + random.NextDouble() * 15,
                    BudgetSortieWeekend = 10_000 + random.NextDouble() * 10_000,
                    BudgetVacances = 15_000 + random.NextDouble() * 10_000,
                    ProbabiliteSortieWeekend = 0.40 + random.NextDouble() * 0.20,
                    FrequenceVacancesJours = 180,
                    ProbabiliteVacances = 0.40 + random.NextDouble() * 0.20,
                    DureeVacancesJours = 3 + random.Next(3),
                },
                ClasseSocioEconomique.Cadre => new HouseholdBehavior
                {
                    TauxEpargne = 0.18 + random.NextDouble() * 0.12,
                    PropensionConsommation = 0.50 + random.NextDouble() * 0.15,
                    DepensesAlimentairesJour = 8_000 + random.NextDouble() * 7_000,
                    DepensesDiversJour = 5_000 + random.NextDouble() * 10_000,
                    EpargneInitiale = 500_000 + random.NextDouble() * 2_000_000,
                    ProbabiliteEmploi = 0.95 + random.NextDouble() * 0.04,
                    Transport = ModeTransport.Voiture,
                    DistanceDomicileTravailKm = 10 + random.NextDouble() * 20,
                    BudgetSortieWeekend = 25_000 + random.NextDouble() * 25_000,
                    BudgetVacances = 40_000 + random.NextDouble() * 30_000,
                    ProbabiliteSortieWeekend = 0.60 + random.NextDouble() * 0.25,
                    FrequenceVacancesJours = 90,
                    ProbabiliteVacances = 0.65 + random.NextDouble() * 0.20,
                    DureeVacancesJours = 4 + random.Next(4),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(classe))
            };
        }

        public DistributionStats CalculerStats(double[] valeurs)
        {
            if (valeurs.Length == 0)
                return new DistributionStats();

            var sorted = valeurs.OrderBy(s => s).ToArray();
            int n = sorted.Length;

            var stats = new DistributionStats
            {
                Moyenne = sorted.Average(),
                Mediane = sorted[n / 2],
                Min = sorted[0],
                Max = sorted[^1],
                EcartType = CalculerEcartType(sorted),
                Q1Moyenne = sorted.Take(n / 5).Average(),
                Q2Moyenne = sorted.Skip(n / 5).Take(n / 5).Average(),
                Q3Moyenne = sorted.Skip(2 * n / 5).Take(n / 5).Average(),
                Q4Moyenne = sorted.Skip(3 * n / 5).Take(n / 5).Average(),
                Q5Moyenne = sorted.Skip(4 * n / 5).Average(),
            };

            stats.Gini = CalculerGini(sorted);

            int d1Index = n / 10;
            int d9Index = 9 * n / 10;
            stats.RatioD9D1 = sorted[d9Index] / Math.Max(sorted[d1Index], 1);

            return stats;
        }

        private static double BoxMullerNormal(Random random)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        private static double CalculerEcartType(double[] values)
        {
            double mean = values.Average();
            double sumSquares = values.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumSquares / values.Length);
        }

        private static double CalculerGini(double[] sorted)
        {
            int n = sorted.Length;
            double sum = sorted.Sum();
            if (sum == 0) return 0;

            double giniNumerator = 0;
            for (int i = 0; i < n; i++)
            {
                giniNumerator += (2.0 * (i + 1) - n - 1) * sorted[i];
            }
            return giniNumerator / (n * sum);
        }

        /// <remarks>
        /// NOTE DE CALIBRAGE : les valeurs de <c>DepensesAlimentairesJour</c> ici (15 000 MGA pour
        /// Subsistance) sont sensiblement différentes de celles de
        /// <c>HouseholdSalaryDistribution.ComportementParClasse</c> (2 000-3 500 MGA pour Subsistance),
        /// qui est la source effectivement utilisée à l'initialisation dans
        /// <c>EconomicSimulatorViewModel.Initialiser()</c>. À réconcilier.
        /// </remarks>
        public (double TauxEpargne, double PropensionConsommation, double DepensesAlimentairesJour,
                 double DepensesDiversJour, string Transport, double DistanceDomicileTravailKm,
                 double EpargneInitiale) GetComportementParClasse(ClasseSocioEconomique classe)
        {
            return classe switch
            {
                ClasseSocioEconomique.Subsistance => (
                    TauxEpargne: 0.02,
                    PropensionConsommation: 0.98,
                    DepensesAlimentairesJour: 15_000,
                    DepensesDiversJour: 2_000,
                    Transport: "à pied",
                    DistanceDomicileTravailKm: 2,
                    EpargneInitiale: 5_000
                ),
                ClasseSocioEconomique.InformelBas => (
                    TauxEpargne: 0.05,
                    PropensionConsommation: 0.90,
                    DepensesAlimentairesJour: 20_000,
                    DepensesDiversJour: 5_000,
                    Transport: "moto",
                    DistanceDomicileTravailKm: 5,
                    EpargneInitiale: 25_000
                ),
                ClasseSocioEconomique.FormelBas => (
                    TauxEpargne: 0.15,
                    PropensionConsommation: 0.80,
                    DepensesAlimentairesJour: 25_000,
                    DepensesDiversJour: 10_000,
                    Transport: "moto",
                    DistanceDomicileTravailKm: 8,
                    EpargneInitiale: 100_000
                ),
                ClasseSocioEconomique.FormelQualifie => (
                    TauxEpargne: 0.25,
                    PropensionConsommation: 0.70,
                    DepensesAlimentairesJour: 35_000,
                    DepensesDiversJour: 20_000,
                    Transport: "voiture",
                    DistanceDomicileTravailKm: 12,
                    EpargneInitiale: 500_000
                ),
                ClasseSocioEconomique.Cadre => (
                    TauxEpargne: 0.40,
                    PropensionConsommation: 0.55,
                    DepensesAlimentairesJour: 50_000,
                    DepensesDiversJour: 40_000,
                    Transport: "voiture",
                    DistanceDomicileTravailKm: 15,
                    EpargneInitiale: 2_000_000
                ),
                _ => (0.10, 0.80, 20_000, 5_000, "moto", 5, 50_000)
            };
        }

        /// <summary>
        /// Simule l'achat de produits alimentaires avec comportement réaliste.
        /// - 85 % auprès du secteur informel (prix bas, volatilité haute)
        /// - 15 % auprès du secteur formel (prix stable, TVA incluse)
        /// - Réduction progressive des quantités en cas d'augmentations répétitives
        /// </summary>
        public (double CoutTotal, double CoutInformel, double CoutFormel, double QuantiteReduite)
            AcheteProduitsAlimentaires(
                double depenseAlimentairesJourBase,
                double cumulHaussePrixAlimentaire,
                double elasticiteUtilisateur,
                double revenuDisponible)
        {
            double facteurReduction = CalculerFacteurReductionQuantites(cumulHaussePrixAlimentaire, elasticiteUtilisateur);

            double depenseEffective = depenseAlimentairesJourBase * facteurReduction;

            double coutInformel = depenseEffective * 0.85;
            double coutFormel   = depenseEffective * 0.15 * 1.20; // TVA 20 %

            double coutTotal = Math.Min(coutInformel + coutFormel, revenuDisponible * 0.80);

            return (coutTotal, coutInformel, coutFormel, facteurReduction);
        }

        /// <summary>
        /// Calcule la réduction de quantités achetées suite à des augmentations répétitives de prix.
        /// Courbe logistique : adaptation rapide → plateau (privation).
        /// </summary>
        private static double CalculerFacteurReductionQuantites(
            double cumulHaussePrix,
            double elasticiteUtilisateur)
        {
            if (cumulHaussePrix <= 0) return 1.0;

            double k  = 0.15 / Math.Max(elasticiteUtilisateur, 0.1);
            double x0 = 25.0;

            // Logistique inversée : plus l'inflation, moins on achète
            double facteur = 1.0 / (1.0 + Math.Exp(k * (cumulHaussePrix - x0)));

            return Math.Max(facteur, 0.30); // floor biologique
        }

        public DailyHouseholdResult SimulerJournee(
            Models.Household menage,
            double impotIRSAJournalier,
            double tauxEffectifIRSA,
            double tauxInflation,
            double tauxTVA,
            double prixCarburant,
            Jirama? Jirama,
            bool estJourDeTravail = true,
            int jourCourant = 1,
            double coutTransportJirama = 0,
            double prixCarburantReference = 5_500,
            double elasticitePrixParCarburant = 0.70,
            double volatiliteAleatoireMarche = 0.10,
            double elasticiteComportementMenage = 0.65,
            double partRevenuAlimentaireNormale = 0.40,
            Random? random = null)
        {
            var result = new DailyHouseholdResult();

            // 1. Revenu du jour (uniquement si employé ET jour de travail)
            // On ajuste le salaire journalier : salaire mensuel / 22 jours ouvrables (au lieu de /30)
            // pour que le total mensuel soit correct sur 22 jours ouvrables
            bool travailleAujourdHui = menage.EstEmploye && estJourDeTravail;
            double salaireJourOuvrable = menage.SalaireMensuel / 22.0; // 22 jours ouvrables/mois
            double revenuBrut = travailleAujourdHui ? salaireJourOuvrable : 0;
            result.RevenuBrut = revenuBrut;

            // 2. IRSA progressif (uniquement les jours travaillés)
            double impotIRSA = travailleAujourdHui ? impotIRSAJournalier : 0;
            result.ImpotIR = impotIRSA;
            result.TauxEffectifIR = menage.SalaireMensuel > 0 ? tauxEffectifIRSA : 0;
            menage.TotalImpotsPaye += impotIRSA;

            double revenuNet = revenuBrut - impotIRSA;

            // 3. Transport (seulement si jour de travail, il faut aller travailler)
            double depensesTransport = travailleAujourdHui ? menage.CalculerDepensesTransportJour(prixCarburant) : 0;
            double facteurInflationJour = 1.0 + (tauxInflation / 365.0);
            depensesTransport *= facteurInflationJour;
            result.DepensesTransport = depensesTransport;
            menage.TotalTransport += depensesTransport;

            // ── Facteur de choc carburant → prix locaux ─────────────────────────────
            // Δcarburant relatif par rapport au prix de référence (0 si pas de choc)
            double chocCarburantRel = prixCarburantReference > 0
                ? (prixCarburant - prixCarburantReference) / prixCarburantReference
                : 0.0;

            // Aléa de marché ~ N(0, σ) — bruit journalier autour de la transmission
            double alea = 0.0;
            if (random != null && volatiliteAleatoireMarche > 0)
            {
                // Box-Muller : deux U(0,1) → N(0,1)
                double u1 = Math.Max(1e-10, random.NextDouble());
                double u2 = random.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                alea = z * volatiliteAleatoireMarche;
            }

            // facteurChocPrix ≥ 0.50 (plancher : les prix ne s'effondrent pas en deçà de 50 %)
            double facteurChocPrix = Math.Max(0.50,
                1.0 + (chocCarburantRel * elasticitePrixParCarburant) + alea);

            result.FacteurChocPrix = facteurChocPrix;
            // ────────────────────────────────────────────────────────────────────────

            // 4. Riz — incompressible mais le prix monte avec le choc carburant
            double depensesRiz = menage.DepensesRizJour * facteurInflationJour * facteurChocPrix;

            // 5. Eau et électricité Jirama (quotidien, tous les jours si connecté)
            double depensesEau = menage.AccesEau ? menage.DepensesEauJour * facteurInflationJour : 0;
            double prixKWhAjuste = Jirama != null ? Jirama.PrixElectriciteArKWh * facteurInflationJour : 0;
            double kwhConsommes = menage.AccesElectricite ? menage.ConsommationElecKWhJour : 0;
            double depensesElectricite = kwhConsommes * prixKWhAjuste;
            result.DepensesEau = depensesEau;
            result.DepensesElectricite = depensesElectricite;
            menage.TotalDepensesJirama += depensesEau + depensesElectricite;

            bool estJourPaiementJirama = (menage.AccesEau || menage.AccesElectricite) && jourCourant % 30 == 0;
            double depensesTransportJirama = estJourPaiementJirama ? coutTransportJirama * facteurInflationJour : 0;
            result.DepensesTransportJirama = depensesTransportJirama;
            menage.TotalTransportJirama += depensesTransportJirama;

            if (Jirama != null)
                Jirama.EnregistrerPaiementMenage(depensesEau, depensesElectricite, kwhConsommes, tauxTVA);

            // 6. Dépenses alimentaires : incompressibles mais ajustées par le choc de prix
            double depensesAlimentaires = menage.DepensesAlimentairesJour * facteurInflationJour * facteurChocPrix;

            // ── Réduction comportementale ────────────────────────────────────────────
            // Mécanisme asymétrique selon la classe :
            //   • Ménages pauvres (Subsistance, InformelBas) → réduisent l'alimentaire (survie)
            //   • Ménages aisés (FormelQualifie, Cadre) → réduisent les loisirs d'abord,
            //     ne touchent à l'alimentaire qu'en dernier recours
            double revenuNetPourComportement = Math.Max(1.0, revenuNet);
            double partAlimentaireActuelle = (depensesAlimentaires + depensesRiz) / revenuNetPourComportement;
            double reductionQuantite = 0.0;
            if (partAlimentaireActuelle > partRevenuAlimentaireNormale)
            {
                double surcharge = partAlimentaireActuelle - partRevenuAlimentaireNormale;

                // Atténuation pour les ménages aisés : ils absorbent le choc par les loisirs
                double attenuation = menage.Classe switch
                {
                    ClasseSocioEconomique.Cadre => 0.20,            // 80% d'atténuation : coupe les loisirs d'abord
                    ClasseSocioEconomique.FormelQualifie => 0.40,   // 60% d'atténuation
                    ClasseSocioEconomique.FormelBas => 0.70,        // 30% d'atténuation
                    _ => 1.0                                         // aucune atténuation : les pauvres sacrifient l'alimentaire
                };

                reductionQuantite = Math.Min(0.40, surcharge * elasticiteComportementMenage * attenuation);
                depensesAlimentaires *= (1.0 - reductionQuantite);
                depensesRiz *= (1.0 - reductionQuantite * 0.5); // riz plus vital → réduction moitié moindre
            }

            result.ReductionQuantiteAlimentaire = reductionQuantite;
            result.DepensesAlimentairesSimulee = depensesAlimentaires;   // composant brut pour routage B2C
            result.DepensesRiz = depensesRiz;
            menage.TotalDepensesRiz += depensesRiz;
            // ────────────────────────────────────────────────────────────────────────

            // 6b. Consommation totale
            double depensesIncompressibles = depensesAlimentaires
                                             + menage.DepensesDiversJour * facteurInflationJour
                                             + depensesTransport
                                             + depensesRiz
                                             + depensesEau + depensesElectricite
                                             + depensesTransportJirama;

            double budgetDiscretionnaire = Math.Max(0, (revenuNet - depensesIncompressibles) * menage.PropensionConsommation);
            double consommationTotale = depensesIncompressibles + budgetDiscretionnaire;

            // TVA payée sur la consommation (hors transport public)
            double consomSoumiseTVA = consommationTotale
                - (menage.Transport == ModeTransport.TransportPublic ? Math.Min(depensesTransport, consommationTotale) : 0);
            double tvaPayee = consomSoumiseTVA * (tauxTVA / (1.0 + tauxTVA));
            result.TVAPayee = tvaPayee;

            // Déficit : puiser dans l'épargne si nécessaire
            if (consommationTotale > revenuNet)
            {
                double deficit = consommationTotale - revenuNet;
                if (menage.Epargne >= deficit)
                    menage.Epargne -= deficit;
                else
                {
                    consommationTotale = revenuNet + menage.Epargne;
                    menage.Epargne = 0;
                }
            }

            result.Consommation = consommationTotale;
            menage.TotalConsomme += consommationTotale;

            // 7. Épargne
            double resteApresConsommation = revenuNet - consommationTotale;
            if (resteApresConsommation > 0)
                menage.Epargne += resteApresConsommation;

            result.EpargneJour = Math.Max(0, resteApresConsommation);
            result.EpargneTotale = menage.Epargne;

            return result;
        }
    }
}
