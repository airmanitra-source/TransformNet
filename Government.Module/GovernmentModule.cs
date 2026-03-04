using Government.Module.Models;

namespace Government.Module
{
    public class GovernmentModule : IGovernmentModule
    {

        public GovernmentModule()
        {
                
        }
        /// <summary>
        /// Calcule l'IRSA journalier (prorata du mensuel).
        /// </summary>
        public double CalculerIRSAJournalier(Models.Government government, double salaireMensuelBrut)
        {
            return CalculerIRSAMensuel(government, salaireMensuelBrut) / 30.0;
        }

        /// <summary>
        /// Taux effectif d'IRSA pour un salaire donné.
        /// </summary>
        public double TauxEffectifIRSA(Models.Government government, double salaireMensuelBrut)
        {
            if (salaireMensuelBrut <= 0) return 0;
            return CalculerIRSAMensuel(government, salaireMensuelBrut) / salaireMensuelBrut;
        }

        /// <summary>
        /// Calcule l'IRSA mensuel selon le barème progressif malgache.
        /// L'impôt est calculé par tranches marginales.
        /// </summary>
        public double CalculerIRSAMensuel(Models.Government government, double salaireMensuelBrut)
        {
            double impot = 0;
            double resteImposable = salaireMensuelBrut;

            for (int i = 0; i < government.TranchesIRSA.Count; i++)
            {
                var tranche = government.TranchesIRSA[i];
                double plafondTranche = (i + 1 < government.TranchesIRSA.Count)
                    ? government.TranchesIRSA[i + 1].SeuilMin
                    : double.MaxValue;

                double montantDansTranche = Math.Min(resteImposable, plafondTranche - tranche.SeuilMin);
                if (montantDansTranche <= 0) break;

                impot += montantDansTranche * tranche.Taux;
                resteImposable -= montantDansTranche;
            }

            // Minimum de perception IRSA : 2 000 MGA/mois si imposable
            if (impot > 0 && impot < 2_000 && salaireMensuelBrut > government.TranchesIRSA[0].SeuilMin)
                impot = 2_000;

            return impot;
        }

