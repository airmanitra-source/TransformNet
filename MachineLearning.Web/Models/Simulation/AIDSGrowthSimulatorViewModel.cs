using MachineLearning.Web.Models.Simulation.Config;

namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Almost Ideal Demand System (AIDS) — Estimation de paramètres à partir
/// d'un objectif de croissance de la masse monétaire (M3).
///
/// Le modèle AIDS de Deaton & Muellbauer (1980) exprime les parts budgétaires :
///   w_i = α_i + Σ_j γ_ij × ln(p_j) + β_i × ln(X / P*)
///
/// Où :
///   w_i = part du budget consacrée au bien i
///   p_j = indice de prix du bien j
///   X   = dépense totale (≈ masse monétaire disponible pour la consommation)
///   P*  = indice de prix agrégé de Stone : ln(P*) = Σ w_i × ln(p_i)
/// </summary>
public class AIDSGrowthSimulatorViewModel
{
    // ═══════════════════════════════════════════
    //  CATÉGORIES DE DÉPENSES (N = 8)
    // ═══════════════════════════════════════════

    public static readonly string[] Categories =
    [
        "Alimentaire informel",
        "Alimentaire formel",
        "Riz (local + importé)",
        "Eau (Jirama)",
        "Électricité (Jirama)",
        "Transport",
        "Loisirs / Tourisme",
        "Biens divers"
    ];

    public static readonly string[] CategoriesIcons =
    [
        "🏪", "🛒", "🍚", "💧", "⚡", "🚌", "🎭", "🛍️"
    ];

    public int N => Categories.Length;

    // ═══════════════════════════════════════════
    //  INPUTS UTILISATEUR
    // ═══════════════════════════════════════════

    /// <summary>Masse monétaire initiale M3 (MGA).</summary>
    public double MasseMonetaireInitiale { get; set; } = 30_000_000_000_000;

    /// <summary>Objectif de croissance en % (ex : 15 = +15%).</summary>
    public double ObjectifCroissancePourcent { get; set; } = 15.0;

    /// <summary>Horizon en jours.</summary>
    public int HorizonJours { get; set; } = 365;

    /// <summary>
    /// Parts budgétaires observées w_i (somme ≈ 1).
    /// Calibrées sur EPM INSTAT Madagascar.
    /// </summary>
    public double[] PartsBudgetaires { get; set; } =
    [
        0.34,  // Alimentaire informel  (85% × 40%)
        0.06,  // Alimentaire formel    (15% × 40%)
        0.08,  // Riz
        0.02,  // Eau
        0.03,  // Électricité
        0.12,  // Transport
        0.05,  // Loisirs
        0.30   // Biens divers (résiduel)
    ];

    /// <summary>
    /// Indices de prix normalisés (base 1.0 = période de référence).
    /// </summary>
    public double[] IndicesPrix { get; set; } =
    [
        1.00,  // Alimentaire informel
        1.00,  // Alimentaire formel
        1.00,  // Riz
        1.00,  // Eau
        1.00,  // Électricité
        1.00,  // Transport
        1.00,  // Loisirs
        1.00   // Biens divers
    ];

    /// <summary>
    /// Indices de prix projetés à l'horizon (inflation attendue par catégorie).
    /// </summary>
    public double[] IndicesPrixProjectes { get; set; } =
    [
        1.08,  // Alimentaire informel  (+8%)
        1.06,  // Alimentaire formel    (+6%)
        1.10,  // Riz                   (+10%)
        1.04,  // Eau                   (+4%)
        1.05,  // Électricité           (+5%)
        1.12,  // Transport             (+12%)
        1.06,  // Loisirs               (+6%)
        1.07   // Biens divers          (+7%)
    ];

    // ═══════════════════════════════════════════
    //  RÉSULTATS AIDS
    // ═══════════════════════════════════════════

    public bool EstEstime { get; set; }

    /// <summary>α_i — parts de base (intercepts).</summary>
    public double[] Alpha { get; set; } = [];

    /// <summary>β_i — élasticités-revenu (effet de la dépense totale).</summary>
    public double[] Beta { get; set; } = [];

    /// <summary>γ_ij — élasticités-prix croisées (matrice N×N).</summary>
    public double[,] Gamma { get; set; } = new double[0, 0];

