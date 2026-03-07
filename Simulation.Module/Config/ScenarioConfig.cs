using Company.Module.Models;

namespace Simulation.Module.Config;

/// <summary>
/// Configuration d'un scénario de simulation économique.
/// </summary>
public class ScenarioConfig
{
    public string Name { get; set; } = "Scénario de base";
    public string Description { get; set; } = "Paramètres macroéconomiques de base de Madagascar";

    public static readonly double NombreMenagesReference = 6_000_000;
    public static readonly int CourtTerme = 90;
    public static readonly int MoyenTerme = 365;
    public static readonly int LongTerme = 1825;

    public int DureeJours { get; set; } = 365;
    public int NombreMenages { get; set; } = 100000;
    public int NombreEntreprises { get; set; } = 50000;

    public double TauxIS { get; set; } = 0.20;
    public double TauxTVA { get; set; } = 0.20;
    public double TauxDirecteur { get; set; } = 0.09;
    public double TauxInflation { get; set; } = 0.08;
    public double PrixCarburantLitre { get; set; } = 5_500;

    public double SalaireMedian { get; set; } = 170_000;
    public double SalaireSigma { get; set; } = 0.85;
    public double SalairePlancher { get; set; } = 50_000;
    public double PartSecteurInformel { get; set; } = 0.85;

    public double TauxEpargneMenage { get; set; } = 0.10;
    public double PropensionConsommation { get; set; } = 0.75;
    // Propension marginale à consommer par classe socio-économique (0-1)
    public double PropensionConsommation_Subsistance { get; set; } = 0.92; // précédemment behavior
    public double PropensionConsommation_InformelBas { get; set; } = 0.85;
    public double PropensionConsommation_FormelBas { get; set; } = 0.75;
    public double PropensionConsommation_FormelQualifie { get; set; } = 0.65;
    public double PropensionConsommation_Cadre { get; set; } = 0.50;

    public double GetPropensionParClasse(Company.Module.Models.ESecteurActivite? dummy = null) => PropensionConsommation; // placeholder

    public double GetPropensionParClasse(Household.Module.Models.ClasseSocioEconomique classe)
    {
        return classe switch
        {
            Household.Module.Models.ClasseSocioEconomique.Subsistance => PropensionConsommation_Subsistance,
            Household.Module.Models.ClasseSocioEconomique.InformelBas => PropensionConsommation_InformelBas,
            Household.Module.Models.ClasseSocioEconomique.FormelBas => PropensionConsommation_FormelBas,
            Household.Module.Models.ClasseSocioEconomique.FormelQualifie => PropensionConsommation_FormelQualifie,
            Household.Module.Models.ClasseSocioEconomique.Cadre => PropensionConsommation_Cadre,
            _ => PropensionConsommation
        };
    }

    public double ConsommationRizAnnuelleKgParPersonne { get; set; } = 130;
    public double PrixRizLocalKg { get; set; } = 2_400;
    public double PrixRizImporteKg { get; set; } = 2_800;
    public double PartRizImporte { get; set; } = 0.18;

    public double TarifEauJourMenage { get; set; } = 500;
    public double PrixElectriciteArKWh { get; set; } = 653;
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;
    public double PartProductionHydraulique { get; set; } = 0.516;
    public double PartConsommationElecMenages { get; set; } = 0.474;
    public double TauxPertesDistribution { get; set; } = 0.289;

    public double TauxAccesEau { get; set; } = 0.25;
    public double TauxAccesElectricite { get; set; } = 0.30;
    public double CoutTransportPaiementJirama { get; set; } = 1_200;

    public double ConsommationElecParEmployeKWhJour { get; set; } = 2.5;
    public double ConsommationElecEtatKWhJour { get; set; } = 44_400;

    public double MargeBeneficiaireEntreprise { get; set; } = 0.20;
    public double ProductiviteParEmploye { get; set; } = 15_000;
    public double PartEntreprisesAgricoles { get; set; } = 0.30;
    public double PartEntreprisesConstruction { get; set; } = 0.05;
    /// <summary>Part des entreprises dans le secteur hôtellerie/tourisme (~3-4% à Madagascar)</summary>
    public double PartEntreprisesHotellerieTourisme { get; set; } = 0.03;

    public double TauxCotisationsPatronalesCNaPS { get; set; } = 0.18;
    public double TauxCotisationsSalarialesCNaPS { get; set; } = 0.01;

    public double AideInternationaleJour { get; set; } = 3_704_000_000;
    public double SubventionJiramaJour { get; set; } = 1_370_000_000;
    public double RemittancesJour { get; set; } = 7_400_000_000;

