namespace MachineLearning.Web.Components.Pages;

public partial class DocumentationCalibration
{
    private readonly record struct AméliorationItem(
        int Numero,
        string Titre,
        string ImpactPIB,
        string Valeur,
        string Source,
        string BadgeClass);

    private readonly record struct FormulaItem(
        string Nom,
        string Formule,
        string Description);

    private static readonly AméliorationItem[] Améliorations =
    [
        new(1, "Secteur Agricole (Agriculture, Construction)", "+30% PIB VA", "~5 400 Mds MGA/an VA", "INSTAT 2024", "bg-danger"),
        new(2, "Fiscalité informelle (exonération IS/TVA)", "-70% recettes fiscales", "~85% entreprises", "INSTAT", "bg-danger"),
        new(3, "Aide internationale (Dons)", "+11% recettes État", "1 000 Mds / 9 mois", "TOFE sept. 2025", "bg-danger"),
        new(4, "Subventions État → Jirama", "-500 Mds budget/an", "~500 Mds MGA/an", "FMI Art. IV", "bg-danger"),
        new(5, "Cotisations CNaPS", "+18% charges entreprises", "18% patron + 1% salarié", "Code Travail MG", "bg-warning text-dark"),
        new(6, "Transferts diaspora (remittances)", "+5% revenus ménages", "~$600M/an", "BCM 2024", "bg-warning text-dark"),
        new(7, "Loyers imputés (SCN 2008)", "+5-8% PIB", "~1 200 Mds MGA/an", "SCN 2008", "bg-warning text-dark"),
        new(8, "FBCF (investissement réel)", "Corrige PIB demande", "FBCF publique = 13,5 Mds/jour", "TOFE sept. 2025", "bg-info"),
        new(9, "Personnel État (350k agents)", "Corrige PIB revenus", "2 720 Mds / 9 mois = 10,1 Mds/jour", "TOFE sept. 2025", "bg-info"),
        new(10, "Lien emploi ménage ↔ entreprise", "Cohérence micro/macro salaires", "1 chef de ménage employé = 1 employeur simulé", "Hypothèse modèle", "bg-primary"),
        new(11, "Variation journalière CIF/FOB (±15%)", "Réalisme commerce ext.", "Bruit aléatoire [×0.85 – ×1.15] par catégorie/jour", "Hypothèse modèle", "bg-success"),
        new(12, "Réconciliation des 3 PIB", "PIB_D = PIB_VA = PIB_R", "Écart distribué sur VA/EBE des agents commerciaux", "SCN 2008", "bg-success"),
        new(13, "Calibration TOFE sept. 2025", "Réalisme budget État", "6 lignes budgétaires calibrées (Personnel, Fonct., Intérêts, Capital, Dons)", "TOFE sept. 2025", "bg-danger"),
        new(14, "Dette publique initiale", "Réalisme macro", "29 250 Mds MGA (~$6,5 Mds)", "FMI Art. IV 2024", "bg-info"),
    ];

    private static readonly FormulaItem[] FormulesPIB =
    [
        new("PIB par la demande",
            "PIB = C + FBCF + G + (X − M) + Loyers imputés",
            "C = consommation ménages, FBCF = formation brute de capital fixe (18% PIB), G = dépenses publiques, (X-M) = balance commerciale avec variation journalière ±15%"),
        new("PIB par la valeur ajoutée",
            "PIB = Σ(VA entreprises + correction commerce) + VA Jirama + Loyers imputés",
            "VA = Production − Consommations intermédiaires. Correction commerce = écart PIB_D distribué sur agents import/export avec poids aléatoires."),
        new("PIB par les revenus",
            "PIB = Salaires + Fonctionnaires + Jirama + EBE (corrigé) + Impôts + Loyers imputés",
            "EBE inclut la correction commerciale. Les 3 approches convergent exactement (écart 0,0%)."),
    ];
}