    /// <summary>Parts budgétaires projetées w_i(T).</summary>
    public double[] PartsBudgetairesProjetees { get; set; } = [];

    /// <summary>Dépenses projetées par catégorie (MGA).</summary>
    public double[] DepensesProjetees { get; set; } = [];

    /// <summary>M3 cible.</summary>
    public double MasseMonetaireCible { get; set; }

    /// <summary>Taux de croissance journalier nécessaire.</summary>
    public double TauxCroissanceJournalier { get; set; }

    /// <summary>Trajectoire M3 simulée jour par jour (échantillonnée).</summary>
    public List<(int Jour, double M3)> TrajectoireM3 { get; set; } = [];

    /// <summary>Recommandations textuelles.</summary>
    public List<AIDSRecommandation> Recommandations { get; set; } = [];

    // ═══════════════════════════════════════════
    //  ESTIMATION & SIMULATION
    // ═══════════════════════════════════════════

    /// <summary>
    /// Estime les paramètres AIDS et projette les parts budgétaires
    /// à l'horizon donné, selon l'objectif de croissance de M3.
    /// </summary>
    public void Estimer()
    {
        int n = N;
        MasseMonetaireCible = MasseMonetaireInitiale * (1.0 + ObjectifCroissancePourcent / 100.0);
        TauxCroissanceJournalier = Math.Pow(1.0 + ObjectifCroissancePourcent / 100.0, 1.0 / HorizonJours) - 1.0;

        // ── 1. Estimation de α_i (intercept) ─────────────────────
        // α_i = w_i observé (à prix de base et dépense de base)
        Alpha = new double[n];
        Array.Copy(PartsBudgetaires, Alpha, n);

        // ── 2. Estimation de β_i (élasticité-revenu) ─────────────
        // β_i reflète comment w_i change quand la dépense totale X augmente.
        // Loi d'Engel : biens de première nécessité → β < 0 (part diminue),
        //               biens de luxe → β > 0 (part augmente).
        // Calibré sur la littérature EPM Madagascar.
        Beta = new double[n];
        Beta[0] = -0.08;   // Alimentaire informel : bien inférieur
        Beta[1] = -0.03;   // Alimentaire formel : légèrement inférieur
        Beta[2] = -0.06;   // Riz : bien de base (Engel fort)
        Beta[3] = -0.01;   // Eau : incompressible
        Beta[4] =  0.01;   // Électricité : légèrement supérieur
        Beta[5] =  0.02;   // Transport : quasi-unitaire
        Beta[6] =  0.08;   // Loisirs : bien de luxe
        Beta[7] =  0.07;   // Biens divers : supérieur

        // ── 3. Estimation de γ_ij (élasticités-prix croisées) ────
        // Matrice simplifiée : effets propres négatifs (loi de la demande),
        // substitution limitée entre alimentaire et non-alimentaire.
        Gamma = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            // Effet propre : compensé (adding-up)
            Gamma[i, i] = -0.10 * PartsBudgetaires[i];

            // Substitutions croisées symétriques simplifiées
            for (int j = i + 1; j < n; j++)
            {
                double substitution = 0.02 * PartsBudgetaires[i] * PartsBudgetaires[j];
                Gamma[i, j] = substitution;
                Gamma[j, i] = substitution;
            }
        }

        // ── 4. Projection des parts budgétaires w_i(T) ──────────
        // Formule AIDS (variation par rapport à la période de base) :
        //   w_i(T) = α_i + Σ_j γ_ij × Δln(p_j) + β_i × Δln(X/P*)
        //
        // Où Δln(X/P*) = ln(X_T/P*_T) − ln(X_0/P*_0)
        //   = variation du log de la dépense réelle entre la base et l'horizon.
        //
        // À la période de base : p_j(0) = 1.0 → ln(p_j(0)) = 0, P*_0 = 1.0
        // Donc Δln(p_j) = ln(p_j(T)) et Δln(X/P*) = ln(X_T) − ln(P*_T) − ln(X_0)
        //
        // ATTENTION : utiliser ln(X_T) en absolu (≈31 pour M3=30 Tds) au lieu de
        // la variation (≈0.09 pour +10%) rend β_i × ln(X) ≫ α_i → parts négatives.

