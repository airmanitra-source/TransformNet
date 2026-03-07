using Household.Module.Models;
using BankAgent = Bank.Module.Models.Bank;

namespace Bank.Module;

public class BankModule : IBankModule
{
    /// <summary>
    /// Calcule les dépôts, décompose en M0/M1/M2/M3, calcule le multiplicateur monétaire
    /// et les contreparties de la masse monétaire (avoirs extérieurs, crédit intérieur).
    /// Logique alignée sur les rapports conjoncturels trimestriels de la BCM.
    /// </summary>
    public void CalculerBilansBancaires(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        IEnumerable<Company.Module.Models.Company> entreprises,
        double partDepotsAVue,
        double partMonnaieCirculationDansM1,
        double ratioM3SurM2,
        double reservesBCMUSD,
        double tauxChangeMGAParUSD,
        double dettePublique)
    {
        // ── Dépôts ───────────────────────────────────────────────────────────
        banque.TotalDepotsMenages = menages.Sum(m => m.Epargne);
        banque.TotalDepotsEntreprises = entreprises.Sum(e => e.Tresorerie);
        double totalDepots = banque.TotalDepotsMenages + banque.TotalDepotsEntreprises;

        // Décomposition dépôts à vue / à terme
        banque.DepotsAVue = totalDepots * partDepotsAVue;
        banque.DepotsATerme = totalDepots * (1.0 - partDepotsAVue);

        // ── Réserves obligatoires (dérivées des dépôts) ──────────────────────
        banque.ReservesObligatoires = totalDepots * banque.TauxReserveObligatoire;
        // Excédent réel = SCB - RO (recalculé par CalculerLiquiditeBancaire)
        banque.ReservesExcedentaires = Math.Max(0, banque.SoldeEnCompteBanques - banque.ReservesObligatoires);
        banque.EcartMoyenSCB_RO = banque.ReservesExcedentaires;

        // ── Agrégats monétaires (BCM) ────────────────────────────────────────
        // M1 = Monnaie fiduciaire en circulation + Dépôts à vue
        // La monnaie fiduciaire est estimée comme une fraction de M1 (approche circulaire résolue)
        // M1 = DAV / (1 - partFiduciaire)
        double denominateurM1 = Math.Max(1.0 - partMonnaieCirculationDansM1, 0.1);
        banque.MasseMonetaireM1 = banque.DepotsAVue / denominateurM1;
        banque.MonnaieCirculation = banque.MasseMonetaireM1 * partMonnaieCirculationDansM1;

        // M2 = M1 + Dépôts à terme
        banque.MasseMonetaireM2 = banque.MasseMonetaireM1 + banque.DepotsATerme;

        // M3 = M2 × ratio (titres négociables, placements)
        banque.MasseMonetaireM3 = banque.MasseMonetaireM2 * ratioM3SurM2;

        // M0 = Monnaie en circulation + SCB (réserves détenues à la BFM)
        // Identité fondamentale : M0 = Fiduciaire + Réserves bancaires
        banque.BaseMonetaireM0 = banque.MonnaieCirculation + banque.SoldeEnCompteBanques;

        // Multiplicateur monétaire = M3 / M0
        banque.MultiplicateurMonetaire = banque.BaseMonetaireM0 > 0
            ? banque.MasseMonetaireM3 / banque.BaseMonetaireM0
            : 1.0;

        // ── Contreparties de M3 (bilan monétaire consolidé BCM) ──────────────
        // Avoirs extérieurs nets = Réserves BCM en USD × taux de change MGA/USD
        banque.AvoirsExtérieursNets = reservesBCMUSD * tauxChangeMGAParUSD;

        // Créances nettes sur l'État (proportion simplifiée de la dette publique)
        // BCM avance au Trésor ~10% de la dette publique
        banque.CreancesNettesEtat = dettePublique * 0.10;

        // Crédit intérieur net = Crédits à l'économie + Créances nettes sur l'État
        banque.CreditInterieurNet = banque.EncoursCreditEconomie + banque.CreancesNettesEtat;
    }

