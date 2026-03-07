namespace MachineLearning.Web.Components.Pages;

public partial class Documentation
{
    // ── Shared record types ───────────────────────────────────────────────────

    private readonly record struct FormulaItem(string Nom, string Formule, string Description);
    private readonly record struct AméliorationItem(int Numero, string Titre, string ImpactPIB, string Valeur, string Source, string BadgeClass);
    private readonly record struct MécanismeItem(string Titre, string Formule, string Description, string Params);
    private readonly record struct ParamItem(string Nom, string Valeur, string Source);
    private readonly record struct ChocItem(string Icone, string Titre, string Couleur, string[] Items);
    private readonly record struct SecteurItem(string Icone, string Nom, string Part, string Tresorerie);
    private readonly record struct ComposanteInflationItem(string Nom, string Poids, string Formule, string Calibrage);

    // ── Boucle journalière ───────────────────────────────────────────────────

    private static readonly string[] BouclePasDeTemps =
    [
        "Gouvernement : salaires fonctionnaires + dépenses publiques + intérêts dette",
        "BCM : crédit bancaire (ΔCR), M3 mise à jour, règle de Taylor",
        "Saisonnalité : facteurs agricoles, touristiques, prix riz",
        "Remittances + aide internationale injectées en MGA",
        "Commerce extérieur : exports FOB (+M3) + imports CIF (−M3) avec bruit ±15%",
        "Jirama : production eau/électricité, facturation ménages et entreprises",
        "Entreprises : ventes B2C + B2B, recrutement/licenciement, IS/TVA, CNaPS",
        "Ménages : consommation par classe, épargne, autoconsommation agricole",
        "Inflation endogène : Phillips + cost-push + monétaire + anticipations",
        "Taux de change : balance commerciale + PPA + intervention BCM",
        "Chocs stochastiques : cyclone (nov–avr), sécheresse (mai–nov)",
        "Snapshot : PIB ×3 approches, M3, Gini, dette, validation macro",
        "Recalibration mensuelle (si activée) : correction paramètres sur cibles INSTAT",
    ];

    // ── Formules PIB ─────────────────────────────────────────────────────────

    private static readonly FormulaItem[] FormulesPIB =
    [
        new("PIB par la demande",
            "PIB_D = C + FBCF + G + (X − M) + Loyers imputés",
            "C = consommation ménages, FBCF = 18% PIB (investissement), G = dépenses publiques, (X−M) = balance commerciale ±15%/jour"),
        new("PIB par la valeur ajoutée",
            "PIB_VA = Σ VA(entreprises) + VA(Jirama) + VA(agriculture) + Loyers imputés",
            "VA = Production − Consommations intermédiaires. Correction commerciale distribuée sur agents avec poids aléatoires."),
        new("PIB par les revenus",
            "PIB_R = Salaires + Fonctionnaires + EBE(corrigé) + Impôts + Loyers imputés",
            "EBE = excédent brut d'exploitation. Convergence forcée avec PIB_D (écart distribué)."),
    ];

    // ── Hypothèses de calibrage ───────────────────────────────────────────────

