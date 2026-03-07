using Simulation.Module.Config;

namespace Simulation.Module.Models;

/// <summary>
/// Contrat du module de validation macro automatique.
/// Compare les résultats simulés aux données de référence réelles
/// et produit un rapport de diagnostic quantifié.
/// </summary>
public interface IMacroValidationModule
{
    /// <summary>
    /// Valide les résultats d'une simulation en les comparant aux données de référence.
    /// Appelé automatiquement à la fin de la simulation ou à la demande.
    /// </summary>
    /// <param name="result">Résultats de la simulation (snapshots journaliers).</param>
    /// <param name="config">Configuration du scénario (pour le facteur d'échelle).</param>
    /// <param name="referenceData">Données macro de référence.</param>
    /// <returns>Rapport de validation complet avec scoring.</returns>
    MacroValidationReport Valider(SimulationResult result, ScenarioConfig config, MacroReferenceData referenceData);
}

/// <summary>
/// Moteur de validation macro automatique.
///
/// Compare 15+ indicateurs simulés aux données réelles de Madagascar
/// et produit un rapport détaillé avec :
///   - Score global (0-100)
///   - Écarts par indicateur avec diagnostic
///   - Alertes sur les incohérences majeures
///   - Recommandations d'ajustement des paramètres
///
/// Le moteur annualise automatiquement les résultats si la simulation
/// dure moins de 365 jours.
///
/// Système de scoring :
///   Score = 100 × (1 - min(1, |écart| / seuil_max))
///   seuil_max = 50% (au-delà, score = 0)
///
/// Score global = moyenne pondérée :
///   PIB (poids 3), Commerce ext. (2), Fiscal (2), Monétaire (1.5),
///   Inégalités (1), Énergie (0.5).
/// </summary>
public class MacroValidationModule : IMacroValidationModule
{
    /// <summary>Seuil max d'écart pour le scoring (50%). Au-delà, score = 0.</summary>
    private const double SeuilMaxPourcent = 50.0;