    /// <summary>
    /// Calcule le SCB et la liquidité bancaire quotidienne.
    /// Le SCB est affecté par 3 flux (tableau BFM « Liquidité bancaire ») :
    ///   1. Flux des FA → entrées/sorties de devises (balance commerciale + remittances)
    ///   2. Interventions nettes BFM → injection/ponction pour maintenir l'excédent cible
    ///   3. Flux endogènes SCB → variation nette des dépôts et crédits
    /// </summary>
    public void CalculerLiquiditeBancaire(
        BankAgent banque,
        double balanceCommercialeJour,
        double remittancesJour,
        double tauxChangeMGAParUSD,
        double intensiteInterventionBFM,
        double ratioExcedentCible,
        Random random)
    {
        // ── 1. Flux des FA (Foreign Assets) ──────────────────────────────────
        // Balance commerciale + Remittances → flux de devises convertis en MGA
        // Aléa ±20% pour décalage entre flux réels et flux financiers
        double aleaFA = 0.8 + random.NextDouble() * 0.4;
        banque.FluxFAJour = (balanceCommercialeJour + remittancesJour) * aleaFA;

        // ── 2. Flux endogènes du SCB ─────────────────────────────────────────
        // Crédits accordés augmentent les dépôts → RO augmente → SCB change
        double fluxEndogene = (banque.CreditsEntreprisesJour + banque.CreditsMenagesJour)
                            - banque.NPLRecuperesJour;

        // ── 3. Intervention BFM ──────────────────────────────────────────────
        // La BFM vise un écart SCB-RO autour du ratio d'excédent cible
        double excedentCible = banque.ReservesObligatoires * ratioExcedentCible;
        double excedentActuel = Math.Max(0, banque.SoldeEnCompteBanques - banque.ReservesObligatoires);
        double ecartExcedent = excedentCible - excedentActuel;

        // Si écart > 0 → excédent insuffisant → BFM injecte
        // Si écart < 0 → excédent excessif → BFM ponctionne
        banque.InterventionNetteBFMJour = ecartExcedent * intensiteInterventionBFM;
        banque.EncoursInterventionsBFM += banque.InterventionNetteBFMJour;

        // ── Mise à jour du SCB ───────────────────────────────────────────────
        banque.FluxSCBJour = banque.FluxFAJour + fluxEndogene + banque.InterventionNetteBFMJour;
        banque.SoldeEnCompteBanques += banque.FluxSCBJour;
        // Le SCB ne peut pas descendre sous 80% des RO (plancher de sécurité)
        banque.SoldeEnCompteBanques = Math.Max(banque.SoldeEnCompteBanques, banque.ReservesObligatoires * 0.8);

        // Recalculer l'excédent effectif
        banque.ReservesExcedentaires = Math.Max(0, banque.SoldeEnCompteBanques - banque.ReservesObligatoires);
        banque.EcartMoyenSCB_RO = banque.ReservesExcedentaires;

        // Liquidité avant/après intervention (courbes BFM)
        banque.LiquiditeAvantIntervention = banque.SoldeEnCompteBanques - banque.EncoursInterventionsBFM;
        banque.LiquiditeApresIntervention = banque.SoldeEnCompteBanques;
    }

    /// <summary>
    /// Simule la création de monnaie par octroi de crédits.
    /// Le crédit potentiel est borné par DEUX contraintes :
    ///   1. Multiplicateur monétaire : crédit max = M0 × (1/r) - encours
    ///   2. Liquidité SCB : crédit max = excédent SCB-RO × (1/r)
    /// La contrainte la plus stricte s'applique (binding constraint).
    /// </summary>
    public void SimulerOctroiCredit(
        BankAgent banque,
        IEnumerable<Company.Module.Models.Company> entreprises,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxCroissanceCreditJour,
        double partCreditEntreprises,
        Random random)
    {
        // Crédit à créer aujourd'hui = M3 × taux de croissance journalier ciblé
        double creditACreer = banque.MasseMonetaireM3 * tauxCroissanceCreditJour;
        if (creditACreer <= 0) return;

        // Contrainte 1 : capacité du multiplicateur monétaire
        double creditMaxMultiplicateur = banque.TauxReserveObligatoire > 0
            ? banque.BaseMonetaireM0 / banque.TauxReserveObligatoire
            : banque.BaseMonetaireM0 * 10;
        double margeMultiplicateur = Math.Max(0, creditMaxMultiplicateur - banque.EncoursCreditEconomie);

        // Contrainte 2 : liquidité SCB — seul l'excédent (SCB-RO) peut être prêté
        double margeSCB = banque.TauxReserveObligatoire > 0
            ? banque.EcartMoyenSCB_RO / banque.TauxReserveObligatoire
            : banque.EcartMoyenSCB_RO * 10;

        // La contrainte la plus stricte s'applique
        double margeCredit = Math.Min(margeMultiplicateur, margeSCB);
        creditACreer = Math.Min(creditACreer, margeCredit);
        if (creditACreer <= 0) return;

        double creditEntreprises = creditACreer * partCreditEntreprises;
        double creditMenages = creditACreer * (1.0 - partCreditEntreprises);

        banque.TotalCreditsAccordes += creditACreer;
        banque.EncoursCreditEconomie += creditACreer;
        banque.CreditsEntreprisesJour = creditEntreprises;
        banque.CreditsMenagesJour = creditMenages;

        // 1. Octroi de crédit aux entreprises : injecté dans la Trésorerie
        var entreprisesInvestissant = entreprises.Where(e => !e.EstInformel).ToList();
        if (entreprisesInvestissant.Count > 0)
        {
            double creditParEntreprise = creditEntreprises / entreprisesInvestissant.Count;
            foreach (var e in entreprisesInvestissant)
            {
                double allocation = creditParEntreprise * (0.5 + random.NextDouble());
                e.Tresorerie += allocation;
            }
        }
        else
        {
            var entreprisesList = entreprises.ToList();
            if (entreprisesList.Count > 0)
            {
                double creditParEntreprise = creditEntreprises / entreprisesList.Count;
                foreach (var e in entreprisesList)
                {
                    e.Tresorerie += creditParEntreprise * (0.5 + random.NextDouble());
                }
            }
        }

        // 2. Octroi de crédit aux ménages : injecté dans l'Épargne
        var menagesEligibles = menages.Where(m => m.Classe == ClasseSocioEconomique.Cadre || m.Classe == ClasseSocioEconomique.FormelQualifie).ToList();

        if (menagesEligibles.Count == 0)
        {
            menagesEligibles = menages.Where(m => m.Classe >= ClasseSocioEconomique.FormelBas).ToList();
        }

        if (menagesEligibles.Count > 0)
        {
            double creditParMenage = creditMenages / menagesEligibles.Count;
            foreach (var m in menagesEligibles)
            {
                double allocation = creditParMenage * (0.5 + random.NextDouble());
                m.Epargne += allocation;
            }
        }
    }

