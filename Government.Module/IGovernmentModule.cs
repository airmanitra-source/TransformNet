using System.Data;

namespace Government.Module
{
    public interface IGovernmentModule
    {
        /// <summary>
        /// Calcule l'IRSA mensuel selon le barème progressif malgache.
        /// L'impôt est calculé par tranches marginales.
        /// </summary>
        double CalculerIRSAMensuel(Models.Government government, double salaireMensuelBrut);
        double CalculerIRSAJournalier(Models.Government government, double salaireMensuelBrut);
        double TauxEffectifIRSA(Models.Government government, double salaireMensuelBrut);
    }
}