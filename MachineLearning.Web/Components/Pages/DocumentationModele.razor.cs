namespace MachineLearning.Web.Components.Pages;

public partial class DocumentationModele
{
    private readonly record struct NotationItem(
        string Symbole,
        string Dimension,
        string Description,
        string Code);

    private readonly record struct CorrespondanceCodeItem(
        string Composant,
        string Code,
        string Detail);

    private readonly record struct CategorieProduitItem(
        int Index,
        string Nom,
        string Champ,
        string Destinataire,
        string TVA,
        string BadgeClass);

    private readonly record struct ComposanteItem(
        string Composant,
        string Implementation,
        string Fichier);

    private readonly record struct ComposanteManquanteItem(
        string Composant,
        string Probleme,
        string Extension);

    private readonly record struct ParametreEstimableItem(
        string Parametre,
        string Observable,
        string Methode,
        string Donnee);

    private static readonly NotationItem[] Notations =
    [
        new("K", "—", "Nombre d'entreprises", "_entreprises.Count + _importateurs.Count + _exportateurs.Count"),
        new("N", "—", "Nombre de produits (ou catégories de dépenses)", "8 catégories dans DailyHouseholdResult"),
        new("M", "—", "Nombre de ménages", "_config.NombreMenages"),
        new("T", "—", "Nombre de périodes (jours)", "_config.DureeJours"),
        new("Q(m,n,t)", "M × N × T", "Quantité du produit n achetée par ménage m auprès d'une entreprise", "DailyHouseholdResult.*"),
        new("S(m₁,m₂,n,t)", "M × M × N × T", "Quantité du produit n vendue par ménage m₁ au ménage m₂ (H2H informel)", "DepensesAlimentairesInformel (proxy)"),
        new("P(k,n,t)", "K × N × T", "Prix unitaire du produit n vendu par entreprise k", "IPriceModule.AjusterPrixParCarburant()"),
        new("P_H(m₁,n,t)", "M × N × T", "Prix informel du produit n vendu par ménage m₁ (pas de TVA)", "Prix implicite (pas de TVA)"),
        new("A(k,n)", "K × N", "Matrice d'affectation produit-entreprise", "ESecteurActivite + EstInformel"),
        new("V_B2C(n,t)", "N × T", "Ventes entreprise → ménage (neutre MS)", "CompanyDailyResult.VentesB2C"),
        new("V_H2H(n,t)", "N × T", "Ventes ménage → ménage (informel, neutre MS)", "DepensesAlimentairesInformel (proxy)"),
        new("V_X(n,t)", "N × T", "Ventes entreprise → étranger (exports, +MS injection)", "Exporter.TotalExportationsFOB"),
        new("I_IMP(n,t)", "N × T", "Achats étranger → entreprise (imports, −MS fuite)", "Importer.TotalImportationsCIF"),
        new("CIF(k,t)", "K × T", "Valeur CIF des marchandises importées par entreprise k (fuite devises)", "Importer.ValeurCIFJour"),
        new("R_H2H(m,t)", "M × T", "Revenu informel du ménage vendeur (ventes H2H reçues)", "Implicite dans Household.Epargne"),
        new("E(m,t)", "M × T", "Épargne du ménage m au temps t", "Household.Epargne"),
        new("T(k,t)", "K × T", "Trésorerie de l'entreprise k au temps t", "Company.Tresorerie"),
        new("MS(t)", "T", "Masse monétaire M3 = MS(t-1) + ΔCR(t) + [FOB(t) − CIF(t)]", "Bank.MasseMonetaireM3"),
    ];