    /// <summary>
    /// Calcule les intérêts journaliers sur les dépôts et les crédits.
    /// Intérêts dépôts : rémunèrent l'épargne des ménages (classes Cadre/FormelQualifié).
    /// Intérêts crédits : coût du crédit, alimente la marge bancaire (NIM).
    /// </summary>
    public void CalculerInterets(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxInteretDepotsAnnuel,
        double tauxInteretCreditsAnnuel)
    {
        banque.TauxInteretDepots = tauxInteretDepotsAnnuel;
        banque.TauxInteretCredits = tauxInteretCreditsAnnuel;

        // Taux journaliers (intérêt simple pour la simulation)
        double tauxDepotJour = tauxInteretDepotsAnnuel / 365.0;
        double tauxCreditJour = tauxInteretCreditsAnnuel / 365.0;

        // Intérêts sur les dépôts (versés aux dépôts à terme uniquement — épargne rémunérée)
        banque.InteretsDepotJour = banque.DepotsATerme * tauxDepotJour;
        banque.InteretsDepotsCumules += banque.InteretsDepotJour;

        // Distribuer les intérêts aux ménages éligibles (bancarisés, classes moyennes/hautes)
        var menagesRemuneres = menages
            .Where(m => m.Epargne > 0 && (m.Classe == ClasseSocioEconomique.Cadre
                                        || m.Classe == ClasseSocioEconomique.FormelQualifie
                                        || m.Classe == ClasseSocioEconomique.FormelBas))
            .ToList();

        if (menagesRemuneres.Count > 0 && banque.InteretsDepotJour > 0)
        {
            double totalEpargneBancarisee = menagesRemuneres.Sum(m => m.Epargne);
            if (totalEpargneBancarisee > 0)
            {
                foreach (var m in menagesRemuneres)
                {
                    // Prorata de l'épargne de chaque ménage
                    double part = m.Epargne / totalEpargneBancarisee;
                    m.Epargne += banque.InteretsDepotJour * part;
                }
            }
        }

        // Intérêts sur les crédits (encours de crédits sains = encours - NPL)
        double encoursSains = Math.Max(0, banque.EncoursCreditEconomie - banque.EncoursNPL);
        banque.InteretsCreditJour = encoursSains * tauxCreditJour;
        banque.InteretsCreditsCumules += banque.InteretsCreditJour;

        // Marge nette d'intérêt (Net Interest Margin)
        banque.MargeNetteInteretJour = banque.InteretsCreditJour - banque.InteretsDepotJour;
    }

    /// <summary>
    /// Simule les défauts de remboursement (Non-Performing Loans) et les recouvrements.
    /// Chaque jour, une fraction de l'encours sain peut tomber en défaut (NPL).
    /// Simultanément, une fraction des NPL existants est récupérée ou provisionnée.
    /// </summary>
    public void SimulerNPL(
        BankAgent banque,
        double probabiliteDefautJour,
        double tauxRecouvrementJour,
        Random random)
    {
        // Encours de crédits sains (hors NPL)
        double encoursSains = Math.Max(0, banque.EncoursCreditEconomie - banque.EncoursNPL);

        // Nouveaux défauts : chaque jour, une probabilité de défaut s'applique à l'encours sain
        // Ajout d'un léger aléa autour de la probabilité de base
        double probaEffective = probabiliteDefautJour * (0.5 + random.NextDouble());
        banque.NouveauxNPLJour = encoursSains * probaEffective;
        banque.EncoursNPL += banque.NouveauxNPLJour;

        // Recouvrement / provisionnement des NPL existants
        banque.NPLRecuperesJour = banque.EncoursNPL * tauxRecouvrementJour;
        banque.EncoursNPL = Math.Max(0, banque.EncoursNPL - banque.NPLRecuperesJour);
        banque.ProvisionsCumulees += banque.NPLRecuperesJour;

        // Les NPL provisionnés réduisent l'encours de crédits total
        banque.EncoursCreditEconomie = Math.Max(0, banque.EncoursCreditEconomie - banque.NPLRecuperesJour);

        // Ratio NPL
        banque.RatioNPL = banque.EncoursCreditEconomie > 0
            ? banque.EncoursNPL / banque.EncoursCreditEconomie
            : 0;
    }

