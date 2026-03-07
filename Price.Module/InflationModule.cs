namespace Price.Module;

/// <summary>
/// Implémentation du module d'inflation endogène.
///
/// Modèle hybride combinant :
///   1. Courbe de Phillips néo-keynésienne (demand-pull)
///   2. Chocs d'offre / cost-push (carburant + importations)
///   3. Théorie quantitative de la monnaie (composante monétaire)
///   4. Anticipations adaptatives (inertie + ancrage BCM)
///
/// Calibrage spécifique à Madagascar :
///   - Économie à forte dépendance des importations (~40% du PIB)
///   - Transmission rapide des chocs de carburant vers les prix intérieurs
///   - Secteur informel (85%) où les prix s'ajustent rapidement
///   - Sous-emploi structurel (NAIRU élevé ~15-20%)
///   - Banque centrale avec ancrage implicite (pas de ciblage strict)
///
/// L'inflation calculée est annualisée puis utilisée par la simulation
/// pour remplacer le paramètre fixe TauxInflation dans Government.
/// </summary>
public class InflationModule : IInflationModule
{
    // ═══════════════════════════════════════════
    //  ÉTAT INTERNE (mémoire entre les jours)
    // ═══════════════════════════════════════════

    /// <summary>Anticipations d'inflation courantes (annualisées).</summary>
    private double _anticipationsInflation;

    /// <summary>PIB potentiel journalier (MGA). Croît selon le taux tendanciel.</summary>
    private double _pibPotentielJour;

    /// <summary>M3 du jour précédent (pour calculer la croissance).</summary>
    private double _m3Precedent;

    /// <summary>PIB du jour précédent (pour calculer la croissance).</summary>
    private double _pibPrecedent;

    /// <summary>Taux d'inflation annuel du jour précédent (pour le lissage).</summary>
    private double _inflationPrecedente;

    /// <summary>Jour courant (pour le calcul de la croissance du PIB potentiel).</summary>
    private int _jourCompteur;

    /// <summary>
    /// Moyenne mobile exponentielle de l'inflation sur 30 jours.
    /// Évite les oscillations violentes dues aux bruits journaliers.
    /// </summary>
    private double _inflationLissee;

    /// <summary>Coefficient de lissage EMA (0.05 ≈ fenêtre de 20 jours).</summary>
    private const double AlphaLissage = 0.05;

    /// <summary>
    /// Plancher d'inflation annuelle (-2%). Madagascar n'a jamais connu de déflation significative.
    /// </summary>
    private const double PlancherInflation = -0.02;

    /// <summary>
    /// Plafond d'inflation annuelle (80%). Au-delà = hyperinflation, hors scope du modèle.
    /// </summary>
    private const double PlafondInflation = 0.80;

    public void Initialiser(double tauxInflationInitial, double pibPotentielJour, double m3Initial)
    {
        _anticipationsInflation = tauxInflationInitial;
        _pibPotentielJour = pibPotentielJour;
        _m3Precedent = m3Initial;
        _pibPrecedent = pibPotentielJour;
        _inflationPrecedente = tauxInflationInitial;
        _inflationLissee = tauxInflationInitial;
        _jourCompteur = 0;
    }