    private static readonly CorrespondanceCodeItem[] CorrespondancesCode =
    [
        new("Q(m,n,t) — Achats B2C", "DailyHouseholdResult", "Consommation, DepensesAlimentaires, DepensesRiz, DepensesEau, DepensesElectricite, DepensesTransport, DepensesLoisirs"),
        new("S(m₁,m₂,n,t) — Ventes H2H", "DepensesAlimentairesInformel (proxy)", "85% des achats alimentaires routés vers entreprises informelles — proxy des ventes inter-ménages"),
        new("P(k,n,t) — Prix entreprise", "IPriceModule.AjusterPrixParCarburant()", "Ajustement dynamique par élasticité carburant + aléa marché"),
        new("P_H(m₁,n,t) — Prix H2H", "Prix implicite (pas de TVA)", "Prix informels négociés, volatiles, sans marge formelle"),
        new("A(k,n) — Affectation", "ESecteurActivite + EstInformel", "Routage 85% informel / 15% formel dans SimulerUnJour()"),
        new("V_B2C(n,t) — Ventes B2C", "CompanyDailyResult.VentesB2C", "Entreprise → Ménage, transfert E(m) → T(k), neutre MS"),
        new("V_H2H(n,t) — Ventes H2H", "DepensesAlimentairesInformel (proxy)", "Ménage → Ménage, neutre sur MS(t), redistribue la monnaie entre E(m)"),
        new("V_X(n,t) — Exports", "Exporter.TotalExportationsFOB", "Entreprise → Étranger, injecte des devises → +MS(t)"),
        new("I_IMP(n,t) — Imports", "Importer.TotalImportationsCIF", "Étranger → Entreprise, fuite CIF → −MS(t). Taxes (DD+Accise+TVA) restent dans le système"),
        new("CIF(k,t) — Coût import", "Importer.ValeurCIFJour × facteurInflation", "Valeur CIF journalière + DD + Accise + TVA import + Redevance 2%"),
        new("R_H2H(m,t) — Revenu H2H", "Implicite dans Household.Epargne", "Revenu informel du ménage vendeur, s'ajoute au salaire"),
        new("E(m,t) — Épargne ménage", "Household.Epargne", "Y_salaire + R_H2H + Remittance − C_B2C − C_H2H"),
        new("T(k,t) — Trésorerie", "Company.Tresorerie", "CA_B2C + V_X − CIF − salaires − IS − TVA − charges"),
        new("CA(t) — Chiffre d'affaires", "DailySnapshotViewModel", "Σ(VentesB2C + VentesB2B) + Σ(FOB) − Σ(CIF)"),
        new("MS(t) — Masse monétaire", "Bank.MasseMonetaireM3", "MS(t-1) + ΔCR(t) + [FOB(t) − CIF(t)]. B2C et H2H neutres."),
        new("ΔCR(t) — Création crédit", "BankModule.SimulerOctroiCredit()", "M3 × tauxCroissanceCreditJour, 75% entreprises / 25% ménages"),
        new("FOB(t) − CIF(t) — Balance commerciale", "DailySnapshotViewModel.BalanceCommerciale", "FOB exportateurs (→+MS) − CIF importateurs (→−MS)"),
    ];

    private static readonly CategorieProduitItem[] CategoriesProduits =
    [
        new(1, "Alimentaire informel", "DepensesAlimentairesInformel", "Entreprises EstInformel = true", "❌", "bg-danger"),
        new(2, "Alimentaire formel", "DepensesAlimentairesFormel", "Entreprises EstInformel = false", "✅ 20%", "bg-danger"),
        new(3, "Riz (local + importé)", "DepensesRiz", "Implicite (inclus dans alimentaire)", "❌", "bg-warning text-dark"),
        new(4, "Eau (Jirama)", "DepensesEau", "Jirama", "✅", "bg-info"),
        new(5, "Électricité (Jirama)", "DepensesElectricite", "Jirama", "✅", "bg-info"),
        new(6, "Transport", "DepensesTransport", "ITransportationModule → informel/formel/carburant", "Mixte", "bg-secondary"),
        new(7, "Loisirs / Tourisme", "DepensesLoisirs", "Entreprises HotellerieTourisme", "✅ 20%", "bg-success"),
        new(8, "Biens divers", "Consommation − Alim − Transport", "Réparti uniformément (informel/formel)", "Mixte", "bg-primary"),
    ];

