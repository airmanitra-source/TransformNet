using Company.Module.Models;
using Household.Module.Models;
using BankAgent = Bank.Module.Models.Bank;

namespace Bank.Module;

public class BankModule : IBankModule
{
    public void CalculerBilansBancaires(BankAgent banque, IEnumerable<Household.Module.Models.Household> menages, IEnumerable<Company.Module.Models.Company> entreprises)
    {
        // Les dépôts bancaires sont constitués de l'épargne accumulée des ménages et de la trésorerie des entreprises.
        banque.TotalDepotsMenages = menages.Sum(m => m.Epargne);
        banque.TotalDepotsEntreprises = entreprises.Sum(e => e.Tresorerie);
        
        // M3 (Masse monétaire au sens large) = Dépôts + Monnaie fiduciaire en circulation.
        // Dans notre modèle simplifié, la quasi-totalité de l'argent disponible est comptée comme scripturale.
        banque.MasseMonetaireM3 = banque.TotalDepotsMenages + banque.TotalDepotsEntreprises; 
    }

    public void SimulerOctroiCredit(BankAgent banque, IEnumerable<Company.Module.Models.Company> entreprises, IEnumerable<Household.Module.Models.Household> menages, double tauxCroissanceCreditJour, Random random)
    {
        // Crédit à créer aujourd'hui = M3 * taux de croissance journalier ciblé
        double creditACreer = banque.MasseMonetaireM3 * tauxCroissanceCreditJour;
        if (creditACreer <= 0) return;

        // Distribuer ce crédit : historiquement, en Afrique subsaharienne, 
        // les entreprises captent la majorité du crédit bancaire privé (~75%),
        // et les ménages (souvent non bancarisés) une minorité (~25%).
        double creditEntreprises = creditACreer * 0.75;
        double creditMenages = creditACreer * 0.25;

        banque.TotalCreditsAccordes += creditACreer;

        // 1. Octroi de crédit aux entreprises : injecté dans la Trésorerie
        var entreprisesInvestissant = entreprises.Where(e => !e.EstInformel).ToList(); // Souvent seules les formelles ont accès au crédit
        if (entreprisesInvestissant.Count > 0)
        {
            double creditParEntreprise = creditEntreprises / entreprisesInvestissant.Count;
            foreach (var e in entreprisesInvestissant)
            {
                // Un peu d'aléatoire pour ne pas donner l'exact même montant à tous
                double allocation = creditParEntreprise * (0.5 + random.NextDouble());
                e.Tresorerie += allocation;
            }
        }
        else
        {
            // Fallback si pas de formelles (peu probable)
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
        // En général, ce sont les classes moyennes ou cadres (FormelQualifie, Cadre) qui ont accès au crédit
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
}