    public double LoyerImputeJourParMenage { get; set; } = 1_000;
    public double TauxMenagesProprietaires { get; set; } = 0.65;
    public double LoyerJourLocataire { get; set; } = 3_500;
    public double ProbabiliteConstructionMaisonLocataire { get; set; } = 0.08;
    public int DureeConstructionMaisonJours { get; set; } = 240;
    public double BudgetConstructionMaisonJour { get; set; } = 7_500;
    public double PartBudgetConstructionBTP { get; set; } = 0.55;
    public double PartBudgetConstructionQuincaillerie { get; set; } = 0.30;
    public double PartBudgetConstructionTransportInformel { get; set; } = 0.15;

    public double NombreEnfantsMoyenParMenage { get; set; } = 2.3;
    public double PartEnfantsScolarises { get; set; } = 0.72;
    public int DureeDepenseEducationJours { get; set; } = 180;
    public double CoutEducationJournalierParEnfant { get; set; } = 900;
    public double PartFormelleDepenseEducation { get; set; } = 0.75;

    public double TauxOccupationHopitaux { get; set; } = 0.68;
    public double CoutConsultationSanteBase { get; set; } = 8_000;
    public double CoutHospitalisationSanteBase { get; set; } = 45_000;
    public double PartFormelleDepenseSante { get; set; } = 0.70;

    public int NombreFonctionnaires { get; set; } = 350_000;
    public double SalaireMoyenFonctionnaireMensuel { get; set; } = 863_000;

    public double TauxReinvestissementPrive { get; set; } = 0.25;
    public double DepensesCapitalJour { get; set; } = 13_526_000_000;
    public double InteretsDetteJour { get; set; } = 1_678_000_000;
    public double DettePubliqueInitiale { get; set; } = 29_250_000_000_000;

    public double TauxDroitsDouane { get; set; } = 0.12;
    public double TauxAccise { get; set; } = 0.10;
    public double TauxTaxeExport { get; set; } = 0.03;
    public double DepensesPubliquesJour { get; set; } = 3_218_000_000;

    // ══════════════════════════════════════════════════════════════
    // ░░░ NOUVEAUX PARAMÈTRES : AJUSTEMENT DE PRIX PAR CARBURANT ░░░
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// Élasticité-prix : impact de la variation du carburant sur les prix des marchandises locales.
    /// 0.3 = faible transmission (commerce local absorbé les chocs)
    /// 0.7 = transmission modérée (réaliste pour Madagascar)
    /// 1.0 = transmission complète (100% du choc carburant → prix)
    /// </summary>
    public double ElasticitePrixParCarburant { get; set; } = 0.70;

    /// <summary>
    /// Volatilité de l'aléa de marché ~ N(0, σ).
    /// 0.05 = ±5% d'aléa journalier (marchés stables)
    /// 0.15 = ±15% d'aléa journalier (marchés informels volatiles)
    /// </summary>
    public double VolatiliteAleatoireMarche { get; set; } = 0.10;

    /// <summary>
    /// Part du revenu mensuel consacrée aux dépenses alimentaires.
    /// Utilisée pour calibrer l'impact comportemental des hausses de prix.
    /// Exemple: 0.40 = 40% du revenu en nourriture (réaliste pour pauvres).
    /// </summary>
    public double PartRevenuAlimentaire { get; set; } = 0.40;

    /// <summary>
    /// Élasticité du comportement du consommateur face aux chocs de prix répétitifs.
    /// 0.3 = peu sensible (accepte les augmentations)
    /// 0.7 = très sensible (réduit quantités rapidement)
    /// Affecte la courbe de privation (réduction quantités).
    /// </summary>
    public double ElasticiteComportementMenage { get; set; } = 0.65;

    /// <summary>
    /// Prix carburant de référence pour calibrage des élasticités.
    /// Permet de ramener tous les chocs de carburant à une base comparable.
    /// </summary>
    public double PrixCarburantReference { get; set; } = 5_500;

    // ════════════════════════════════════════════════════════════════
    // ░░░ TAUX DE CHANGE DYNAMIQUE MGA/USD ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le taux de change dynamique.
    /// Si true, le taux MGA/USD est recalculé chaque jour à partir des flux de devises.
    /// Si false, le taux reste fixe (TauxChangeMGAParUSD).
    /// </summary>
    public bool TauxChangeDynamiqueActive { get; set; } = true;