    private static readonly ComposanteItem[] ComposantesImplementees =
    [
        new("M ménages individuels", "List<Household> avec Id, Épargne, Salaire", "Household.Module/Models/Household.cs"),
        new("K entreprises individuelles", "List<Company> + importateurs + exportateurs", "Company.Module/Models/Company.cs"),
        new("T périodes (jours)", "_jourCourant itéré dans SimulerUnJour()", "EconomicSimulator.cs"),
        new("E(m,t) Épargne", "Household.Epargne mis à jour chaque jour", "HouseholdModule.SimulerJournee()"),
        new("T(k,t) Trésorerie", "Company.Tresorerie mis à jour chaque jour", "CompanyModule.SimulerJournee()"),
        new("MS(t) Masse monétaire", "Bank.MasseMonetaireM3 = ΣÉpargne + ΣTrésorerie", "BankModule.cs"),
        new("CA(k,t) Ventes agrégées", "CompanyDailyResult.VentesB2C + VentesB2B", "CompanyDailyResult.cs"),
        new("Prix ajustés", "IPriceModule.AjusterPrixParCarburant()", "PriceModule.cs"),
        new("Split informel/formel", "85% / 15% répartition de la demande", "EconomicSimulator.cs:745"),
        new("ΔCR(t) Crédit bancaire", "SimulerOctroiCredit() — M3 × taux croissance", "BankModule.cs"),
        new("Transport routé", "ITransportationModule.RouterDepenseTransport()", "TransportationModule.cs"),
    ];

    private static readonly ComposanteManquanteItem[] ComposantesManquantes =
    [
        new("Q(m,n,t) par produit individuel", "Achats agrégés par catégorie, pas par produit", "Ajouter Product + HouseholdPurchase"),
        new("P(k,n,t) prix unitaire par produit × entreprise", "Prix au niveau secteur, pas produit", "Catalogue de prix par produit"),
        new("A(k,n) matrice d'affectation", "Entreprises affectées à un secteur, pas à des produits", "Lien explicite produit → entreprise"),
        new("V(n,t) ventes par produit", "Ventes agrégées par entreprise", "Suivi des ventes par produit individuel"),
    ];

    private static readonly ParametreEstimableItem[] ParametresEstimables =
    [
        new("α(n) — Part de base", "✅ Oui (via w(n,t) moyen)", "Moyenne des parts budgétaires observées", "EPM + IPC INSTAT"),
        new("β(n) — Élasticité-revenu", "✅ Oui (via ΔE/ΔC)", "Régression AIDS : Δw = β × Δln(C/P*)", "Série MS(t) + revenus"),
        new("γ(n,j) — Élasticité-prix croisée", "⚠️ Partiel (prix carburant)", "Régression panel : Δw = γ × Δln(P)", "Série IPC par catégorie"),
        new("TauxEpargne s(m)", "✅ Oui (ΔE/Y)", "s = 1 − C/Y = ΔE / Y directement observable", "MS_ménages(t)"),
        new("PropensionConsommation c(m)", "✅ Oui (C/Y)", "c = 1 − s, complément du taux d'épargne", "MS_ménages(t)"),
        new("PartAlimentaire w₁+w₂+w₃", "⚠️ Via prior EPM", "Loi d'Engel : w_alim décroît avec le revenu", "Enquête EPM + revenu"),
        new("PartTransport w₆", "✅ Oui (ITransportationModule)", "Décomposition sectorielle taxi-be/moto/voiture", "ITransportationModule + prix carburant"),
        new("PartLoisirs w₇", "⚠️ Résiduel", "w₇ = f(revenu, classe) — très élastique au revenu", "Données tourisme + EPM"),
    ];

    // ═══════════════════════════════════════════
    //  10. Intégration Simulation ↔ AIDS
    // ═══════════════════════════════════════════

    private readonly record struct MappingSimToAIDSItem(
        string ChampSnapshot,
        string ParametreAIDS,
        string Calcul);

