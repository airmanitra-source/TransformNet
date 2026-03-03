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
        new(3, "Aide internationale", "+35% budget État", "~$1,5 Mds/an", "OCDE/CAD 2024", "bg-danger"),
        new(4, "Subventions État → JIRAMA", "-500 Mds budget/an", "~500 Mds MGA/an", "FMI Art. IV", "bg-danger"),
        new(5, "Cotisations CNaPS", "+18% charges entreprises", "18% patron + 1% salarié", "Code Travail MG", "bg-warning text-dark"),
        new(6, "Transferts diaspora (remittances)", "+5% revenus ménages", "~$600M/an", "BCM 2024", "bg-warning text-dark"),
        new(7, "Loyers imputés (SCN 2008)", "+5-8% PIB", "~1 200 Mds MGA/an", "SCN 2008", "bg-warning text-dark"),
        new(8, "FBCF (investissement réel)", "Corrige PIB demande", "~18% du PIB", "Banque Mondiale", "bg-info"),
        new(9, "Salaires fonctionnaires + JIRAMA", "Corrige PIB revenus", "~200 000 agents", "Min. Finances", "bg-info"),
    ];

    private static readonly FormulaItem[] FormulesPIB =
    [
        new("PIB par la demande",
            "PIB = C + FBCF + G + (X − M) + Loyers imputés",
            "C = consommation ménages, FBCF = formation brute de capital fixe (18% PIB), G = dépenses publiques, (X-M) = balance commerciale"),
        new("PIB par la valeur ajoutée",
            "PIB = Σ(VA entreprises) + VA JIRAMA + Loyers imputés",
            "VA = Production − Consommations intermédiaires (achats B2B + électricité). Inclut agriculture, construction, secteur informel."),
        new("PIB par les revenus",
            "PIB = Salaires + Fonctionnaires + JIRAMA + Profits + Impôts + Loyers imputés",
            "Salaires = ménages + fonctionnaires (200k agents) + employés JIRAMA (5k). Profits = bénéfice avant impôt de toutes les entreprises."),
    ];
}