    /// <summary>
    /// Taux de change initial MGA par USD. BCM sept. 2024 ≈ 4 500.
    /// Interprétation : 1 USD = 4 500 MGA.
    /// </summary>
    public double TauxChangeMGAParUSD { get; set; } = 4_500;

    /// <summary>
    /// Taux de change MGA par EUR. BCM sept. 2024 ≈ 5 000.
    /// Calculé comme TauxChangeMGAParUSD × 1.11 (parité EUR/USD).
    /// </summary>
    public double TauxChangeMGAParEUR => TauxChangeMGAParUSD * 1.11;

    /// <summary>
    /// Réserves de change BCM en USD. BCM 2024 ≈ 2.5 milliards USD.
    /// Couvrent environ 5 mois d'importations.
    /// </summary>
    public double ReservesBCMUSD { get; set; } = 2_500_000_000;

    /// <summary>
    /// Taux d'inflation étrangère (USD zone, annuel).
    /// Sert au calcul de la PPA relative. FED 2024 ≈ 2.5-3.5%.
    /// </summary>
    public double InflationEtrangere { get; set; } = 0.03;

    /// <summary>
    /// Élasticité du taux de change au solde commercial (0-1).
    /// 0 = fixité (peg), 1 = flottement libre.
    /// Madagascar ≈ 0.5 (flottement géré BCM).
    /// </summary>
    public double ElasticiteChangeBalanceCommerciale { get; set; } = 0.50;

    /// <summary>
    /// Poids de la PPA relative dans le taux de change (0-1).
    /// Convergence lente vers la parité de pouvoir d'achat.
    /// </summary>
    public double PoidsChangePPA { get; set; } = 0.30;

    /// <summary>
    /// Intensité d'intervention BCM (0 = aucune, 1 = fixité totale).
    /// 0.5 = intervention modérée pour lisser la volatilité.
    /// </summary>
    public double IntensiteInterventionBCM { get; set; } = 0.50;

    /// <summary>
    /// Réserves minimales BCM en mois d'imports.
    /// En dessous, la BCM cesse d'intervenir. FMI recommande 3 mois.
    /// </summary>
    public double ReservesMinimalesMoisImports { get; set; } = 3.0;

    /// <summary>
    /// Tendance de dépréciation structurelle annuelle (ex: 0.05 = 5%/an).
    /// Historique Madagascar 2015-2024 ≈ 5-7%/an.
    /// </summary>
    public double DepreciationStructurelleAnnuelle { get; set; } = 0.05;

    /// <summary>
    /// Élasticité du taux de change vers l'inflation (pass-through, ζ).
    /// Part de la dépréciation MGA transmise aux prix intérieurs.
    /// Madagascar ≈ 0.20-0.40 (pass-through modéré à élevé,
    /// car ~40% du PIB est importé).
    /// </summary>
    public double ElasticiteChangeInflation { get; set; } = 0.30;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ SECTEUR BANCAIRE ET MONÉTAIRE ░░░
    // ════════════════════════════════════════════════════════════════
    public double TauxReserveObligatoire { get; set; } = 0.13;
    public double CroissanceCreditJour { get; set; } = 0.00041; // Permet de viser +15% sur un an (1.15^(1/365) - 1)
    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ SAISONNALITÉ AGRICOLE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la saisonnalité agricole.
    /// Si true, la productivité agricole, les prix alimentaires, les exports
    /// et le tourisme varient selon le calendrier cultural malgache.
    /// Si false, tous les facteurs saisonniers restent à 1.0 (pas de variation).
    /// </summary>
    public bool SaisonnaliteActivee { get; set; } = true;

    /// <summary>
    /// Jour calendaire du début de la simulation (1 = 1er janvier, 182 = 1er juillet).
    /// Détermine la position dans le cycle saisonnier au jour 1 de la simulation.
    /// Par défaut : 1er janvier (début d'année / milieu de saison des pluies).
    /// </summary>
    public int JourCalendaireDebutSimulation { get; set; } = 1;

    /// <summary>
    /// Amplitude de la variation saisonnière de productivité agricole (0-1).
    /// 0 = pas de variation, 0.5 = ±50%, 1.0 = variation extrême.
    /// Calibrage Madagascar : rendements rizicoles varient de 40-60% entre saisons.
    /// </summary>
    public double AmplitudeProductiviteAgricole { get; set; } = 0.50;

    /// <summary>
    /// Amplitude de la variation saisonnière du prix du riz (0-1).
    /// INSTAT : prix du riz local varie de 25-40% entre soudure et post-récolte.
    /// </summary>
    public double AmplitudePrixRiz { get; set; } = 0.35;