    public MacroValidationReport Valider(SimulationResult result, ScenarioConfig config, MacroReferenceData referenceData)
    {
        var rapport = new MacroValidationReport
        {
            JoursSimules = result.JoursSimules,
            SourceReference = referenceData.Source,
            AnneeReference = referenceData.Annee
        };

        if (result.Snapshots.Count == 0)
        {
            rapport.Verdict = "Aucune donnée simulée — validation impossible.";
            rapport.ScoreGlobal = 0;
            return rapport;
        }

        // ═══════════════════════════════════════════
        //  FACTEURS DE MISE À L'ÉCHELLE
        // ═══════════════════════════════════════════
        //
        // La simulation utilise un échantillon (ex: 100 000 ménages sur 6 000 000).
        // On annualise et on met à l'échelle nationale pour comparer aux données réelles.
        //
        double facteurEchelle = (double)referenceData.NombreMenages / config.NombreMenages;
        double facteurAnnualisation = result.JoursSimules > 0 ? 365.0 / result.JoursSimules : 1.0;

        var dernier = result.Snapshots[^1];
        int n = result.Snapshots.Count;

        // Cumuler les flux journaliers sur toute la simulation
        double pibJourMoyen = result.Snapshots.Average(s => s.PIBProxy);
        double consommationJourMoyenne = result.Snapshots.Average(s => s.ConsommationTotaleMenages);
        double recettesFiscalesJourMoyennes = result.Snapshots.Average(s => s.RecettesFiscalesTotales);
        double exportFOBJourMoyen = result.Snapshots.Average(s => s.ExportationsFOB);
        double importCIFJourMoyen = result.Snapshots.Average(s => s.ImportationsCIF);
        double depensesPubliquesJourMoyennes = result.Snapshots.Average(s => s.DepensesPubliques);

        // Annualiser
        double pibAnnualise = pibJourMoyen * 365.0 * facteurEchelle;
        double exportAnnualise = exportFOBJourMoyen * 365.0 * facteurEchelle;
        double importAnnualise = importCIFJourMoyen * 365.0 * facteurEchelle;
        double balanceCommerciale = exportAnnualise - importAnnualise;
        double recettesFiscalesAnnualisees = recettesFiscalesJourMoyennes * 365.0 * facteurEchelle;
        double depensesPubliquesAnnualisees = depensesPubliquesJourMoyennes * 365.0 * facteurEchelle;
        double m3Actuel = dernier.MasseMonetaireM3 * facteurEchelle;
        double dettePublique = dernier.DettePublique * facteurEchelle;

        // ═══════════════════════════════════════════
        //  1. PRODUCTION
        // ═══════════════════════════════════════════

        rapport.Indicateurs.Add(CreerIndicateur(
            "PIB nominal annualisé", "Production",
            pibAnnualise, referenceData.PIBNominalAnnuel,
            "MGA", poids: 3.0,
            recommandation: "Ajuster ProductiviteParEmploye ou NombreEntreprises"));

        double pibParHabitantSimule = pibAnnualise / referenceData.Population;
        rapport.Indicateurs.Add(CreerIndicateur(
            "PIB par habitant", "Production",
            pibParHabitantSimule, referenceData.PIBParHabitant,
            "MGA/hab", poids: 1.5));

        // ═══════════════════════════════════════════
        //  2. COMMERCE EXTÉRIEUR
        // ═══════════════════════════════════════════

        rapport.Indicateurs.Add(CreerIndicateur(
            "Exportations FOB annualisées", "Commerce",
            exportAnnualise, referenceData.ExportationsFOBAnnuelles,
            "MGA", poids: 2.0,
            recommandation: "Ajuster FOBCalibresJour ou productivité exportateurs"));

        rapport.Indicateurs.Add(CreerIndicateur(
            "Importations CIF annualisées", "Commerce",
            importAnnualise, referenceData.ImportationsCIFAnnuelles,
            "MGA", poids: 2.0,
            recommandation: "Ajuster CIFCalibresJour ou nombre importateurs"));

        rapport.Indicateurs.Add(CreerIndicateur(
            "Balance commerciale", "Commerce",
            balanceCommerciale, referenceData.BalanceCommercialeAnnuelle,
            "MGA", poids: 1.5));

        // ═══════════════════════════════════════════
        //  3. FINANCES PUBLIQUES
        // ═══════════════════════════════════════════

        rapport.Indicateurs.Add(CreerIndicateur(
            "Recettes fiscales annualisées", "Fiscal",
            recettesFiscalesAnnualisees, referenceData.RecettesFiscalesAnnuelles,
            "MGA", poids: 2.0,
            recommandation: "Ajuster TauxIS, TauxTVA ou PartSecteurInformel"));

        double pressionFiscaleSimulee = pibAnnualise > 0 ? recettesFiscalesAnnualisees / pibAnnualise : 0;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Pression fiscale (recettes/PIB)", "Fiscal",
            pressionFiscaleSimulee, referenceData.PressionFiscale,
            "%", poids: 2.0,
            recommandation: "Pression fiscale trop basse = informel trop élevé, taux trop bas"));

        rapport.Indicateurs.Add(CreerIndicateur(
            "Dépenses publiques annualisées", "Fiscal",
            depensesPubliquesAnnualisees, referenceData.DepensesPubliquesAnnuelles,
            "MGA", poids: 1.0,
            recommandation: "Ajuster DepensesPubliquesJour ou DepensesCapitalJour"));

        double soldeBudgetairePIB = pibAnnualise > 0
            ? (recettesFiscalesAnnualisees - depensesPubliquesAnnualisees) / pibAnnualise
            : 0;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Solde budgétaire / PIB", "Fiscal",
            soldeBudgetairePIB, referenceData.SoldeBudgetairePIB,
            "%", poids: 1.5));

        double dettePubliquePIB = pibAnnualise > 0 ? dettePublique / pibAnnualise : 0;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Dette publique / PIB", "Fiscal",
            dettePubliquePIB, referenceData.DettePubliquePIB,
            "%", poids: 1.0,
            recommandation: "Ajuster DettePubliqueInitiale ou InteretsDetteJour"));

        // ═══════════════════════════════════════════
        //  4. MONÉTAIRE
        // ═══════════════════════════════════════════

        rapport.Indicateurs.Add(CreerIndicateur(
            "Masse monétaire M3", "Monétaire",
            m3Actuel, referenceData.MasseMonetaireM3,
            "MGA", poids: 1.5,
            recommandation: "Ajuster CroissanceCreditJour ou TauxReserveObligatoire"));

        double inflationSimulee = dernier.TauxInflationEndogene;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Taux d'inflation", "Monétaire",
            inflationSimulee, referenceData.TauxInflationAnnuel,
            "%", poids: 2.0,
            recommandation: "Ajuster les poids d'inflation (NAIRU, Phillips, cost-push)"));

        // ═══════════════════════════════════════════
        //  5. INÉGALITÉS ET EMPLOI
        // ═══════════════════════════════════════════

        rapport.Indicateurs.Add(CreerIndicateur(
            "Coefficient de Gini", "Inégalités",
            dernier.Gini, referenceData.Gini,
            "ratio", poids: 1.0,
            recommandation: "Ajuster SalaireSigma ou propensions par classe"));

        rapport.Indicateurs.Add(CreerIndicateur(
            "Ratio D9/D1", "Inégalités",
            dernier.RatioD9D1, referenceData.RatioD9D1,
            "ratio", poids: 0.5));

        double partInformelSimulee = n > 0
            ? (double)dernier.NbEntreprisesInformelles /
              Math.Max(1, dernier.NbEntreprisesInformelles + dernier.NbEntreprisesAssujettiesTVA)
            : 0;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Part secteur informel", "Inégalités",
            partInformelSimulee, referenceData.PartSecteurInformel,
            "%", poids: 1.0,
            recommandation: "Ajuster PartSecteurInformel dans ScenarioConfig"));

        // ═══════════════════════════════════════════
        //  6. ÉNERGIE (Jirama)
        // ═══════════════════════════════════════════

        double productionElecAnnualiseeGWh = result.Snapshots.Average(s => s.ProductionElecKWh) * 365.0 * facteurEchelle / 1_000_000;
        rapport.Indicateurs.Add(CreerIndicateur(
            "Production électrique (GWh/an)", "Énergie",
            productionElecAnnualiseeGWh, referenceData.ProductionElecAnnuelleGWh,
            "GWh", poids: 0.5,
            recommandation: "Ajuster ConsommationElecMenageKWhJour ou NombreMenages"));

        // ═══════════════════════════════════════════
        //  7. TAUX DE CHANGE
        // ═══════════════════════════════════════════

        double tauxChangeFinal = dernier.TauxChangeMGAParUSD;
        if (tauxChangeFinal > 0)
        {
            rapport.Indicateurs.Add(CreerIndicateur(
                "Taux de change MGA/USD", "Change",
                tauxChangeFinal, referenceData.TauxChangeMGAParUSD,
                "MGA/USD", poids: 1.5,
                recommandation: "Ajuster ElasticiteChangeBalanceCommerciale ou IntensiteInterventionBCM"));

            double reservesFin = dernier.ReservesBCMUSD;
            double reservesMoisFin = dernier.ReservesMoisImports;
            rapport.Indicateurs.Add(CreerIndicateur(
                "Réserves BCM (mois d'imports)", "Change",
                reservesMoisFin, referenceData.ReservesMoisImports,
                "mois", poids: 1.0,
                recommandation: "Ajuster ReservesBCMUSD ou IntensiteInterventionBCM"));
        }

        // ═══════════════════════════════════════════
        //  ALERTES DE COHÉRENCE
        // ═══════════════════════════════════════════

        // PIB incohérent entre les 3 approches
        double pibVA = result.Snapshots.Average(s => s.PIBParValeurAjoutee) * 365.0 * facteurEchelle;
        double pibRevenu = result.Snapshots.Average(s => s.PIBParRevenus) * 365.0 * facteurEchelle;
        double ecartPIBApproches = pibAnnualise > 0
            ? Math.Abs(pibVA - pibAnnualise) / pibAnnualise * 100.0
            : 0;
        if (ecartPIBApproches > 10)
        {
            rapport.Alertes.Add(
                $"⚠️ Écart PIB demande vs PIB VA = {ecartPIBApproches:F1}% — réconciliation insuffisante.");
        }

        // Déficit budgétaire excessif
        if (soldeBudgetairePIB < -0.10)
        {
            rapport.Alertes.Add(
                $"🔴 Déficit budgétaire/PIB = {soldeBudgetairePIB:P1} — largement hors norme (réf: -3 à -5%).");
        }

        // Inflation divergente
        if (inflationSimulee > 0.25)
        {
            rapport.Alertes.Add(
                $"🔴 Inflation simulée = {inflationSimulee:P1} — risque d'hyperinflation dans le modèle.");
        }
        else if (inflationSimulee < 0)
        {
            rapport.Alertes.Add(
                "⚠️ Déflation simulée — rare à Madagascar, vérifier les paramètres monétaires.");
        }

        // Gini irréaliste
        if (dernier.Gini < 0.25 || dernier.Gini > 0.65)
        {
            rapport.Alertes.Add(
                $"⚠️ Gini = {dernier.Gini:F3} — hors fourchette réaliste (0.35-0.50 pour Madagascar).");
        }

        // M3 négatif ou nul
        if (m3Actuel <= 0)
        {
            rapport.Alertes.Add("🔴 M3 ≤ 0 — problème critique dans le module bancaire.");
        }

        // Réserves de change épuisées
        if (dernier.ReservesMoisImports < 2.0 && dernier.ReservesMoisImports > 0)
        {
            rapport.Alertes.Add(
                $"🔴 Réserves BCM = {dernier.ReservesMoisImports:F1} mois — en dessous du seuil critique FMI (3 mois).");
        }

        // Taux de change en chute libre
        if (dernier.DepreciationAnnualisee > 0.20)
        {
            rapport.Alertes.Add(
                $"⚠️ Dépréciation MGA = {dernier.DepreciationAnnualisee:P0}/an — risque de crise de change.");
        }

        // ═══════════════════════════════════════════
        //  SCORE GLOBAL
        // ═══════════════════════════════════════════

        double totalPoids = 0;
        double totalScorePondere = 0;
        foreach (var ind in rapport.Indicateurs)
        {
            totalPoids += ind.Poids;
            totalScorePondere += ind.Score * ind.Poids;
        }
        rapport.ScoreGlobal = totalPoids > 0 ? totalScorePondere / totalPoids : 0;

        rapport.Verdict = rapport.ScoreGlobal switch
        {
            >= 90 => "🏆 Excellent — Le modèle reproduit fidèlement les données macro de Madagascar.",
            >= 75 => "✅ Bon — Le modèle est globalement réaliste, quelques écarts à corriger.",
            >= 60 => "☑️ Acceptable — Ordres de grandeur corrects, mais calibrage à affiner.",
            >= 45 => "⚠️ Moyen — Écarts significatifs sur plusieurs indicateurs clés.",
            _ => "❌ Insuffisant — Le modèle nécessite une recalibration majeure."
        };

        // ═══════════════════════════════════════════
        //  RECOMMANDATIONS (top 3 pires indicateurs)
        // ═══════════════════════════════════════════

        var piresIndicateurs = rapport.Indicateurs
            .Where(i => i.Recommandation != null && i.EcartAbsoluPourcent > 15)
            .OrderByDescending(i => i.EcartAbsoluPourcent * i.Poids)
            .Take(3);

        foreach (var ind in piresIndicateurs)
        {
            rapport.Recommandations.Add(
                $"{ind.Icone} {ind.Nom} ({ind.Sens} de {ind.EcartAbsoluPourcent:F0}%) → {ind.Recommandation}");
        }

        return rapport;
    }

    // ═══════════════════════════════════════════
    //  UTILITAIRES
    // ═══════════════════════════════════════════

    private static MacroValidationItem CreerIndicateur(
        string nom, string categorie,
        double valeurSimulee, double valeurReference,
        string unite, double poids = 1.0,
        string? recommandation = null)
    {
        double ecartAbsolu = valeurReference != 0
            ? Math.Abs(valeurSimulee - valeurReference) / Math.Abs(valeurReference) * 100.0
            : (valeurSimulee != 0 ? 100.0 : 0);

        // Score : 100 si écart=0, 0 si écart ≥ SeuilMaxPourcent
        double score = Math.Max(0, 100.0 * (1.0 - Math.Min(1.0, ecartAbsolu / SeuilMaxPourcent)));

        return new MacroValidationItem
        {
            Nom = nom,
            Categorie = categorie,
            ValeurSimulee = valeurSimulee,
            ValeurReference = valeurReference,
            Unite = unite,
            Poids = poids,
            Score = score,
            Recommandation = recommandation
        };
    }
}
