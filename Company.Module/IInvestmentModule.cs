using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Résultat de l'investissement productif d'une entreprise pour un jour donné.
/// </summary>
public class InvestmentResult
{
    /// <summary>Montant investi ce jour (MGA).</summary>
    public double InvestissementJour { get; set; }

    /// <summary>Dépréciation du capital ce jour (MGA).</summary>
    public double DepreciationJour { get; set; }

    /// <summary>Variation nette du stock de capital = Investissement - Dépréciation (MGA).</summary>
    public double VariationNetteCapital { get; set; }

    /// <summary>Nouveau stock de capital après investissement et dépréciation (MGA).</summary>
    public double NouveauStockCapital { get; set; }

    /// <summary>Gain de productivité résultant de l'investissement (MGA/employé/jour).</summary>
    public double GainProductivite { get; set; }

    /// <summary>True si l'entreprise a décidé d'investir.</summary>
    public bool AInvesti { get; set; }
}

/// <summary>
/// Module d'investissement productif (FBCF privée par entreprise).
///
/// L'investissement productif est ESSENTIEL pour les simulations &gt;1 an :
///   - Sans investissement, la capacité de production reste fixe
///   - Le PIB stagne même avec une demande croissante
///   - La FBCF représente ~20-25% du PIB à Madagascar (BCM/INSTAT)
///
/// ══════════════════════════════════════════════════════════════
///  MODÈLE D'INVESTISSEMENT
/// ══════════════════════════════════════════════════════════════
///
///  1. DÉCISION D'INVESTIR
///     L'entreprise investit si :
///       - Bénéfice avant impôt &gt; 0 (rentable)
///       - Trésorerie suffisante (couvre les charges + marge)
///       - Taux d'utilisation capacité &gt; seuil (demande justifie l'investissement)
///
///  2. MONTANT INVESTI
///     Investissement = Bénéfice × TauxReinvestissement × FacteurSectoriel
///     Plafonné à une part de la trésorerie disponible.
///
///  3. EFFET SUR LA PRODUCTIVITÉ
///     Le capital accumulé augmente la productivité par employé.
///     Rendements décroissants : chaque MGA investi a un effet marginal décroissant.
///     Productivité = ProductivitéBase × (1 + α × ln(1 + K/K₀))
///     où K = stock de capital, K₀ = capital de référence, α = élasticité
///
///  4. DÉPRÉCIATION
///     Le capital se déprécie à un taux annuel de ~5-8% (Madagascar).
///     Machines, outils, bâtiments = ~10-15% ; Terre = ~0%.
///     Taux journalier = TauxAnnuel / 365.
///
///  Source : BCM, INSTAT SCN, FMI Article IV Madagascar.
/// ══════════════════════════════════════════════════════════════
/// </summary>
public interface IInvestmentModule
{
    /// <summary>
    /// Évalue et exécute l'investissement productif d'une entreprise pour un jour donné.
    /// Met à jour le stock de capital, applique la dépréciation, et calcule le gain de productivité.
    /// </summary>
    /// <param name="entreprise">L'entreprise qui investit.</param>
    /// <param name="beneficeAvantImpot">Bénéfice avant impôt du jour (MGA).</param>
    /// <param name="tauxUtilisationCapacite">Taux d'utilisation de la capacité (0-2+).</param>
    /// <param name="tauxReinvestissement">Part du bénéfice réinvestie (0-1, défaut 0.25).</param>
    /// <param name="tauxDepreciationAnnuel">Taux de dépréciation annuel du capital (défaut 0.07).</param>
    /// <param name="seuilUtilisationInvestissement">Seuil de taux d'utilisation pour investir (défaut 0.70).</param>
    /// <param name="elasticiteCapitalProductivite">Élasticité capital→productivité α (défaut 0.08).</param>
    /// <returns>Résultat de l'investissement.</returns>
    InvestmentResult SimulerInvestissement(
        Models.Company entreprise,
        double beneficeAvantImpot,
        double tauxUtilisationCapacite,
        double tauxReinvestissement = 0.25,
        double tauxDepreciationAnnuel = 0.07,
        double seuilUtilisationInvestissement = 0.70,
        double elasticiteCapitalProductivite = 0.08);

    /// <summary>
    /// Retourne le taux de réinvestissement sectoriel.
    /// Certains secteurs (minier, construction) investissent plus.
    /// </summary>
    double GetTauxReinvestissementSectoriel(Models.ESecteurActivite secteur, double tauxBase);

    /// <summary>
    /// Retourne le taux de dépréciation sectoriel.
    /// Agriculture = faible (terre), Minier = élevé (machines lourdes).
    /// </summary>
    double GetTauxDepreciationSectoriel(Models.ESecteurActivite secteur, double tauxBase);
}
