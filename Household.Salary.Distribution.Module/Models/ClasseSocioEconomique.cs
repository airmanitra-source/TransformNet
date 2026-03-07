namespace Household.Module.Models
{
    /// <summary>
    /// Classification socio-économique des ménages malgaches.
    /// Basée sur la distribution log-normale des salaires et le revenu mensuel.
    /// </summary>
    public enum ClasseSocioEconomique
    {
        /// <summary>Revenus de subsistance (&lt;Q1) — Ultra-pauvres, aucune capacité d'épargne</summary>
        Subsistance = 0,

        /// <summary>Secteur informel bas (Q1-Q2) — Petits métiers, épargne très faible</summary>
        InformelBas = 1,

        /// <summary>Secteur formel bas (Q2-Q3) — Employés simples, début d'épargne</summary>
        FormelBas = 2,

        /// <summary>Secteur formel qualifié (Q3-Q4) — Techniciens, épargne modérée</summary>
        FormelQualifie = 3,

        /// <summary>Cadres supérieurs (Q4-Q5) — Managers, cadres, forte épargne</summary>
        Cadre = 4
    }
}