    public InflationResult CalculerInflationJournaliere(InflationContext ctx)
    {
        _jourCompteur++;
        var result = new InflationResult();

        // ═══════════════════════════════════════════
        //  1. DEMAND-PULL (courbe de Phillips)
        // ═══════════════════════════════════════════
        //
        // π_dp = -β × (u - u*)
        //   u = taux de chômage = 1 - TauxEmploi
        //   u* = NAIRU
        //   β = sensibilité Phillips
        //
        // Si u < u* (chômage bas) → inflation (+)
        // Si u > u* (sous-emploi) → désinflation (-)
        //
        double tauxChomage = 1.0 - ctx.TauxEmploi;
        double ecartChomage = tauxChomage - ctx.NAIRU;
        double composanteDemandPull = -ctx.CoefficientPhillips * ecartChomage;

        // Output gap additionnel (renforce Phillips si PIB dépasse le potentiel)
        double outputGap = 0;
        if (_pibPotentielJour > 0 && ctx.PIBJourEffectif > 0)
        {
            outputGap = (ctx.PIBJourEffectif - _pibPotentielJour) / _pibPotentielJour;
            // Ajouter une petite contribution de l'output gap au demand-pull
            // (0.5× pour ne pas double-compter avec Phillips)
            composanteDemandPull += 0.5 * ctx.CoefficientPhillips * outputGap;
        }

        result.ComposanteDemandPull = composanteDemandPull;
        result.EcartChomage = ecartChomage;
        result.OutputGap = outputGap;

        // ═══════════════════════════════════════════
        //  2. COST-PUSH (chocs d'offre)
        // ═══════════════════════════════════════════
        //
        // π_cp = δ × Δcarburant/carburant_ref + ε × Δimport/import_ref + ζ × Δe/e + η × Δsalaires/salaires_ref
        //
        // Quatre canaux de transmission des chocs de coûts :
        //   a) Carburant → transport, Jirama thermique, intrants agricoles
        //   b) Importations CIF → prix des biens importés (40% du PIB)
        //   c) Taux de change → renchérissement de TOUS les imports (pass-through)
        //   d) Salaires → coûts de production des entreprises (spirale prix-salaires)
        //
        // Le canal du taux de change est le plus important à Madagascar :
        //   - 40% du PIB est importé (carburant, riz, biens manufacturés)
        //   - Dépréciation MGA → hausse mécanique du coût CIF en MGA
        //   - Pass-through empirique ~25-35% (BCM, études FMI)
        //
        double chocCarburant = 0;
        if (ctx.PrixCarburantReference > 0)
        {
            chocCarburant = (ctx.PrixCarburantCourant - ctx.PrixCarburantReference) / ctx.PrixCarburantReference;
        }

        double chocImport = 0;
        if (ctx.ImportationsCIFReference > 0 && ctx.ImportationsCIFJour > 0)
        {
            // Hausse du coût des importations = inflation importée
            chocImport = (ctx.ImportationsCIFJour - ctx.ImportationsCIFReference) / ctx.ImportationsCIFReference;
        }

        // Canal du taux de change (exchange rate pass-through)
        // Dépréciation journalière × 365 = dépréciation annualisée
        // Seule la dépréciation (positif) génère de l'inflation (asymétrique)
        double chocChange = Math.Max(0, ctx.VariationChangeJournaliere * 365.0);

        // Canal salarial (wage-price pass-through / spirale prix-salaires)
        // Si les salaires augmentent, les coûts de production montent → entreprises
        // répercutent sur les prix de vente → inflation cost-push.
        // Essentiel pour simuler l'impact d'une hausse du SMIG.
        double chocSalaires = Math.Max(0, ctx.VariationSalaireMoyen);

        double composanteCostPush = ctx.ElasticiteCarburantInflation * chocCarburant
                                  + ctx.ElasticiteImportInflation * Math.Max(0, chocImport)
                                  + ctx.ElasticiteChangeInflation * chocChange
                                  + ctx.ElasticiteSalairesInflation * chocSalaires;

        result.ComposanteCostPush = composanteCostPush;

        // ═══════════════════════════════════════════
        //  3. MONÉTAIRE (théorie quantitative)
        // ═══════════════════════════════════════════
        //
        // MV = PY  →  ΔP/P ≈ ΔM/M - ΔY/Y  (en supposant V constant)
        // π_m = λ × (ΔM3/M3 - ΔPIB/PIB)
        //
        // Si la masse monétaire croît plus vite que le PIB réel,
        // l'excédent se traduit en inflation.
        //
        double croissanceM3Jour = 0;
        if (_m3Precedent > 0 && ctx.MasseMonetaireM3 > 0)
        {
            croissanceM3Jour = (ctx.MasseMonetaireM3 - _m3Precedent) / _m3Precedent;
        }

        double croissancePIBJour = 0;
        if (_pibPrecedent > 0 && ctx.PIBJourEffectif > 0)
        {
            croissancePIBJour = (ctx.PIBJourEffectif - _pibPrecedent) / _pibPrecedent;
        }

        // Annualiser pour la comparaison (×365)
        double croissanceM3Annualisee = croissanceM3Jour * 365.0;
        double croissancePIBAnnualisee = croissancePIBJour * 365.0;

        // Borner les croissances pour éviter les valeurs aberrantes journalières
        croissanceM3Annualisee = Math.Clamp(croissanceM3Annualisee, -0.50, 1.00);
        croissancePIBAnnualisee = Math.Clamp(croissancePIBAnnualisee, -0.50, 0.50);

        double excessMonetaire = croissanceM3Annualisee - croissancePIBAnnualisee;
        double composanteMonetaire = ctx.CoefficientMonetaire * Math.Max(0, excessMonetaire);
        // On ne prend que l'excès positif : la contraction monétaire est un processus plus lent

        result.ComposanteMonetaire = composanteMonetaire;
        result.CroissanceM3 = croissanceM3Annualisee;
        result.CroissancePIB = croissancePIBAnnualisee;

        // ═══════════════════════════════════════════
        //  4. ANTICIPATIONS ADAPTATIVES
        // ═══════════════════════════════════════════
        //
        // π_e(t) = α × π(t-1) + (1-α) × π_ancrage
        //
        // Les agents économiques forment leurs anticipations à partir :
        //   - De l'inflation récente (poids α) : composante adaptative
        //   - De la cible BCM / tendance historique (poids 1-α) : ancrage
        //
        // α élevé = inertie forte (spirale prix-salaires)
        // α faible = crédibilité de la banque centrale (désancrage difficile)
        //
        double anticipations = ctx.VitesseAdaptationAnticipations * _inflationPrecedente
                             + (1.0 - ctx.VitesseAdaptationAnticipations) * ctx.InflationAncrage;

        // Borner les anticipations
        anticipations = Math.Clamp(anticipations, PlancherInflation, PlafondInflation);
        _anticipationsInflation = anticipations;

        result.ComposanteAnticipations = anticipations;
        result.AnticipationsInflation = anticipations;

        // ═══════════════════════════════════════════
        //  5. AGRÉGATION PONDÉRÉE
        // ═══════════════════════════════════════════
        //
        // π(t) = w_e × π_e + w_dp × π_dp + w_cp × π_cp + w_m × π_m
        //
        // Les poids reflètent la structure économique de Madagascar :
        //   - Forte inertie (anticipations ≈ 40%)
        //   - Chocs d'offre importants (cost-push ≈ 25%)
        //   - Demand-pull modéré (Phillips ≈ 20%)
        //   - Canal monétaire (≈ 15%)
        //
        double inflationBrute = ctx.PoidsAnticipations * anticipations
                              + ctx.PoidsDemandPull * composanteDemandPull
                              + ctx.PoidsCostPush * composanteCostPush
                              + ctx.PoidsMonetaire * composanteMonetaire;

        // Borner
        inflationBrute = Math.Clamp(inflationBrute, PlancherInflation, PlafondInflation);

        // ═══════════════════════════════════════════
        //  6. LISSAGE EMA (éviter les sauts brutaux)
        // ═══════════════════════════════════════════
        //
        // Les premiers jours, utiliser un alpha plus élevé pour converger vite.
        // Ensuite, lissage doux pour stabilité.
        //
        double alphaEffectif = _jourCompteur < 30
            ? 0.20  // convergence rapide les 30 premiers jours
            : AlphaLissage;

        _inflationLissee = alphaEffectif * inflationBrute + (1.0 - alphaEffectif) * _inflationLissee;
        _inflationLissee = Math.Clamp(_inflationLissee, PlancherInflation, PlafondInflation);

        result.TauxInflationAnnuel = _inflationLissee;
        result.TauxInflationJournalier = _inflationLissee / 365.0;

        // ═══════════════════════════════════════════
        //  7. MISE À JOUR ÉTAT INTERNE
        // ═══════════════════════════════════════════

        _inflationPrecedente = _inflationLissee;
        _m3Precedent = ctx.MasseMonetaireM3 > 0 ? ctx.MasseMonetaireM3 : _m3Precedent;
        _pibPrecedent = ctx.PIBJourEffectif > 0 ? ctx.PIBJourEffectif : _pibPrecedent;

        // Croissance du PIB potentiel (tendanciel ~4.5%/an)
        double croissancePotentielJour = ctx.CroissancePIBPotentielAnnuel / 365.0;
        _pibPotentielJour *= (1.0 + croissancePotentielJour);

        return result;
    }