    private static readonly AméliorationItem[] Améliorations =
    [
        new(1, "Secteur Agricole (Agriculture, Construction)", "+30% PIB VA", "~5 400 Mds MGA/an VA", "INSTAT 2024", "bg-danger"),
        new(2, "Fiscalité informelle (exonération IS/TVA)", "−70% recettes fiscales", "~85% entreprises", "INSTAT", "bg-danger"),
        new(3, "Aide internationale (Dons)", "+11% recettes État", "1 000 Mds / 9 mois", "TOFE sept. 2025", "bg-danger"),
        new(4, "Subventions État → Jirama", "−500 Mds budget/an", "~500 Mds MGA/an", "FMI Art. IV", "bg-danger"),
        new(5, "Cotisations CNaPS", "+18% charges entreprises", "18% patron + 1% salarié", "Code Travail MG", "bg-warning text-dark"),
        new(6, "Transferts diaspora (remittances)", "+5% revenus ménages", "~$600M/an", "BCM 2024", "bg-warning text-dark"),
        new(7, "Loyers imputés (SCN 2008)", "+5–8% PIB", "~1 200 Mds MGA/an", "SCN 2008", "bg-warning text-dark"),
        new(8, "FBCF (investissement réel)", "Corrige PIB demande", "FBCF publique = 13,5 Mds/jour", "TOFE sept. 2025", "bg-info"),
        new(9, "Personnel État (350k agents)", "Corrige PIB revenus", "2 720 Mds / 9 mois = 10,1 Mds/jour", "TOFE sept. 2025", "bg-info"),
        new(10, "Lien emploi ménage ↔ entreprise", "Cohérence micro/macro", "1 chef de ménage employé = 1 employeur simulé", "Hypothèse modèle", "bg-primary"),
        new(11, "Variation journalière CIF/FOB ±15%", "Réalisme commerce ext.", "Bruit aléatoire [×0.85 – ×1.15] par catégorie/jour", "Hypothèse modèle", "bg-success"),
        new(12, "Réconciliation des 3 PIB", "PIB_D = PIB_VA = PIB_R", "Écart distribué sur VA/EBE des agents commerciaux", "SCN 2008", "bg-success"),
        new(13, "Calibration TOFE sept. 2025", "Réalisme budget État", "6 lignes budgétaires calibrées (Personnel, Fonct., Intérêts, Capital, Dons)", "TOFE sept. 2025", "bg-danger"),
        new(14, "Dette publique initiale", "Réalisme macro", "29 250 Mds MGA (~$6,5 Mds)", "FMI Art. IV 2024", "bg-info"),
    ];

    // ── Mécanismes de transmission ───────────────────────────────────────────

    private static readonly MécanismeItem[] MécanismesTransmission =
    [
        new("Carburant → prix locaux",
            "ΔPrix = ΔCarburant × ε_carburant + N(0, σ_marché)",
            "Élasticité calibrée sur Madagascar. +10% carburant → ~7% inflation locale (ε=0.70). Délai d'ajustement via moyenne mobile.",
            "ε=0.70, σ=0.10, référence=5 500 MGA/L"),
        new("Panier alimentaire — réduction quantités",
            "Réduction = σ_logistique(chocs, ε_comportement, part_alimentaire)",
            "Courbe logistique : les ménages pauvres réduisent les quantités quand les prix montent. Pas de substitution parfaite.",
            "ε_comp=0.65, part_alim=40%, classe Subsistance la plus vulnérable"),
        new("Pass-through salarial → inflation",
            "π_salarial = η × ΔSalaire / Salaire",
            "Une hausse du SMIG (+50%) génère ~7.5% d'inflation via les coûts de production. Essentiel pour évaluer les politiques salariales.",
            "η=0.15, applicable au scénario SMIG"),
        new("Pass-through change → prix imports",
            "ΔP_imports = ζ × Δe / e",
            "30% de la dépréciation MGA transmis aux prix intérieurs (économie importatrice ~40% PIB).",
            "ζ=0.30, importations = ~40% PIB Madagascar"),
        new("Phillips — demande → inflation",
            "π_Phillips = β × (NAIRU − u) / NAIRU",
            "Courbe de Phillips augmentée. Quand le chômage passe sous le NAIRU (18%), l'inflation accélère.",
            "β=0.03, NAIRU=18%, poids=20%"),
        new("Monétaire — excès M3 → inflation",
            "π_mon = λ × (ΔM3/M3 − ΔY/Y)",
            "Théorie quantitative de la monnaie. L'excès de croissance monétaire par rapport à la croissance réelle génère de l'inflation.",
            "λ=0.30, poids=15%"),
    ];

    // ── Composantes inflation ─────────────────────────────────────────────────

    private static readonly ComposanteInflationItem[] ComposantesInflation =
    [
        new("Anticipations adaptatives", "w = 0.40",
            "π_ant(t) = (1-α)×π_ant(t-1) + α×π(t-1)",
            "α=0.60 (adaptatif), ancrage 6% (cible BCM implicite)"),
        new("Demand-pull (Phillips)", "w = 0.20",
            "π_dem = β × (NAIRU − u) / NAIRU",
            "β=0.03, NAIRU=18% (sous-emploi structurel Madagascar)"),
        new("Cost-push (carburant + imports)", "w = 0.25",
            "π_push = δ×ΔCarb/Carb + ε×ΔCIF/CIF + η×ΔSal/Sal",
            "δ=0.20, ε=0.12, η=0.15 — Madagascar très dépendant des imports"),
        new("Monétaire (excès M3)", "w = 0.15",
            "π_mon = λ × (ΔM3/M3 − ΔY/Y)",
            "λ=0.30 — croissance M3 > PIB potentiel génère de l'inflation"),
    ];

