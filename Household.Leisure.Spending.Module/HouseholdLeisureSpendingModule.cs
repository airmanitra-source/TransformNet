using Household.Module.Models;

namespace Household.Leisure.Spending.Module
{
    public class HouseholdLeisureSpendingModule : IHouseholdLeisureSpendingModule
    {
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
    }
}