        // Indice de Stone projeté : ln(P*_T) = Σ w_i × ln(p_i(T))
        double lnP_star_T = 0;
        for (int i = 0; i < n; i++)
            lnP_star_T += PartsBudgetaires[i] * Math.Log(IndicesPrixProjectes[i]);

        // Indice de Stone de base : ln(P*_0) = Σ w_i × ln(p_i(0)) = 0 (prix base = 1.0)
        double lnP_star_0 = 0;
        for (int i = 0; i < n; i++)
            lnP_star_0 += PartsBudgetaires[i] * Math.Log(IndicesPrix[i]);

        // Variation de la dépense réelle (log) :
        // Δln(X/P*) = [ln(X_T) − ln(P*_T)] − [ln(X_0) − ln(P*_0)]
        double deltaLnRealExpenditure =
            (Math.Log(MasseMonetaireCible) - lnP_star_T)
          - (Math.Log(MasseMonetaireInitiale) - lnP_star_0);

        PartsBudgetairesProjetees = new double[n];
        for (int i = 0; i < n; i++)
        {
            // Σ_j γ_ij × Δln(p_j) — variation des prix par rapport à la base
            double sommeGamma = 0;
            for (int j = 0; j < n; j++)
                sommeGamma += Gamma[i, j] * (Math.Log(IndicesPrixProjectes[j]) - Math.Log(IndicesPrix[j]));

            PartsBudgetairesProjetees[i] = Alpha[i]
                + sommeGamma
                + Beta[i] * deltaLnRealExpenditure;
        }

        // Normalisation pour que Σw_i = 1
        // Les contraintes théoriques (adding-up) garantissent Σw_i ≈ 1,
        // mais les γ simplifiés peuvent introduire un léger écart.
        double sommeW = PartsBudgetairesProjetees.Sum();
        if (Math.Abs(sommeW) > 1e-10)
            for (int i = 0; i < n; i++)
                PartsBudgetairesProjetees[i] /= sommeW;

        // Clamp : les parts budgétaires doivent rester dans [0, 1]
        // (une part négative signifierait qu'un ménage est payé pour consommer)
        for (int i = 0; i < n; i++)
            PartsBudgetairesProjetees[i] = Math.Max(0, PartsBudgetairesProjetees[i]);

        // Re-normaliser après le clamp
        sommeW = PartsBudgetairesProjetees.Sum();
        if (sommeW > 1e-10)
            for (int i = 0; i < n; i++)
                PartsBudgetairesProjetees[i] /= sommeW;

        // ── 5. Dépenses projetées par catégorie ──────────────────
        DepensesProjetees = new double[n];
        for (int i = 0; i < n; i++)
            DepensesProjetees[i] = PartsBudgetairesProjetees[i] * MasseMonetaireCible;

        // ── 6. Trajectoire M3 ────────────────────────────────────
        TrajectoireM3 = [];
        int pas = Math.Max(1, HorizonJours / 50);
        for (int j = 0; j <= HorizonJours; j += pas)
        {
            double m3 = MasseMonetaireInitiale * Math.Pow(1.0 + TauxCroissanceJournalier, j);
            TrajectoireM3.Add((j, m3));
        }
        if (TrajectoireM3[^1].Jour != HorizonJours)
            TrajectoireM3.Add((HorizonJours, MasseMonetaireCible));

        // ── 7. Interprétation & recommandations ──────────────────
        GenererRecommandations();

