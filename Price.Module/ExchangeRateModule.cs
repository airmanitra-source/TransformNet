namespace Price.Module;

/// <summary>
/// Implémentation du module de taux de change dynamique MGA/USD.
///
/// Modèle structurel combinant :
///   1. Pression de la balance commerciale (déficit → dépréciation)
///   2. Parité de pouvoir d'achat relative (différentiel d'inflation)
///   3. Flux de capitaux (remittances, aide → appréciation)
///   4. Interventions BCM (vente de réserves pour stabiliser)
///   5. Tendance structurelle de dépréciation (~5%/an)
///
/// Calibrage spécifique à Madagascar :
///   - Régime de flottement géré par la BCM
///   - Réserves ~2.5 Mds USD (~5 mois d'imports)
///   - Dépréciation tendancielle ~5-7%/an depuis 2015
///   - Marché interbancaire des devises (MID) peu profond
///   - Forte saisonnalité (vanilla season = afflux devises jul-oct)
///
/// Le taux calculé est utilisé pour :
///   - Convertir les imports CIF (facturés en devises) en MGA
///   - Convertir les exports FOB en MGA
///   - Renchérir/alléger les remittances
///   - Alimenter la composante cost-push de l'inflation
/// </summary>
public class ExchangeRateModule : IExchangeRateModule
{
    // ═══════════════════════════════════════════
    //  ÉTAT INTERNE
    // ═══════════════════════════════════════════

    /// <summary>Taux MGA/USD courant.</summary>
    private double _tauxCourant;

    /// <summary>Taux MGA/USD du jour précédent (pour la variation).</summary>
    private double _tauxPrecedent;

    /// <summary>Réserves de change BCM en USD.</summary>
    private double _reservesBCMUSD;

    /// <summary>Moyenne mobile exponentielle du taux (lissage).</summary>
    private double _tauxLisse;

    /// <summary>Jour courant.</summary>
    private int _jourCompteur;

    /// <summary>Importations CIF journalières moyennes (pour calculer les mois de réserves).</summary>
    private double _importsMoyenJourUSD;

    /// <summary>Coefficient de lissage EMA (0.03 ≈ fenêtre de ~33 jours).</summary>
    private const double AlphaLissage = 0.03;

    /// <summary>Plancher de variation journalière (-3%). Circuit breaker BCM.</summary>
    private const double PlancherVariationJour = -0.03;

    /// <summary>Plafond de variation journalière (+3%). Circuit breaker BCM.</summary>
    private const double PlafondVariationJour = 0.03;

    /// <summary>Plancher du taux MGA/USD (ne peut pas s'apprécier en dessous de 2000).</summary>
    private const double PlancherTaux = 2_000;

    /// <summary>Plafond du taux MGA/USD (hyperinflation = 30 000).</summary>
    private const double PlafondTaux = 30_000;

    public void Initialiser(double tauxInitialMGAParUSD, double reservesBCMUSD)
    {
        _tauxCourant = tauxInitialMGAParUSD;
        _tauxPrecedent = tauxInitialMGAParUSD;
        _tauxLisse = tauxInitialMGAParUSD;
        _reservesBCMUSD = reservesBCMUSD;
        _jourCompteur = 0;
        _importsMoyenJourUSD = 0;
    }

