using Company.Module.Models;

namespace Company.Module;

/// <summary>
/// Implémentation du module d'investissement productif.
/// Gère la FBCF privée par entreprise, la dépréciation du capital,
/// et l'effet de l'accumulation de capital sur la productivité.
/// </summary>
public class InvestmentModule : IInvestmentModule
{
    /// <summary>
    /// Facteurs de réinvestissement par secteur.
    /// Les secteurs capitalistiques réinvestissent davantage.
    /// Source : BCM/INSTAT — structure de la FBCF par branche.
    /// </summary>
    private static readonly Dictionary<ESecteurActivite, double> FacteursReinvestissement = new()
    {
        { ESecteurActivite.Agriculture, 0.60 },        // Faible : paysans investissent peu
        { ESecteurActivite.Textiles, 1.10 },           // ZFI : renouvellement machines
        { ESecteurActivite.Commerces, 0.80 },          // Stock, aménagement
        { ESecteurActivite.Services, 0.90 },           // Équipement IT, véhicules
        { ESecteurActivite.SecteurMinier, 1.50 },      // Très capitalistique (Ambatovy)
        { ESecteurActivite.Construction, 1.20 },       // Engins, matériel BTP
        { ESecteurActivite.HotellerieTourisme, 1.00 }, // Rénovation, aménagement
    };

    /// <summary>
    /// Facteurs de dépréciation par secteur.
    /// Agriculture = faible (terre, bétail), Minier = élevé (machines lourdes).
    /// </summary>
    private static readonly Dictionary<ESecteurActivite, double> FacteursDepreciation = new()
    {
        { ESecteurActivite.Agriculture, 0.50 },        // Terre ne se déprécie pas, outils faiblement
        { ESecteurActivite.Textiles, 1.20 },           // Machines à coudre, métiers à tisser
        { ESecteurActivite.Commerces, 0.80 },          // Aménagements, stocks
        { ESecteurActivite.Services, 1.00 },           // Véhicules, IT
        { ESecteurActivite.SecteurMinier, 1.50 },      // Engins lourds, forages
        { ESecteurActivite.Construction, 1.30 },       // Engins BTP, camions
        { ESecteurActivite.HotellerieTourisme, 0.90 }, // Bâtiments, mobilier
    };

    /// <inheritdoc/>
    public InvestmentResult SimulerInvestissement(
        Models.Company entreprise,
        double beneficeAvantImpot,
        double tauxUtilisationCapacite,
        double tauxReinvestissement = 0.25,
        double tauxDepreciationAnnuel = 0.07,
        double seuilUtilisationInvestissement = 0.70,
        double elasticiteCapitalProductivite = 0.08)
    {
        var result = new InvestmentResult();

        // ═══════════════════════════════════════════
        //  1. DÉPRÉCIATION QUOTIDIENNE
        // ═══════════════════════════════════════════
        double tauxDeprecSectoriel = GetTauxDepreciationSectoriel(entreprise.SecteurActivite, tauxDepreciationAnnuel);
        double tauxDeprecJour = tauxDeprecSectoriel / 365.0;
        double depreciation = entreprise.StockCapital * tauxDeprecJour;
        entreprise.StockCapital = Math.Max(0, entreprise.StockCapital - depreciation);
        result.DepreciationJour = depreciation;
        entreprise.DepreciationCumulee += depreciation;

        // ═══════════════════════════════════════════
        //  2. DÉCISION D'INVESTIR
        // ═══════════════════════════════════════════
        // Critères : bénéfice positif + taux d'utilisation suffisant + trésorerie saine
        bool beneficiairePositif = beneficeAvantImpot > 0;
        bool utilisationSuffisante = tauxUtilisationCapacite >= seuilUtilisationInvestissement;
        bool tresorerieSaine = entreprise.Tresorerie > 0;

        double investissement = 0;

        if (beneficiairePositif && utilisationSuffisante && tresorerieSaine)
        {
            // Montant = bénéfice × taux de réinvestissement × facteur sectoriel
            double tauxReinvSectoriel = GetTauxReinvestissementSectoriel(
                entreprise.SecteurActivite, tauxReinvestissement);
            investissement = Math.Max(0, beneficeAvantImpot) * tauxReinvSectoriel;

            // Plafond : ne pas investir plus de 20% de la trésorerie
            double plafondTresorerie = entreprise.Tresorerie * 0.20;
            investissement = Math.Min(investissement, plafondTresorerie);

            // L'investissement sort de la trésorerie et va dans le stock de capital
            entreprise.Tresorerie -= investissement;
            entreprise.StockCapital += investissement;
            entreprise.InvestissementCumule += investissement;
            result.AInvesti = true;
        }

        result.InvestissementJour = investissement;
        result.VariationNetteCapital = investissement - depreciation;
        result.NouveauStockCapital = entreprise.StockCapital;

        // ═══════════════════════════════════════════
        //  3. EFFET SUR LA PRODUCTIVITÉ
        // ═══════════════════════════════════════════
        // Rendements décroissants du capital :
        // Productivité = Base × (1 + α × ln(1 + K/K₀))
        // K₀ = capital de référence (trésorerie initiale du secteur)
        //
        // Cela signifie :
        //   - K = 0 → productivité = base (pas d'investissement)
        //   - K = K₀ → productivité = base × (1 + α × ln(2)) ≈ base × 1.055
        //   - K = 10×K₀ → productivité = base × (1 + α × ln(11)) ≈ base × 1.19
        //
        double capitalReference = Math.Max(1, entreprise.Tresorerie + entreprise.StockCapital);
        double ratioCapital = entreprise.StockCapital / capitalReference;
        double gainProductivite = elasticiteCapitalProductivite * Math.Log(1.0 + ratioCapital);

        // Appliquer le gain de productivité (cumulatif via le stock de capital)
        // On ne modifie pas ProductiviteParEmployeJour directement (c'est la base).
        // On stocke le facteur de productivité capital dans l'entreprise.
        entreprise.FacteurProductiviteCapital = 1.0 + gainProductivite;
        result.GainProductivite = gainProductivite * entreprise.ProductiviteParEmployeJour;

        return result;
    }

    /// <inheritdoc/>
    public double GetTauxReinvestissementSectoriel(Models.ESecteurActivite secteur, double tauxBase)
    {
        return tauxBase * FacteursReinvestissement.GetValueOrDefault(secteur, 1.0);
    }

    /// <inheritdoc/>
    public double GetTauxDepreciationSectoriel(Models.ESecteurActivite secteur, double tauxBase)
    {
        return tauxBase * FacteursDepreciation.GetValueOrDefault(secteur, 1.0);
    }
}