    /// <summary>
    /// Amplitude de la variation des prix alimentaires généraux (0-1).
    /// Plus faible que le riz seul (panier diversifié).
    /// </summary>
    public double AmplitudePrixAlimentaire { get; set; } = 0.18;

    /// <summary>
    /// Amplitude de la variation touristique (0-1).
    /// Haute saison (jul-oct) vs basse saison cyclones (jan-mars).
    /// </summary>
    public double AmplitudeTourisme { get; set; } = 0.45;

    /// <summary>
    /// Amplitude de la variation de l'emploi agricole saisonnier (0-1).
    /// Semis/récolte = embauche, inter-saison = sous-emploi rural.
    /// </summary>
    public double AmplitudeEmploiAgricole { get; set; } = 0.25;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ INFLATION ENDOGÈNE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le calcul de l'inflation endogène.
    /// Si true, TauxInflation est recalculé chaque jour à partir des fondamentaux
    /// (Phillips, cost-push, monétaire, anticipations).
    /// Si false, TauxInflation reste fixe (comportement d'origine).
    /// </summary>
    public bool InflationEndogeneActivee { get; set; } = true;

    /// <summary>
    /// NAIRU (Non-Accelerating Inflation Rate of Unemployment).
    /// Taux de chômage structurel en dessous duquel l'inflation accélère.
    /// Madagascar ≈ 0.15-0.20 (sous-emploi structurel élevé).
    /// </summary>
    public double NAIRU { get; set; } = 0.18;

    /// <summary>
    /// Coefficient de Phillips (β) : sensibilité de l'inflation à l'écart de chômage.
    /// Plus β est élevé, plus un chômage bas génère de l'inflation.
    /// Valeurs typiques : 0.01 (faible) à 0.10 (forte réactivité).
    /// </summary>
    public double CoefficientPhillips { get; set; } = 0.03;

    /// <summary>
    /// Poids des anticipations d'inflation dans la formule finale.
    /// Reflète l'inertie inflationniste (spirale prix-salaires).
    /// </summary>
    public double PoidsAnticipationsInflation { get; set; } = 0.40;

    /// <summary>
    /// Poids de la composante demand-pull (Phillips) dans la formule finale.
    /// </summary>
    public double PoidsDemandPullInflation { get; set; } = 0.20;

    /// <summary>
    /// Poids de la composante cost-push (carburant + imports) dans la formule finale.
    /// Élevé pour Madagascar (économie très dépendante des importations).
    /// </summary>
    public double PoidsCostPushInflation { get; set; } = 0.25;

    /// <summary>
    /// Poids de la composante monétaire (excès M3 vs PIB) dans la formule finale.
    /// </summary>
    public double PoidsMonetaireInflation { get; set; } = 0.15;

    /// <summary>
    /// Élasticité carburant → inflation (δ).
    /// Part du choc carburant transmise aux prix intérieurs.
    /// Madagascar ≈ 0.15-0.25 (forte transmission : routes, Jirama thermique).
    /// </summary>
    public double ElasticiteCarburantInflation { get; set; } = 0.20;

    /// <summary>
    /// Élasticité importations → inflation (ε).
    /// Part de la hausse du coût CIF transmise aux prix intérieurs.
    /// </summary>
    public double ElasticiteImportInflation { get; set; } = 0.12;

    /// <summary>
    /// Coefficient monétaire (λ).
    /// Part de l'excès de croissance monétaire (ΔM3/M3 - ΔPIB/PIB) transmise aux prix.
    /// </summary>
    public double CoefficientMonetaire { get; set; } = 0.30;

    /// <summary>
    /// Vitesse d'adaptation des anticipations (α).
    /// 0 = ancrage parfait (BCM très crédible), 1 = adaptatif pur (spirale).
    /// Madagascar ≈ 0.50-0.70 (ancrage modéré, crédibilité BCM limitée).
    /// </summary>
    public double VitesseAdaptationAnticipations { get; set; } = 0.60;

    /// <summary>
    /// Inflation d'ancrage (cible implicite BCM).
    /// La composante anticipative converge vers cette valeur à long terme.
    /// BCM cible implicite ≈ 5-7%.
    /// </summary>
    public double InflationAncrage { get; set; } = 0.06;