    /// <inheritdoc/>
    public TaylorRuleResult CalculerTauxDirecteurTaylor(
        double inflationCourante,
        double inflationCible,
        double outputGap,
        double tauxDirecteurPrecedent,
        double tauxReelNeutre = 0.02,
        double coefficientInflation = 0.50,
        double coefficientOutputGap = 0.50,
        double vitesseLissage = 0.05)
    {
        var result = new TaylorRuleResult();

        // ═══════════════════════════════════════════
        //  RÈGLE DE TAYLOR ADAPTÉE À MADAGASCAR
        // ═══════════════════════════════════════════
        //
        //  i_Taylor = r* + π + α×(π - π*) + β×output_gap
        //
        //  Composantes :
        //    r* = taux réel neutre (rendement d'équilibre sans inflation)
        //    π  = inflation courante (la BCM réagit à l'inflation observée)
        //    α×(π - π*) = réaction à l'écart d'inflation
        //       > 0 si inflation au-dessus de la cible → resserrement
        //       < 0 si inflation en dessous → assouplissement
        //    β×output_gap = réaction à l'activité économique
        //       > 0 si surchauffe → resserrement
        //       < 0 si récession → assouplissement
        //
        //  Calibrage BCM Madagascar :
        //    La BCM ne pratique pas de ciblage d'inflation formel.
        //    Le taux directeur sert surtout de signal pour le MID
        //    (marché interbancaire de devises) et les taux créditeurs.
        //    Historiquement très stable : 9.0-9.5% sur 2018-2024.
        //    → Fort lissage nécessaire (vitesse ~0.03-0.05).
        //

        result.ComposanteTauxReelNeutre = tauxReelNeutre;
        result.ComposanteInflation = inflationCourante;

        double ecartInflation = inflationCourante - inflationCible;
        result.ComposanteEcartInflation = coefficientInflation * ecartInflation;

        // Borner l'output gap pour éviter des réactions extrêmes
        double outputGapBorne = Math.Clamp(outputGap, -0.20, 0.20);
        result.ComposanteOutputGap = coefficientOutputGap * outputGapBorne;

        // Taux Taylor brut
        double tauxTaylor = tauxReelNeutre
                          + inflationCourante
                          + coefficientInflation * ecartInflation
                          + coefficientOutputGap * outputGapBorne;

        // Borner le taux Taylor
        // Plancher : le taux nominal ne descend pas sous 0% (Zero Lower Bound)
        //            En pratique, la BCM ne descend jamais sous ~7%
        // Plafond  : historiquement, jamais au-dessus de 25% à Madagascar
        const double plancherTaux = 0.0;
        const double plafondTaux = 0.25;
        tauxTaylor = Math.Clamp(tauxTaylor, plancherTaux, plafondTaux);
        result.TauxDirecteurTaylor = tauxTaylor;

        // ═══════════════════════════════════════════
        //  LISSAGE (inertie institutionnelle BCM)
        // ═══════════════════════════════════════════
        //
        //  i_effectif(t) = (1 - ρ) × i(t-1) + ρ × i_Taylor
        //
        //  ρ = vitesseLissage (~0.03-0.05)
        //  → Avec ρ=0.05, il faut ~20 jours pour parcourir 63% de l'écart
        //  → ~60 jours (~2 mois) pour convergence quasi-complète
        //
        //  Ceci reflète que la BCM ajuste son taux
        //  lors de réunions trimestrielles, pas quotidiennement.
        //
        double ecartVsPrecedent = tauxTaylor - tauxDirecteurPrecedent;
        result.EcartTaylorVsPrecedent = ecartVsPrecedent;

        // Limiter la variation journalière à ±2 bps/jour (~50 bps/mois max)
        // C'est cohérent avec le comportement historique de la BCM
        const double variationMaxJour = 0.0002; // 2 bps
        double variationSouhaitee = vitesseLissage * ecartVsPrecedent;
        double variationEffective = Math.Clamp(variationSouhaitee, -variationMaxJour, variationMaxJour);

        double tauxEffectif = tauxDirecteurPrecedent + variationEffective;
        tauxEffectif = Math.Clamp(tauxEffectif, plancherTaux, plafondTaux);

        result.TauxDirecteurEffectif = tauxEffectif;
        result.VariationEffective = variationEffective;

        return result;
    }
}
