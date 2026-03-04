using Company.Module.Models;
using Household.Module.Models;

namespace Household.Module
{
    /// <summary>
    /// Implémentation du module Ménages.
    /// Centralise le comportement économique par classe socio-économique
    /// et la logique d'achat alimentaire.
    /// La distribution salariale (tirage, classification, stats) est déléguée
    /// à <c>HouseholdSalaryDistributionModule</c>.
    /// </summary>
    public class HouseholdModule : IHouseholdModule
    {
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

        /// <summary>
        /// Calcule les dépenses de loisirs du jour.
        /// 
        /// Mécanisme comportemental asymétrique face à la hausse des prix :
        /// - Ménages aisés (FormelQualifie, Cadre) : réduisent les LOISIRS en priorité
        ///   → courbe logistique agressive, les loisirs disparaissent avant l'alimentaire
        /// - Ménages pauvres (Subsistance, InformelBas) : pas ou peu de loisirs,
        ///   ils réduisent l'ALIMENTAIRE (géré par AcheteProduitsAlimentaires)
        /// 
        /// Fréquence :
        /// - Sorties weekend : samedi et/ou dimanche, avec probabilité aléatoire
        /// - Vacances : tous les ~90 jours pour les aisés, avec probabilité aléatoire
        /// - L'inflation cumulative réduit progressivement ces probabilités et budgets
        /// </summary>
        public (double DepensesLoisirs, double FacteurReduction, bool EstEnSortie, bool EstEnVacances)
            CalculerDepensesLoisirs(
                ClasseSocioEconomique classe,
                double budgetSortieWeekend,
                double budgetVacances,
                double probabiliteSortieWeekend,
                double probabiliteVacances,
                bool estWeekend,
                bool estPeriodeVacances,
                bool estEnVacancesEnCours,
                double cumulHaussePrix,
                double revenuDisponible,
                Company.Module.Models.Company? compagnieTourisme,
                Random random)
        {
            // Les ménages Subsistance et InformelBas n'ont pas de budget loisirs
            if (classe <= ClasseSocioEconomique.InformelBas
                && budgetSortieWeekend <= 0 && budgetVacances <= 0)
            {
                return (0, 1.0, false, false);
            }

            // ── Facteur de réduction des loisirs par la hausse des prix ──
            // Les ménages aisés coupent les loisirs AVANT l'alimentaire.
            // Courbe logistique plus sensible que celle de l'alimentaire :
            //   - x0 = 8% d'inflation (vs 25% pour l'alimentaire) → réaction plus précoce
            //   - k plus élevé → transition plus brutale
            double facteurReductionLoisirs = 1.0;
            if (cumulHaussePrix > 0)
            {
                double sensibilite = classe switch
                {
                    ClasseSocioEconomique.Cadre => 0.30,            // très sensible aux loisirs
                    ClasseSocioEconomique.FormelQualifie => 0.25,
                    ClasseSocioEconomique.FormelBas => 0.20,
                    _ => 0.15
                };
                double x0Loisirs = 8.0; // seuil de réaction à 8% d'inflation
                facteurReductionLoisirs = 1.0 / (1.0 + Math.Exp(sensibilite * (cumulHaussePrix - x0Loisirs)));
                facteurReductionLoisirs = Math.Max(facteurReductionLoisirs, 0.0); // peut tomber à 0 (suppression totale)
            }

            double depenses = 0;
            bool estEnSortie = false;
            bool estEnVacances = false;

            // ── Vacances en cours (priorité : le séjour continue) ──
            if (estEnVacancesEnCours)
            {
                estEnVacances = true;
                // Dépenses journalières de vacances = budget total / durée, ajusté par l'inflation
                double budgetJourVacances = budgetVacances * facteurReductionLoisirs;
                depenses = Math.Min(budgetJourVacances, revenuDisponible * 0.60);
                if (depenses > 0 && compagnieTourisme != null)
                {
                    compagnieTourisme.ChiffreAffairesCumule += depenses;
                    compagnieTourisme.Tresorerie += depenses;
                }
                return (depenses, facteurReductionLoisirs, false, true);
            }

            // ── Départ en vacances (période trimestrielle atteinte) ──
            if (estPeriodeVacances && budgetVacances > 0)
            {
                double probaAjustee = probabiliteVacances * facteurReductionLoisirs;
                if (random.NextDouble() < probaAjustee)
                {
                    estEnVacances = true;
                    double budgetJourVacances = budgetVacances * facteurReductionLoisirs;
                    depenses = Math.Min(budgetJourVacances, revenuDisponible * 0.60);
                    if (depenses > 0 && compagnieTourisme != null)
                    {
                        compagnieTourisme.ChiffreAffairesCumule += depenses;
                        compagnieTourisme.Tresorerie += depenses;
                    }
                    return (depenses, facteurReductionLoisirs, false, true);
                }
            }

            // ── Sortie weekend ──
            if (estWeekend && budgetSortieWeekend > 0)
            {
                double probaAjustee = probabiliteSortieWeekend * facteurReductionLoisirs;
                if (random.NextDouble() < probaAjustee)
                {
                    estEnSortie = true;
                    double budgetSortieAjuste = budgetSortieWeekend * facteurReductionLoisirs;
                    depenses = Math.Min(budgetSortieAjuste, revenuDisponible * 0.30);
                }
            }

            // ── Comptabilisation des ventes sur la compagnie tourisme ──
            // Les dépenses de loisirs du ménage sont les ventes directes de la compagnie
            if (depenses > 0 && compagnieTourisme != null)
            {
                compagnieTourisme.ChiffreAffairesCumule += depenses;
                compagnieTourisme.Tresorerie += depenses;
            }

            return (depenses, facteurReductionLoisirs, estEnSortie, estEnVacances);
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