    /// <summary>
    /// Taux de croissance annuel du PIB potentiel (tendanciel).
    /// Sert au calcul de l'output gap et à la composante monétaire.
    /// Madagascar ≈ 4-5% (tendance historique 2015-2024).
    /// </summary>
    public double CroissancePIBPotentielAnnuel { get; set; } = 0.045;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ RÈGLE DE TAYLOR (TAUX DIRECTEUR ENDOGÈNE BCM) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active le calcul endogène du taux directeur via la règle de Taylor.
    /// Si true, le taux directeur est recalculé chaque jour en fonction de
    /// l'inflation et de l'output gap. Si false, il reste fixe (TauxDirecteur).
    /// </summary>
    public bool TauxDirecteurEndogeneActive { get; set; } = true;

    /// <summary>
    /// Taux réel neutre r* (rendement d'équilibre, annuel).
    /// FMI estime ~2-3% pour Madagascar (pays à revenu faible).
    /// </summary>
    public double TauxReelNeutreTaylor { get; set; } = 0.02;

    /// <summary>
    /// Cible implicite d'inflation de la BCM (annuel).
    /// La BCM ne pratique pas de ciblage formel mais vise ~5-7%.
    /// </summary>
    public double InflationCibleTaylor { get; set; } = 0.06;

    /// <summary>
    /// Coefficient de réaction de Taylor à l'écart d'inflation α.
    /// Taylor standard = 0.5. Valeurs plus élevées = BCM plus agressive.
    /// </summary>
    public double CoefficientInflationTaylor { get; set; } = 0.50;

    /// <summary>
    /// Coefficient de réaction de Taylor à l'output gap β.
    /// Taylor standard = 0.5. BCM Madagascar probablement plus faible (~0.25)
    /// car la BCM est plus focalisée sur l'inflation que sur la croissance.
    /// </summary>
    public double CoefficientOutputGapTaylor { get; set; } = 0.25;

    /// <summary>
    /// Vitesse de lissage du taux directeur (0-1).
    /// 0.03 = très inertiel (convergence en ~3 mois), réaliste pour la BCM.
    /// 0.10 = réactif (convergence en ~1 mois).
    /// </summary>
    public double VitesseLissageTaylor { get; set; } = 0.03;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ DYNAMIQUE DU MARCHÉ DU TRAVAIL ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la dynamique d'embauche/licenciement des entreprises.
    /// Si true, les entreprises ajustent leurs effectifs en fonction de la demande
    /// et de leur trésorerie. Si false, l'emploi reste fixe (comportement d'origine).
    /// </summary>
    public bool DynamiqueEmploiActivee { get; set; } = true;

    /// <summary>
    /// Seuil d'utilisation de la capacité au-delà duquel l'entreprise envisage d'embaucher.
    /// 0.85 = si la demande dépasse 85% de la capacité de production.
    /// INSTAT : taux d'utilisation moyen ~70-80% dans le formel malgache.
    /// </summary>
    public double SeuilUtilisationEmbauche { get; set; } = 0.85;

    /// <summary>
    /// Nombre de jours consécutifs de demande excédentaire avant embauche (formel).
    /// Reflète le délai de recrutement (annonces, entretiens, formalités CNaPS).
    /// Pour l'informel, ce seuil est divisé par 2 (ajustement rapide).
    /// </summary>
    public int JoursAvantEmbauche { get; set; } = 7;

    /// <summary>
    /// Nombre de jours consécutifs de stress de trésorerie avant licenciement (formel).
    /// Code du travail malgache : préavis de 1-3 mois selon ancienneté.
    /// Pour l'informel, ce seuil est divisé par 3 (pas de code du travail).
    /// </summary>
    public int JoursAvantLicenciement { get; set; } = 15;

    /// <summary>
    /// Taux maximal d'embauche par jour (% des effectifs actuels).
    /// 0.02 = max 2% des effectifs embauchés par jour.
    /// Évite les embauches massives irréalistes.
    /// </summary>
    public double TauxEmbaucheMaxJour { get; set; } = 0.02;

    /// <summary>
    /// Taux maximal de licenciement par jour (% des effectifs actuels).
    /// 0.03 = max 3% des effectifs licenciés par jour.
    /// Légèrement plus élevé que l'embauche (plus facile de licencier que d'embaucher).
    /// </summary>
    public double TauxLicenciementMaxJour { get; set; } = 0.03;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ CHOCS CLIMATIQUES STOCHASTIQUES (CYCLONES) ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active les chocs cycloniques stochastiques.
    /// Si true, des cyclones peuvent survenir aléatoirement pendant la saison
    /// cyclonique (nov-avr) avec des impacts sur la production, les prix et
    /// une phase de reconstruction BTP post-cyclone.
    /// </summary>
    public bool ChocsCycloniquesActives { get; set; } = true;