    public ExchangeRateResult CalculerTauxChange(ExchangeRateContext ctx)
    {
        _jourCompteur++;
        var result = new ExchangeRateResult();

        // ═══════════════════════════════════════════
        //  1. CONVERTIR LES FLUX MGA EN USD (au taux courant)
        // ═══════════════════════════════════════════
        //
        // Les imports/exports sont en MGA dans la simulation.
        // On les convertit en USD pour calculer le solde de devises.
        //
        double exportsUSD = _tauxCourant > 0 ? ctx.ExportationsFOBJour / _tauxCourant : 0;
        double importsUSD = _tauxCourant > 0 ? ctx.ImportationsCIFJour / _tauxCourant : 0;
        double remittancesUSD = _tauxCourant > 0 ? ctx.RemittancesJour / _tauxCourant : 0;
        double aideUSD = _tauxCourant > 0 ? ctx.AideInternationaleJour / _tauxCourant : 0;

        // Mise à jour de la moyenne mobile des imports (pour les réserves en mois)
        _importsMoyenJourUSD = _jourCompteur == 1
            ? importsUSD
            : 0.95 * _importsMoyenJourUSD + 0.05 * importsUSD;

        // ═══════════════════════════════════════════
        //  2. SOLDE NET DE DEVISES
        // ═══════════════════════════════════════════
        //
        // Offre de devises : exports + remittances + aide
        // Demande de devises : imports
        //
        double offreDevises = exportsUSD + remittancesUSD + aideUSD;
        double demandeDevises = importsUSD;
        double soldeNetUSD = offreDevises - demandeDevises;

        result.SoldeDevisesJourUSD = soldeNetUSD;

        // ═══════════════════════════════════════════
        //  3. PRESSION DE LA BALANCE COMMERCIALE
        // ═══════════════════════════════════════════
        //
        // Si solde < 0 (déficit) → pression dépréciative
        // Normalisé par le flux total pour avoir un ratio -1 à +1
        //
        double fluxTotal = offreDevises + demandeDevises;
        double pressionBalance = 0;
        if (fluxTotal > 0)
        {
            // Ratio : -1 (déficit total) à +1 (excédent total)
            pressionBalance = -soldeNetUSD / fluxTotal;
            // Négatif solde → positif pression (dépréciation)
        }
        result.PressionBalanceCommerciale = pressionBalance;

        // ═══════════════════════════════════════════
        //  4. PRESSION PPA (Parité de Pouvoir d'Achat)
        // ═══════════════════════════════════════════
        //
        // Δe/e ≈ π_MGA - π_USD (par an)
        // Si inflation MGA (8%) > inflation USD (3%) → MGA se déprécie de ~5%/an
        //
        double differentielInflation = ctx.InflationDomestique - ctx.InflationEtrangere;
        double pressionPPAJour = differentielInflation / 365.0;
        result.PressionPPA = differentielInflation;

        // ═══════════════════════════════════════════
        //  5. TENDANCE STRUCTURELLE
        // ═══════════════════════════════════════════
        //
        // Dépréciation tendancielle due aux facteurs structurels :
        // faible productivité, déficit courant chronique, pression démographique
        //
        double tendanceJour = ctx.DepreciationStructurelleAnnuelle / 365.0;

        // ═══════════════════════════════════════════
        //  6. AGRÉGATION DE LA PRESSION
        // ═══════════════════════════════════════════
        //
        // La variation journalière combine :
        //   - Pression balance commerciale (réactif, court terme)
        //   - Pression PPA (moyen terme)
        //   - Tendance structurelle (long terme)
        //
        // L'élasticité balance commerciale contrôle la transmission du déficit
        //
        double variationBrute =
            ctx.ElasticiteBalanceCommerciale * pressionBalance / 365.0   // Annualisé → journalier
            + ctx.PoidsPPA * pressionPPAJour
            + (1.0 - ctx.ElasticiteBalanceCommerciale - ctx.PoidsPPA) * tendanceJour;

        // ═══════════════════════════════════════════
        //  7. INTERVENTION BCM
        // ═══════════════════════════════════════════
        //
        // Si le MGA se déprécie, la BCM vend des devises pour freiner.
        // La capacité d'intervention dépend du stock de réserves.
        // En dessous du seuil minimal (3 mois d'imports), la BCM cesse d'intervenir.
        //
        double reservesMoisImports = _importsMoyenJourUSD > 0
            ? _reservesBCMUSD / (_importsMoyenJourUSD * 30.0)
            : 12.0; // Si pas d'imports, réserves considérées suffisantes

        double interventionUSD = 0;
        double capaciteIntervention = 0;

        if (variationBrute > 0 && reservesMoisImports > ctx.ReservesMinimalesMoisImports)
        {
            // La BCM intervient proportionnellement à :
            //   - L'intensité de sa politique (IntensiteInterventionBCM)
            //   - Sa marge de manœuvre (réserves au-dessus du seuil)
            double margeReserves = Math.Min(1.0,
                (reservesMoisImports - ctx.ReservesMinimalesMoisImports) / 5.0);
            capaciteIntervention = ctx.IntensiteInterventionBCM * margeReserves;

            // Réduire la pression par l'intervention
            double variationAvantIntervention = variationBrute;
            variationBrute *= (1.0 - capaciteIntervention);

            // La BCM vend des devises (réduit ses réserves)
            // Montant = fraction du déficit quotidien compensé
            interventionUSD = Math.Abs(soldeNetUSD) * capaciteIntervention;
            if (soldeNetUSD < 0) // Déficit : la BCM vend des USD
            {
                _reservesBCMUSD -= interventionUSD;
                _reservesBCMUSD = Math.Max(0, _reservesBCMUSD);
            }
        }
        else if (variationBrute < 0)
        {
            // Appréciation : la BCM achète des devises (reconstitue ses réserves)
            double achatsUSD = Math.Abs(soldeNetUSD) * 0.3; // 30% de l'excédent
            _reservesBCMUSD += achatsUSD;
        }

        result.InterventionBCMUSD = interventionUSD;
        result.ReservesBCMUSD = _reservesBCMUSD;
        result.ReservesMoisImports = reservesMoisImports;

        // ═══════════════════════════════════════════
        //  8. BORNER ET APPLIQUER LA VARIATION
        // ═══════════════════════════════════════════

        variationBrute = Math.Clamp(variationBrute, PlancherVariationJour, PlafondVariationJour);

        double nouveauTaux = _tauxCourant * (1.0 + variationBrute);

        // ═══════════════════════════════════════════
        //  9. LISSAGE EMA
        // ═══════════════════════════════════════════
        //
        // Le taux de change ne saute pas brutalement au jour le jour.
        // Le MID (marché interbancaire des devises) lisse les mouvements.
        //
        double alphaEffectif = _jourCompteur < 30 ? 0.15 : AlphaLissage;
        _tauxLisse = alphaEffectif * nouveauTaux + (1.0 - alphaEffectif) * _tauxLisse;
        _tauxLisse = Math.Clamp(_tauxLisse, PlancherTaux, PlafondTaux);

        // ═══════════════════════════════════════════
        //  10. METTRE À JOUR L'ÉTAT
        // ═══════════════════════════════════════════

        _tauxPrecedent = _tauxCourant;
        _tauxCourant = _tauxLisse;

        double variationEffective = _tauxPrecedent > 0
            ? (_tauxCourant - _tauxPrecedent) / _tauxPrecedent
            : 0;

        result.TauxMGAParUSD = _tauxCourant;
        result.VariationJournaliere = variationEffective;
        result.DepreciationAnnualisee = variationEffective * 365.0;
        result.IndicePression = pressionBalance * ctx.ElasticiteBalanceCommerciale
                              + differentielInflation * ctx.PoidsPPA;

        return result;
    }
}