        public DailyGovernmentResult SimulerJournee(
            Models.Government government,
            List<Company.Module.Models.DailyHouseholdResult> resultsMenages,
            List<Company.Module.Models.CompanyDailyResult> resultsEntreprises,
            List<Company.Module.Models.DailyImporterResult> resultsImportateurs,
            List<Company.Module.Models.DailyExporterResult> resultsExportateurs,
            Company.Module.Models.Jirama? Jirama = null,
            double consoElecEtatKWhJour = 0,
            double aideInternationaleJour = 0,
            double subventionJiramaJour = 0,
            double masseSalarialeFonctionnairesJour = 0,
            double tauxReinvestissementPrive = 0,
            double depensesCapitalJour = 0,
            double interetsDetteJour = 0)
        {
            var result = new DailyGovernmentResult();

            // 1. Collecte IR (déjà prélevé sur les ménages)
            double irCollecte = resultsMenages.Sum(r => r.ImpotIR);
            government.TotalRecettesIR += irCollecte;
            result.RecettesIR = irCollecte;

            // 2. Collecte IS (toutes les entreprises : normales + importateurs + exportateurs)
            double isCollecte = resultsEntreprises.Sum(r => r.ImpotIS)
                              + resultsImportateurs.Sum(r => r.ImpotIS)
                              + resultsExportateurs.Sum(r => r.ImpotIS);
            government.TotalRecettesIS += isCollecte;
            result.RecettesIS = isCollecte;

            // 3. Collecte TVA (ventes locales)
            double tvaCollectee = resultsEntreprises.Sum(r => r.TVACollectee)
                                + resultsImportateurs.Sum(r => r.TVACollectee)
                                + resultsExportateurs.Sum(r => r.TVACollectee);

            // 3b. TVA Jirama (reversement de la TVA collectée sur factures eau + électricité)
            double tvaJirama = Jirama?.TVACollecteeJour ?? 0;
            tvaCollectee += tvaJirama;
            result.TVAJirama = tvaJirama;

            government.TotalRecettesTVA += tvaCollectee;
            result.RecettesTVA = tvaCollectee;

            // 4. Recettes douanières (importations)
            double droitsDouane = resultsImportateurs.Sum(r => r.DroitsDouane);
            double accise = resultsImportateurs.Sum(r => r.Accise);
            double tvaImport = resultsImportateurs.Sum(r => r.TVAImport);
            double redevancesImport = resultsImportateurs.Sum(r => r.RedevanceStatistique);
            government.TotalDroitsDouane += droitsDouane;
            government.TotalAccise += accise;
            government.TotalTVAImport += tvaImport;
            government.TotalRedevances += redevancesImport;
            result.DroitsDouane = droitsDouane;
            result.Accise = accise;
            result.TVAImport = tvaImport;

            // 5. Taxes à l'exportation
            double taxeExport = resultsExportateurs.Sum(r => r.TaxeExport);
            double redevancesExport = resultsExportateurs.Sum(r => r.RedevanceExport);
            government.TotalTaxeExport += taxeExport;
            government.TotalRedevances += redevancesExport;
            result.TaxeExport = taxeExport;

            result.RecettesDouanieres = droitsDouane + accise + tvaImport + redevancesImport + taxeExport + redevancesExport;

            // 6. Volumes commerce extérieur
            result.ImportationsCIF = resultsImportateurs.Sum(r => r.ValeurCIF);
            result.ExportationsFOB = resultsExportateurs.Sum(r => r.ValeurFOB);
            result.BalanceCommerciale = result.ExportationsFOB - result.ImportationsCIF;

            // 7. Recettes totales du jour (fiscales + aide internationale)
            double recettesJour = irCollecte + isCollecte + tvaCollectee + result.RecettesDouanieres;

            // 7b. Aide internationale
            government.TotalAideInternationale += aideInternationaleJour;
            result.AideInternationale = aideInternationaleJour;
            recettesJour += aideInternationaleJour;

            // 7c. Cotisations CNaPS collectées (toutes entreprises formelles)
            double cotisationsCNaPS = resultsEntreprises.Sum(r => r.CotisationsCNaPS)
                                    + resultsImportateurs.Sum(r => r.CotisationsCNaPS)
                                    + resultsExportateurs.Sum(r => r.CotisationsCNaPS);
            government.TotalCotisationsCNaPS += cotisationsCNaPS;
            result.CotisationsCNaPS = cotisationsCNaPS;
            recettesJour += cotisationsCNaPS;

            result.RecettesTotales = recettesJour;

            // 8. Dépenses publiques du jour
            double depenses = government.DepensesPubliquesJour;

            // 8b. Électricité Jirama de l'État (éclairage public + bâtiments publics)
            double facteurInflationJour = 1.0 + (government.TauxInflation / 365.0);
            double prixKWh = Jirama?.PrixElectriciteArKWh ?? 653;
            double depensesElecEtat = consoElecEtatKWhJour * prixKWh * facteurInflationJour;
            if (Jirama != null && consoElecEtatKWhJour > 0)
            {
                Jirama.EnregistrerPaiementEtat(depensesElecEtat, consoElecEtatKWhJour);
            }
            result.DepensesElectriciteEtat = depensesElecEtat;
            government.TotalDepensesElectricite += depensesElecEtat;
            depenses += depensesElecEtat;

            // 8c. Subventions à la Jirama
            result.SubventionsJirama = subventionJiramaJour;
            government.TotalSubventionsJirama += subventionJiramaJour;
            depenses += subventionJiramaJour;
            if (Jirama != null)
            {
                Jirama.Tresorerie += subventionJiramaJour;
            }

            // 8d. Masse salariale des fonctionnaires (calculée micro : somme des salaires des ménages fonctionnaires)
            result.SalairesFonctionnaires = masseSalarialeFonctionnairesJour;
            government.TotalSalairesFonctionnaires += masseSalarialeFonctionnairesJour;
            depenses += masseSalarialeFonctionnairesJour;

            // 8e. Intérêts de la dette publique (TOFE : dette extérieure + intérieure)
            result.InteretsDette = interetsDetteJour;
            depenses += interetsDetteJour;

            // 8f. Dépenses en capital (FBCF publique, TOFE : financement intérieur + extérieur)
            result.DepensesCapital = depensesCapitalJour;
            depenses += depensesCapitalJour;

            government.TotalDepensesPubliques += depenses;
            result.DepensesPubliques = depenses;

            // 9. FBCF (Formation Brute de Capital Fixe)
            // FBCF privée = part des bénéfices positifs réinvestie par les entreprises
            double profitPositifTotal = resultsEntreprises.Sum(r => Math.Max(0, r.BeneficeAvantImpot))
                                      + resultsImportateurs.Sum(r => Math.Max(0, r.BeneficeAvantImpot))
                                      + resultsExportateurs.Sum(r => Math.Max(0, r.BeneficeAvantImpot));
            double fbcfPrivee = profitPositifTotal * tauxReinvestissementPrive;

            // FBCF publique = dépenses en capital de l'État (source TOFE directe)
            result.FBCF = fbcfPrivee + depensesCapitalJour;

            // Consommation finale de l'État (G dans le PIB) = dépenses courantes - subventions (transferts)
            // G exclut : subventions (transferts), dépenses en capital (dans FBCF), intérêts (transfert au créancier)
            result.ConsommationFinaleEtat = depenses - subventionJiramaJour - depensesCapitalJour - interetsDetteJour;

            // 10. Transferts sociaux (redistribution)
            double transferts = government.DepensesPubliquesJour * government.TauxRedistribution;
            result.TransfertsSociaux = transferts;

            // 11. Solde budgétaire
            double soldeJour = recettesJour - depenses;
            government.SoldeBudgetaire += soldeJour;
            result.SoldeJour = soldeJour;
            result.SoldeCumule = government.SoldeBudgetaire;

            // 12. Dette publique (accumule les déficits)
            if (soldeJour < 0)
            {
                government.DettePublique += Math.Abs(soldeJour);
            }
            result.DettePublique = government.DettePublique;

            return result;
        }
    }
}