    /// <summary>
    /// Probabilité journalière de cyclone pendant la saison cyclonique (nov-avr).
    /// 0.003 = ~0.3%/jour → ~54% de chance d'au moins 1 cyclone sur 6 mois.
    /// Calibré BNGRC : 1.5 cyclones/an en moyenne touchant Madagascar.
    /// </summary>
    public double ProbabiliteCycloneJourSaison { get; set; } = 0.003;

    /// <summary>
    /// Probabilité journalière de cyclone hors saison (mai-oct).
    /// Résiduel (~0.02%/jour) pour capturer les événements exceptionnels.
    /// </summary>
    public double ProbabiliteCycloneJourHorsSaison { get; set; } = 0.0002;

    /// <summary>
    /// Part du budget de reconstruction routée vers le BTP (maçons, charpentiers).
    /// Calibrage terrain : ~55% des dépenses de reconstruction vont aux artisans BTP.
    /// </summary>
    public double PartReconstructionBTP { get; set; } = 0.55;

    /// <summary>
    /// Part du budget de reconstruction routée vers la quincaillerie (tôle, briques, ciment).
    /// Calibrage terrain : ~30% des dépenses de reconstruction sont des matériaux.
    /// </summary>
    public double PartReconstructionQuincaillerie { get; set; } = 0.30;

    /// <summary>
    /// Part du budget de reconstruction routée vers le transport informel de matériaux.
    /// Charrettes, camionnettes, pousse-pousse pour acheminer tôle et briques.
    /// </summary>
    public double PartReconstructionTransportInformel { get; set; } = 0.15;

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ VALIDATION MACRO AUTOMATIQUE ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Active la validation macro automatique à la fin de la simulation.
    /// Compare les résultats aux données de référence (INSTAT/BCM/FMI)
    /// et génère un rapport de diagnostic avec scoring.
    /// </summary>
    public bool ValidationMacroActivee { get; set; } = true;

    /// <summary>
    /// Données macroéconomiques de référence pour la validation.
    /// Par défaut : Madagascar 2024 (INSTAT/BCM/FMI).
    /// Modifiables pour tester d'autres années ou scénarios contrefactuels.
    /// </summary>
    public Simulation.Module.Models.MacroReferenceData DonneesReference { get; set; } = new();

    // ════════════════════════════════════════════════════════════════

    // ════════════════════════════════════════════════════════════════
    // ░░░ RECALIBRATION MENSUELLE SUR DONNÉES MACRO OBSERVÉES ░░░
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Activer la recalibration mensuelle automatique.
    /// Si activé, à chaque fin de mois (jour 30, 60, 90...), le simulateur
    /// compare ses résultats aux cibles et ajuste ses paramètres internes.
    /// </summary>
    public bool RecalibrationMensuelleActivee { get; set; } = false;

    /// <summary>
    /// Cibles mensuelles de recalibration (données macro observées).
    /// Chaque entrée correspond à un mois de simulation.
    /// </summary>
    public List<MonthlyCalibrationTarget> CiblesMensuelles { get; set; } = [];

    /// <summary>
    /// Vitesse de convergence (0.0–1.0). Contrôle l'agressivité de la correction.
    /// 0.3 = correction douce (30% de l'écart corrigé), 1.0 = correction totale.
    /// Valeur recommandée : 0.5 (évite les oscillations).
    /// </summary>
    public double VitesseConvergenceRecalibration { get; set; } = 0.5;

    // ════════════════════════════════════════════════════════════════

    public Dictionary<ESecteurActivite, double> TresorerieInitialeParSecteur { get; set; } = new()
    {
        { ESecteurActivite.Agriculture, 2_000_000 },
        { ESecteurActivite.Textiles, 10_000_000 },
        { ESecteurActivite.Commerces, 5_000_000 },
        { ESecteurActivite.Services, 8_000_000 },
        { ESecteurActivite.SecteurMinier, 50_000_000 },
        { ESecteurActivite.Construction, 15_000_000 },
        { ESecteurActivite.HotellerieTourisme, 12_000_000 }
    };

    public Dictionary<ECategorieExport, double> FOBJourParCategorie { get; set; } = new();
    public Dictionary<ECategorieImport, double> CIFJourParCategorie { get; set; } = new();

    public bool UseExportCalibresDirectement { get; set; } = true;
    public string SourceCalibration { get; set; } = "Moyenne INSTAT Tableau 32 (oct 2023 – sept 2025)";
    public Dictionary<ECategorieExport, double> FOBCalibresJour { get; set; } = new();