    // ── Paramètres taux de change ─────────────────────────────────────────────

    private static readonly ParamItem[] ParamsChange =
    [
        new("Taux initial", "4 500 MGA/USD", "BCM sept. 2024"),
        new("Réserves BCM", "$2.5 Mds USD", "BCM 2024"),
        new("Dépréciation structurelle", "5%/an", "Historique MGA 2015-2024"),
        new("Élasticité balance commerciale", "0.50", "Flottement géré BCM"),
        new("Poids PPA", "0.30", "Convergence lente"),
        new("Intervention BCM", "0.50", "Lissage modéré"),
        new("Réserves min. (arrêt intervention)", "3 mois imports", "FMI recommandation"),
        new("Pass-through inflation", "ζ = 0.30", "Madagascar ~0.20-0.40"),
        new("Élasticité remittances", "0.50", "Littérature Afrique sub-Sahar."),
    ];

    // ── Paramètres bancaires ─────────────────────────────────────────────────

    private static readonly ParamItem[] ParamsBancaires =
    [
        new("Réserves obligatoires", "13%", "BCM règlement en vigueur"),
        new("Croissance crédit/jour", "0.041%", "Vise +15%/an"),
        new("Taux débiteur", "16%/an", "BCM rapport 2024"),
        new("Taux créditeur", "4.5%/an", "BCM rapport 2024"),
        new("NPL (prêts non-performants)", "7–9%", "BCM 2024"),
        new("Prob. défaut/jour", "0.03%", "Calibré NPL cible"),
        new("Part crédit entreprises", "75%", "BCM ventilation"),
        new("Part dépôts à vue (M1)", "55%", "BCM 2024"),
        new("SCB initial (BFM)", "2 430 Mds MGA", "BFM sept. 2025"),
        new("M3/PIB ratio", "~35%", "BCM / WB"),
    ];

    // ── Chocs stochastiques ──────────────────────────────────────────────────

    private static readonly ChocItem[] ChocsStochastiques =
    [
        new("🌪️", "Cyclones (nov–avr)", "danger",
        [
            "Prob. 0.3%/jour en saison → ~54% chance sur 6 mois",
            "1.5 cyclones/an en moyenne (BNGRC)",
            "Récoltes −20–50%, destruction BTP",
            "Reconstruction : 55% BTP + 30% matériaux + 15% transport",
            "Phase reconstruction dure 30–90 jours post-cyclone",
        ]),
        new("☀️", "Sécheresse Grand Sud (mai–nov)", "warning",
        [
            "Prob. 0.1%/jour en saison sèche",
            "Affecte ~8% ménages (Grand Sud = 10% pop.)",
            "Production agricole −60% dans zones touchées",
            "Aide alimentaire ~3 000 MGA/jour/ménage (PAM/BNGRC)",
            "Migration interne : 12% ménages touchés → villes",
        ]),
        new("🌾", "Saisonnalité agricole", "success",
        [
            "Soudure (oct–déc) : prix riz +35%, productivité −50%",
            "Récolte principale (mars–mai) : prix riz −35%",
            "Haute saison tourisme : juil–oct (+45%)",
            "Emploi agricole saisonnier : ±25% sur l'année",
            "Amplitude calibrée INSTAT sur séries mensuelles TBE",
        ]),
    ];

    // ── Secteurs d'activité ───────────────────────────────────────────────────

    private static readonly SecteurItem[] SecteursActivite =
    [
        new("🌾", "Agriculture", "30% entreprises", "2 000 000 MGA"),
        new("🧵", "Textiles / Industrie", "Résiduel", "10 000 000 MGA"),
        new("🏪", "Commerces", "Résiduel", "5 000 000 MGA"),
        new("🔧", "Services", "Résiduel", "8 000 000 MGA"),
        new("⛏️", "Secteur minier", "Résiduel", "50 000 000 MGA"),
        new("🏗️", "Construction (BTP)", "5% entreprises", "15 000 000 MGA"),
        new("🏨", "Hôtellerie / Tourisme", "3% entreprises", "12 000 000 MGA"),
    ];