        EstEstime = true;
    }

    private void GenererRecommandations()
    {
        Recommandations = [];
        int n = N;
        double deltaX = ObjectifCroissancePourcent / 100.0;

        // Identifier les catégories dont la part augmente (biens supérieurs)
        for (int i = 0; i < n; i++)
        {
            double deltaW = PartsBudgetairesProjetees[i] - PartsBudgetaires[i];
            double deltaWPct = deltaW * 100.0;

            if (deltaW > 0.005)
            {
                Recommandations.Add(new AIDSRecommandation
                {
                    Categorie = Categories[i],
                    Icone = CategoriesIcons[i],
                    Type = ETypeRecommandation.Croissance,
                    Titre = $"{Categories[i]} : part en hausse ({deltaWPct:+0.0}%)",
                    Description = $"La catégorie « {Categories[i]} » voit sa part budgétaire augmenter de {PartsBudgetaires[i] * 100:F1}% à {PartsBudgetairesProjetees[i] * 100:F1}%. " +
                        $"C'est un bien supérieur (β = {Beta[i]:F3}). La croissance de M3 fait que les ménages consacrent plus à ce poste.",
                    ActionRecommandee = i switch
                    {
                        5 => "Investir dans les infrastructures de transport pour absorber la demande croissante.",
                        6 => "Développer l'offre touristique et les loisirs pour capter cette demande supérieure.",
                        7 => "Diversifier l'offre de biens de consommation pour répondre à la demande croissante.",
                        4 => "Augmenter la capacité de production électrique (JIRAMA) pour satisfaire la demande.",
                        _ => "Adapter l'offre pour répondre à la demande croissante dans ce secteur."
                    }
                });
            }
            else if (deltaW < -0.005)
            {
                Recommandations.Add(new AIDSRecommandation
                {
                    Categorie = Categories[i],
                    Icone = CategoriesIcons[i],
                    Type = ETypeRecommandation.Decroissance,
                    Titre = $"{Categories[i]} : part en baisse ({deltaWPct:+0.0}%)",
                    Description = $"La catégorie « {Categories[i]} » voit sa part budgétaire diminuer de {PartsBudgetaires[i] * 100:F1}% à {PartsBudgetairesProjetees[i] * 100:F1}%. " +
                        $"C'est un bien de première nécessité (β = {Beta[i]:F3}). Loi d'Engel : quand le revenu augmente, la part alimentaire diminue.",
                    ActionRecommandee = i switch
                    {
                        0 or 1 => "Même si la part relative diminue, le montant absolu augmente. Maintenir la production agricole.",
                        2 => "La part du riz diminue mais la consommation absolue reste stable. Sécuriser les stocks.",
                        3 => "L'accès à l'eau reste critique. Investir dans le réseau JIRAMA malgré la baisse relative.",
                        _ => "La baisse relative n'implique pas une baisse absolue. Maintenir la capacité de production."
                    }
                });
            }
        }

        // Recommandation sur l'inflation
        double inflationMoyenne = IndicesPrixProjectes.Zip(PartsBudgetaires, (p, w) => w * (p - 1.0)).Sum() * 100.0;
        if (inflationMoyenne > ObjectifCroissancePourcent * 0.5)
        {
            Recommandations.Add(new AIDSRecommandation
            {
                Categorie = "Politique monétaire",
                Icone = "🏦",
                Type = ETypeRecommandation.Alerte,
                Titre = $"⚠️ Inflation projetée ({inflationMoyenne:F1}%) élevée par rapport à la croissance ({ObjectifCroissancePourcent:F1}%)",
                Description = "L'inflation pondérée absorbe plus de la moitié de la croissance nominale de M3. " +
                    "La croissance réelle sera significativement inférieure à l'objectif nominal.",
                ActionRecommandee = "Contrôler l'inflation via le taux directeur et stabiliser les prix alimentaires (riz, carburant). " +
                    "Envisager des subventions ciblées pour maintenir le pouvoir d'achat réel."
            });
        }

        // Recommandation sur la balance commerciale
        double depTransport = PartsBudgetairesProjetees[5] * MasseMonetaireCible;
        double depImportImplicite = depTransport * 0.35; // ~35% du transport = carburant importé
        Recommandations.Add(new AIDSRecommandation
        {
            Categorie = "Balance commerciale",
            Icone = "📦",
            Type = ETypeRecommandation.Structurel,
            Titre = "Impact sur la balance commerciale",
            Description = $"La croissance de M3 de {ObjectifCroissancePourcent:F1}% augmentera les importations " +
                $"(énergie, équipements, biens de consommation). Sans augmentation parallèle des exports, " +
                $"la balance commerciale se dégradera, créant une pression sur M3.",
            ActionRecommandee = "Augmenter les exportations (vanille, nickel, textile/zones franches) en parallèle " +
                "de la croissance monétaire pour éviter la fuite de devises (CIF > FOB)."
        });

        // Recommandation sur le crédit
        Recommandations.Add(new AIDSRecommandation
        {
            Categorie = "Crédit bancaire",
            Icone = "💳",
            Type = ETypeRecommandation.Structurel,
            Titre = $"Taux de création de crédit nécessaire : {TauxCroissanceJournalier * 100:F4}%/jour",
            Description = $"Pour atteindre +{ObjectifCroissancePourcent:F1}% en {HorizonJours} jours, " +
                $"la masse monétaire doit croître de {TauxCroissanceJournalier * 100:F4}% par jour. " +
                $"Ce rythme doit être alimenté par la création de crédit bancaire et les entrées de devises (exports).",
            ActionRecommandee = "Calibrer le taux directeur pour encourager le crédit tout en maîtrisant l'inflation. " +
                "Objectif : CroissanceCreditJour ≈ " + $"{TauxCroissanceJournalier * 100:F4}% dans le simulateur."
        });
    }

    // ═══════════════════════════════════════════
    //  INTÉGRATION : Simulation → AIDS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Indique si les données proviennent d'une simulation réelle.
    /// </summary>
    public bool ImportéDepuisSimulation { get; set; }

    /// <summary>
    /// Extrait les parts budgétaires observées et les indices de prix
    /// à partir des snapshots d'une simulation terminée.
    /// Mapping des 8 catégories AIDS ← DailySnapshotViewModel.
    /// </summary>
    public void ChargerDepuisSimulation(SimulationResultViewModel result)
    {
        if (result.Snapshots.Count < 2) return;

        var first = result.Snapshots[0];
        var last = result.Snapshots[^1];

        // ── M3 et horizon ────────────────────────────────────────
        MasseMonetaireInitiale = first.MasseMonetaireM3;
        HorizonJours = result.JoursSimules;

        double m3Finale = last.MasseMonetaireM3;
        if (MasseMonetaireInitiale > 0)
            ObjectifCroissancePourcent = (m3Finale / MasseMonetaireInitiale - 1.0) * 100.0;

        // ── Parts budgétaires observées (moyenne sur la simulation) ──
        // Calculer la consommation totale moyenne pour chaque catégorie
        int count = result.Snapshots.Count;
        double avgAlimInformel = result.Snapshots.Average(s => s.DepensesAlimentairesInformelTotal);
        double avgAlimFormel = result.Snapshots.Average(s => s.DepensesAlimentairesFormelTotal);
        double avgRiz = result.Snapshots.Average(s => s.DepensesRizTotales);
        double avgEau = result.Snapshots.Average(s => s.RecettesEauJirama);
        double avgElec = result.Snapshots.Average(s => s.RecettesElectriciteJirama);
        double avgTransport = result.Snapshots.Average(s => s.DepensesTransportTotales);
        double avgLoisirs = result.Snapshots.Average(s => s.DepensesLoisirsTotales);
        double avgConsoTotale = result.Snapshots.Average(s => s.ConsommationTotaleMenages);

        // Les dépenses alimentaires incluent déjà le riz → éviter le double-comptage
        double avgAlimHorsRiz = Math.Max(0, avgAlimInformel + avgAlimFormel - avgRiz);
        double avgDivers = Math.Max(0, avgConsoTotale - avgAlimHorsRiz - avgRiz - avgEau - avgElec - avgTransport - avgLoisirs);

        double total = avgAlimInformel + avgAlimFormel - avgRiz + avgRiz + avgEau + avgElec + avgTransport + avgLoisirs + avgDivers;
        if (total <= 0) total = 1;

        // La part informelle/formelle du alimentaire hors-riz
        double ratioInformelAlim = (avgAlimInformel > 0 && (avgAlimInformel + avgAlimFormel) > 0)
            ? avgAlimInformel / (avgAlimInformel + avgAlimFormel)
            : 0.85;

        double alimHorsRizInformel = (avgAlimHorsRiz) * ratioInformelAlim;
        double alimHorsRizFormel = (avgAlimHorsRiz) * (1.0 - ratioInformelAlim);

        PartsBudgetaires =
        [
            alimHorsRizInformel / total,   // 0: Alimentaire informel
            alimHorsRizFormel / total,      // 1: Alimentaire formel
            avgRiz / total,                 // 2: Riz
            avgEau / total,                 // 3: Eau
            avgElec / total,                // 4: Électricité
            avgTransport / total,           // 5: Transport
            avgLoisirs / total,             // 6: Loisirs
            avgDivers / total               // 7: Biens divers
        ];

        // ── Indices de prix (début vs fin simulation) ────────────
        // Base = 1.0 pour le premier snapshot. Projeté = ratio dernier/premier.
        IndicesPrix = [1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];

        // Estimer l'inflation par catégorie à partir de l'évolution observée
        static double SafeRatio(double fin, double debut) =>
            debut > 0 ? Math.Max(0.5, Math.Min(3.0, fin / debut)) : 1.0;

        // Pour les catégories où on dispose de données granulaires
        IndicesPrixProjectes =
        [
            SafeRatio(last.DepensesAlimentairesInformelTotal, first.DepensesAlimentairesInformelTotal),
            SafeRatio(last.DepensesAlimentairesFormelTotal, first.DepensesAlimentairesFormelTotal),
            SafeRatio(last.DepensesRizTotales, first.DepensesRizTotales),
            SafeRatio(last.RecettesEauJirama, first.RecettesEauJirama),
            SafeRatio(last.PrixElectriciteArKWh, first.PrixElectriciteArKWh),
            SafeRatio(last.DepensesTransportTotales, first.DepensesTransportTotales),
            SafeRatio(last.DepensesLoisirsTotales, first.DepensesLoisirsTotales),
            SafeRatio(last.ConsommationTotaleMenages, first.ConsommationTotaleMenages)
        ];

        ImportéDepuisSimulation = true;
    }

    // ═══════════════════════════════════════════
    //  INTÉGRATION : AIDS → Simulation
    // ═══════════════════════════════════════════

    /// <summary>
    /// Génère un ScenarioConfigViewModel calibré sur les projections AIDS.
    /// Les paramètres du simulateur sont ajustés pour que la simulation
    /// converge vers les parts budgétaires et la M3 cible projetées par le AIDS.
    /// </summary>
    public ScenarioConfigViewModel GenererScenarioConfig()
    {
        var config = new ScenarioConfigViewModel
        {
            Name = $"🧮 Scénario AIDS — +{ObjectifCroissancePourcent:F1}% en {HorizonJours}j",
            Description = $"Généré par le modèle AIDS. Objectif : M3 de {FormatMds(MasseMonetaireInitiale)} → {FormatMds(MasseMonetaireCible)} " +
                          $"en {HorizonJours} jours (+{ObjectifCroissancePourcent:F1}%).",
            DureeJours = HorizonJours,

            // Taux de création de crédit calibré sur l'objectif de croissance M3
            CroissanceCreditJour = TauxCroissanceJournalier,

            // Inflation projetée (moyenne pondérée des indices de prix AIDS)
            TauxInflation = IndicesPrixProjectes.Zip(PartsBudgetaires, (p, w) => w * (p - 1.0)).Sum(),

            // Les parts budgétaires AIDS influencent la répartition consommation
            PartRevenuAlimentaire = PartsBudgetairesProjetees.Length >= 3
                ? PartsBudgetairesProjetees[0] + PartsBudgetairesProjetees[1] + PartsBudgetairesProjetees[2]
                : 0.40,

            // Le taux d'épargne est le complémentaire de la propension à consommer
            PropensionConsommation = Math.Min(0.95, Math.Max(0.50,
                1.0 - (1.0 - PartsBudgetaires.Sum()) * 0.5)),

            // Transport : si la part transport projetée augmente, le carburant peut monter
            PrixCarburantLitre = IndicesPrixProjectes.Length > 5
                ? 5_500 * IndicesPrixProjectes[5]
                : 5_500,
        };

        return config;
    }

    private static string FormatMds(double montant)
    {
        if (Math.Abs(montant) >= 1_000_000_000_000)
            return $"{montant / 1_000_000_000_000:F1} Tds MGA";
        if (Math.Abs(montant) >= 1_000_000_000)
            return $"{montant / 1_000_000_000:F2} Mds MGA";
        return $"{montant:F0} MGA";
    }
}

public class AIDSRecommandation
{
    public string Categorie { get; set; } = "";
    public string Icone { get; set; } = "";
    public ETypeRecommandation Type { get; set; }
    public string Titre { get; set; } = "";
    public string Description { get; set; } = "";
    public string ActionRecommandee { get; set; } = "";
}

public enum ETypeRecommandation
{
    Croissance,
    Decroissance,
    Alerte,
    Structurel
}