    public bool UseImportCalibresDirectement { get; set; } = true;
    public string SourceCalibrationImports { get; set; } = "Moyenne INSTAT Tableau 33 (juil 2023 – juin 2025)";
    public Dictionary<ECategorieImport, double> CIFCalibresJour { get; set; } = new();

    public static ScenarioConfig BaseMadagascar() => new();

    public static List<ScenarioConfig> TousLesScenarios()
    {
        return new List<ScenarioConfig>
        {
            // ══════════════════════════════════════════════════════════════════════
            // 1. SCÉNARIO DE BASE — Madagascar actuel (sept. 2025)
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🇲🇬 Madagascar actuel (TOFE sept. 2025)",
                Description = "Calibrage réel sur données INSTAT/FMI sept. 2025. État économique stable, inflation modérée."
            },

            // ══════════════════════════════════════════════════════════════════════
            // 2. CHOC CARBURANT MODÉRÉ (court terme) — +10%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⛽ Choc carburant modéré (+10%)",
                Description = "Carburant: 5500 → 6050 MGA/L. Élasticité normale. Impact court terme : inflation 5-7%, résilience ménages.",
                PrixCarburantLitre = 6_050,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.70,
                VolatiliteAleatoireMarche = 0.10,
                ElasticiteComportementMenage = 0.65
            },

