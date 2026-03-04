using Government.Module.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