    /// <summary>
    /// Simule le crédit segmenté : bancaire formel, microfinance (IMF), tontines informelles.
    /// 
    /// Segmentation du marché du crédit à Madagascar :
    ///   - Bancaire formel (~10% ménages) : géré par SimulerOctroiCredit existant
    ///   - Microfinance IMF (~25% ménages) : taux 24-36%, petits montants
    ///   - Tontines informelles (~30% ménages) : épargne rotative, pas d'intérêt formel
    /// 
    /// Source : BCM rapport inclusion financière 2024, CNFI Madagascar.
    /// </summary>
    public void SimulerCreditSegmente(
        BankAgent banque,
        IEnumerable<Household.Module.Models.Household> menages,
        double tauxInteretMicrofinanceAnnuel,
        double plafondCreditMicrofinance,
        double plafondTontine,
        double probabiliteOctroiMicrofinanceJour,
        Random random)
    {
        var micro = banque.Microfinance;
        micro.TauxInteretMicrofinanceAnnuel = tauxInteretMicrofinanceAnnuel;
        micro.CreditsMicrofinanceJour = 0;
        micro.MobilisationTontinesJour = 0;

        double tauxMicroJour = tauxInteretMicrofinanceAnnuel / 365.0;

        foreach (var menage in menages)
        {
            switch (menage.AccesCredit)
            {
                case Household.Module.Models.TypeCredit.Microfinance:
                    // Intérêts journaliers sur l'encours existant
                    if (menage.EncoursCreditMenage > 0)
                    {
                        double interet = menage.EncoursCreditMenage * tauxMicroJour;
                        menage.TotalInteretsCredit += interet;
                        micro.InteretsMicrofinanceJour += interet;
                    }

                    // Nouveau crédit microfinance (probabilité journalière)
                    if (menage.EncoursCreditMenage < plafondCreditMicrofinance
                        && random.NextDouble() < probabiliteOctroiMicrofinanceJour)
                    {
                        // Montant basé sur le salaire mensuel (max 3x salaire, plafonné)
                        double montant = Math.Min(
                            menage.SalaireMensuel * (1.0 + random.NextDouble() * 2.0),
                            plafondCreditMicrofinance - menage.EncoursCreditMenage);
                        montant = Math.Max(0, montant);

                        menage.EncoursCreditMenage += montant;
                        menage.Epargne += montant;
                        micro.EncoursCreditMicrofinance += montant;
                        micro.CreditsMicrofinanceJour += montant;
                        micro.NbCreditsMicrofinanceActifs++;
                    }
                    break;

                case Household.Module.Models.TypeCredit.TontineInformelle:
                    // Tontine : chaque membre verse ~1/30 du plafond par jour
                    // et reçoit le pot quand c'est son tour (~1 fois par mois)
                    double versementJour = plafondTontine / 30.0;
                    if (menage.Epargne > versementJour)
                    {
                        menage.Epargne -= versementJour;
                        micro.EncoursTontines += versementJour;
                        micro.MobilisationTontinesJour += versementJour;

                        // Tour de recevoir le pot (~1/30 chance par jour)
                        if (random.NextDouble() < (1.0 / 30.0))
                        {
                            menage.Epargne += plafondTontine;
                            micro.EncoursTontines -= plafondTontine;
                            micro.NbTontinesActives++;
                        }
                    }
                    break;

                // BancaireFormel : déjà géré par SimulerOctroiCredit
                // Aucun : pas d'accès au crédit
                default:
                    break;
            }
        }

        micro.InteretsMicrofinanceCumules += micro.InteretsMicrofinanceJour;
        micro.MobilisationTontinesCumulees += micro.MobilisationTontinesJour;

        // Ratio NPL microfinance (meilleur que bancaire grâce au suivi de proximité)
        micro.RatioNPLMicrofinance = micro.EncoursCreditMicrofinance > 0
            ? Math.Min(0.08, micro.EncoursCreditMicrofinance * 0.00015) / micro.EncoursCreditMicrofinance
            : 0;
    }
}