    // ── Notations tensorielles ────────────────────────────────────────────────

    private readonly record struct NotationItem(string Symbole, string Dimension, string Description, string Code);

    private static readonly NotationItem[] Notations =
    [
        new("Q(m,n,t)", "M×N×T", "Achats ménage m, produit n, jour t", "DailyHouseholdResult.*"),
        new("P(k,n,t)", "K×N×T", "Prix entreprise k, produit n, jour t", "IPriceModule.AjusterPrix()"),
        new("V_B2C(n,t)", "N×T", "Ventes entreprise→ménage (neutre M3)", "CompanyDailyResult.VentesB2C"),
        new("V_H2H(n,t)", "N×T", "Ventes ménage→ménage informel (neutre M3)", "DepensesAlimentairesInformel"),
        new("V_X(n,t)", "N×T", "Exports → +M3 (injection devises)", "Exporter.TotalExportationsFOB"),
        new("I_IMP(n,t)", "N×T", "Imports → −M3 (fuite devises)", "Importer.TotalImportationsCIF"),
        new("MS(t)", "T", "M3 = MS(t-1) + ΔCR + [FOB−CIF]", "Bank.MasseMonetaireM3"),
        new("E(m,t)", "M×T", "Épargne ménage m au jour t", "Household.Epargne"),
        new("T(k,t)", "K×T", "Trésorerie entreprise k au jour t", "Company.Tresorerie"),
    ];

    // ── Catégories produits ───────────────────────────────────────────────────

    private readonly record struct CategorieProduitItem(int Index, string Nom, string Champ, string Destinataire, string TVA, string BadgeClass);

    private static readonly CategorieProduitItem[] CategoriesProduits =
    [
        new(1, "Alimentaire informel", "DepensesAlimentairesInformel", "Entreprises informelles", "❌", "bg-danger"),
        new(2, "Alimentaire formel", "DepensesAlimentairesFormel", "Entreprises formelles", "✅ 20%", "bg-danger"),
        new(3, "Riz (local + importé)", "DepensesRiz", "Implicite (inclus alim.)", "❌", "bg-warning text-dark"),
        new(4, "Eau (Jirama)", "DepensesEau", "Jirama", "✅", "bg-info"),
        new(5, "Électricité (Jirama)", "DepensesElectricite", "Jirama", "✅", "bg-info"),
        new(6, "Transport", "DepensesTransport", "ITransportationModule", "Mixte", "bg-secondary"),
        new(7, "Loisirs / Tourisme", "DepensesLoisirs", "Entreprises tourisme", "✅ 20%", "bg-success"),
        new(8, "Biens divers", "Consommation − Σ(1..7)", "Réparti", "Mixte", "bg-primary"),
    ];

    // ── AIDS — catégories ─────────────────────────────────────────────────────

    private readonly record struct CategorieAIDSMappingItem(int Index, string Icone, string Nom, string ChampSnapshot, double Beta, string TypeEngel, string BadgeClass);

    private static readonly CategorieAIDSMappingItem[] CategoriesAIDSMapping =
    [
        new(0, "🏪", "Alimentaire informel", "DepensesAlimentairesInformelTotal", -0.08, "Inférieur fort (Engel)", "bg-danger"),
        new(1, "🛒", "Alimentaire formel", "DepensesAlimentairesFormelTotal", -0.03, "Inférieur", "bg-danger"),
        new(2, "🍚", "Riz", "DepensesRizTotales", -0.06, "Inférieur (Engel fort)", "bg-warning text-dark"),
        new(3, "💧", "Eau (Jirama)", "RecettesEauJirama", -0.01, "Incompressible", "bg-info"),
        new(4, "⚡", "Électricité (Jirama)", "RecettesElectriciteJirama", 0.01, "Quasi-unitaire", "bg-info"),
        new(5, "🚌", "Transport", "DepensesTransportTotales", 0.02, "Quasi-unitaire", "bg-secondary"),
        new(6, "🎭", "Loisirs / Tourisme", "DepensesLoisirsTotales", 0.08, "Supérieur (luxe)", "bg-success"),
        new(7, "🛍️", "Biens divers", "ConsommationTotale − Σ(1..7)", 0.07, "Supérieur", "bg-primary"),
    ];
}