    private readonly record struct MappingAIDSToSimItem(
        string ParametreAIDS,
        string ChampConfig,
        string Formule);

    private readonly record struct CategorieAIDSMappingItem(
        int Index,
        string Icone,
        string Nom,
        string ChampSnapshot,
        double Beta,
        string TypeEngel,
        string BadgeClass);

    private static readonly MappingSimToAIDSItem[] MappingsSimToAIDS =
    [
        new("MasseMonetaireM3 (jour 1)", "MasseMonetaireInitiale", "Valeur directe du premier snapshot"),
        new("MasseMonetaireM3 (dernier / premier)", "ObjectifCroissancePourcent", "(M3_fin / M3_debut − 1) × 100"),
        new("DepensesAlimentairesInformelTotal", "w₁ (Alimentaire informel)", "Moyenne / Σ dépenses × ratio informel"),
        new("DepensesAlimentairesFormelTotal", "w₂ (Alimentaire formel)", "Moyenne / Σ dépenses × ratio formel"),
        new("DepensesRizTotales", "w₃ (Riz)", "Moyenne / Σ dépenses"),
        new("RecettesEauJirama", "w₄ (Eau)", "Moyenne / Σ dépenses"),
        new("RecettesElectriciteJirama", "w₅ (Électricité)", "Moyenne / Σ dépenses"),
        new("DepensesTransportTotales", "w₆ (Transport)", "Moyenne / Σ dépenses"),
        new("DepensesLoisirsTotales", "w₇ (Loisirs)", "Moyenne / Σ dépenses"),
        new("ConsommationTotaleMenages − Σ(1..7)", "w₈ (Biens divers)", "Résiduel : total − catégories identifiées"),
        new("PrixElectriciteArKWh (fin/début)", "IndicesPrixProjectes[4]", "Ratio borné [0.5, 3.0]"),
        new("Dépenses par catégorie (fin/début)", "IndicesPrixProjectes[0..7]", "SafeRatio(fin, début) par catégorie"),
    ];

    private static readonly MappingAIDSToSimItem[] MappingsAIDSToSim =
    [
        new("TauxCroissanceJournalier", "CroissanceCreditJour", "(1 + objectif%)^(1/N) − 1"),
        new("Σ(w_i × Δp_i)", "TauxInflation", "Inflation pondérée par parts budgétaires AIDS"),
        new("w₁ + w₂ + w₃ projetés", "PartRevenuAlimentaire", "Somme des 3 parts alimentaires projetées"),
        new("IndicesPrixProjectes[5]", "PrixCarburantLitre", "5 500 × indice prix transport projeté"),
        new("HorizonJours", "DureeJours", "Horizon temporel du modèle AIDS"),
    ];

    private static readonly CategorieAIDSMappingItem[] CategoriesAIDSMapping =
    [
        new(0, "🏪", "Alimentaire informel", "DepensesAlimentairesInformelTotal", -0.08, "Inférieur (Engel)", "bg-danger"),
        new(1, "🛒", "Alimentaire formel", "DepensesAlimentairesFormelTotal", -0.03, "Inférieur", "bg-danger"),
        new(2, "🍚", "Riz", "DepensesRizTotales", -0.06, "Inférieur (Engel fort)", "bg-warning text-dark"),
        new(3, "💧", "Eau (Jirama)", "RecettesEauJirama", -0.01, "Incompressible", "bg-info"),
        new(4, "⚡", "Électricité (Jirama)", "RecettesElectriciteJirama", 0.01, "Quasi-unitaire", "bg-info"),
        new(5, "🚌", "Transport", "DepensesTransportTotales", 0.02, "Quasi-unitaire", "bg-secondary"),
        new(6, "🎭", "Loisirs / Tourisme", "DepensesLoisirsTotales", 0.08, "Supérieur (luxe)", "bg-success"),
        new(7, "🛍️", "Biens divers", "ConsommationTotaleMenages − Σ(1..7)", 0.07, "Supérieur", "bg-primary"),
    ];
}