            // ══════════════════════════════════════════════════════════════════════
            // 3. CHOC CARBURANT SÉVÈRE (moyen terme) — +50%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⛽⛽ Crise carburant (moyen terme +50%)",
                Description = "Carburant: 5500 → 8250 MGA/L. Transmission forte. Inflation alimentaire, réduction quantités, impact PIB négatif.",
                PrixCarburantLitre = 8_250,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.75,
                VolatiliteAleatoireMarche = 0.12,
                ElasticiteComportementMenage = 0.70,
                PartRevenuAlimentaire = 0.45
            },

            // ══════════════════════════════════════════════════════════════════════
            // 4. STAGFLATION — +80% carburant, transmission très forte
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🔥 Stagflation (crise sévère +80% carburant)",
                Description = "Carburant: 5500 → 9900 MGA/L. Transmission complète. Inflation 50-60%, privation ménages, chômage, cercle vicieux.",
                PrixCarburantLitre = 9_900,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.85,
                VolatiliteAleatoireMarche = 0.15,
                ElasticiteComportementMenage = 0.75,
                PartRevenuAlimentaire = 0.50,
                TauxDirecteur = 0.15,
                TauxInflation = 0.20
            },

            // ══════════════════════════════════════════════════════════════════════
            // 5. RÉSILIENCE — Commerce absorbe les chocs
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "💪 Résilience (absorption des chocs)",
                Description = "Carburant monte mais commerce absorbe 70%. Volatilité basse. Ménages peu affectés, économie stable.",
                PrixCarburantLitre = 8_250,
                PrixCarburantReference = 5_500,
                ElasticitePrixParCarburant = 0.30,
                VolatiliteAleatoireMarche = 0.05,
                ElasticiteComportementMenage = 0.30,
                PartRevenuAlimentaire = 0.35
            },

            // ══════════════════════════════════════════════════════════════════════
            // 6. STIMULUS FISCAL — Augmentation dépenses publiques
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📊 Stimulus fiscal (+20% dépenses publiques)",
                Description = "État augmente dépenses capital et fonctionnement. Demande aggregate ↑, emploi ↑, inflation modérée.",
                DepensesPubliquesJour = 3_861_600_000,  // +20%
                DepensesCapitalJour = 16_231_200_000,   // +20%
                TauxDirecteur = 0.06,
                TauxInflation = 0.10
            },

            // ══════════════════════════════════════════════════════════════════════
            // 7. AUSTÉRITÉ BUDGÉTAIRE — Réduction dépenses État
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "⚠️ Austérité budgétaire (-20% dépenses publiques)",
                Description = "État réduit drastiquement. Chômage du secteur public, baisse demande, récession.",
                DepensesPubliquesJour = 2_574_400_000,  // -20%
                DepensesCapitalJour = 10_820_800_000,   // -20%
                NombreFonctionnaires = 280_000,         // -20%
                TauxDirecteur = 0.12,
                TauxInflation = 0.12
            },

            // ══════════════════════════════════════════════════════════════════════
            // 8. BOOM EXPORT — Volumes export INSTAT +30%
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📦 Boom export (vanilla, nickel, textile)",
                Description = "Exportations +30% (prix mondiaux ↑ ou volumes). Devise forte, investissements, emploi privé ↑.",
                UseExportCalibresDirectement = true,
                TauxIS = 0.15,
                TauxTVA = 0.18,
                PartEntreprisesConstruction = 0.08
            },

            // ══════════════════════════════════════════════════════════════════════
            // 9. CHOC IMPORTATIONS — Augmentation prix mondiaux
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌍 Choc importations (prix mondiaux +25%)",
                Description = "Coût CIF monte de 25%. Inflation importée, déficit commercial, pression reserve devises.",
                PrixRizImporteKg = 3_500,  // +25%
                PrixCarburantLitre = 6_875,  // +25%
                TauxDroitsDouane = 0.15,
                TauxAccise = 0.12,
                TauxInflation = 0.12
            },

            // ══════════════════════════════════════════════════════════════════════
            // 10. CHOC EMPLOI NÉGATIF — Entreprises réduisent effectifs
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "😔 Crise emploi (chômage +30%)",
                Description = "Entreprises réduisent effectifs de 30%. Baisse salaires, consommation, relayage par secteur informel.",
                NombreMenages = 70_000,
                NombreEntreprises = 35_000,
                MargeBeneficiaireEntreprise = 0.15,
                TauxTVA = 0.16,
                RemittancesJour = 9_620_000_000  // +30% remittances compensent
            },

            // ══════════════════════════════════════════════════════════════════════
            // 11. INNOVATION / PRODUCTIVITÉ — +15% efficacité
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🚀 Innovation & productivité (+15%)",
                Description = "Secteurs modernes (FDI textile, Ambatovy expansion). VA par employé ↑, salaires ↑, gini ↓.",
                ProductiviteParEmploye = 17_250,  // +15%
                SalaireMedian = 195_500,  // +15%
                MargeBeneficiaireEntreprise = 0.25,
                TauxReinvestissementPrive = 0.35
            },

            // ══════════════════════════════════════════════════════════════════════
            // 12. CLIMAT DÉFAVORABLE — Cyclone, sécheresse
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌪️ Choc climatique (récoltes -40%)",
                Description = "Cyclone/sécheresse détruit récoltes. Secteur agricole -40%, inflation alimentaire, pauvreté temporaire.",
                PartEntreprisesAgricoles = 0.18,  // -40% production
                ConsommationRizAnnuelleKgParPersonne = 130,
                PrixRizLocalKg = 3_500,  // +45% pénurie
                PrixRizImporteKg = 4_200,
                TauxInflation = 0.15
            },

            // ══════════════════════════════════════════════════════════════════════
            // 13. POLITIQUE MONÉTAIRE EXPANSIVE — Taux directeur bas
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "💰 Monnaie facile (taux directeur 3%)",
                Description = "Banque centrale baisse taux. Crédit ↑, consommation ↑, investissement ↑, mais inflation risque ↑.",
                TauxDirecteur = 0.03,
                SubventionJiramaJour = 1_644_000_000,  // +20% subvention
                RemittancesJour = 8_880_000_000  // -20% remittances (dévaluation)
            },

            // ══════════════════════════════════════════════════════════════════════
            // 14. CONSOLIDATION FISCALE — Augmentation TVA
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "📈 Hausse TVA (18% → 22%)",
                Description = "Gouvernement augmente TVA pour réduire déficit. Inflation court terme, baisse consommation, mais finances saines LT.",
                TauxTVA = 0.22,
                TauxDirecteur = 0.10,
                AideInternationaleJour = 4_445_000_000  // +20% FMI
            },

            // ══════════════════════════════════════════════════════════════════════
            // 15. SCÉNARIO OPTIMISTE 2030 — Croissance inclusive
            // ══════════════════════════════════════════════════════════════════════
            new()
            {
                Name = "🌟 Vision 2030 (croissance inclusive)",
                Description = "Investissement FDI, réduction inégalités, inflation 4%, emploi +25%, Gini -10%, PIB +7%/an.",
                NombreMenages = 120_000,
                NombreEntreprises = 62_500,
                SalaireMedian = 220_000,
                SalaireSigma = 0.70,  // Moins d'inégalités
                MargeBeneficiaireEntreprise = 0.22,
                ProductiviteParEmploye = 19_500,
                TauxDirecteur = 0.05,
                TauxInflation = 0.04,
                TauxIS = 0.18,
                TauxTVA = 0.20,
                PartEntreprisesConstruction = 0.10
            }
        };
    }
}






