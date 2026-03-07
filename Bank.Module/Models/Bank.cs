namespace Bank.Module.Models;

/// <summary>
/// Modèle représentant l'agrégation du secteur bancaire de la simulation (Banque Centrale + Banques Commerciales).
/// Traque la création monétaire, les agrégats M0/M1/M2/M3, les intérêts et les NPL.
/// Calibré sur les rapports conjoncturels trimestriels de la BCM.
/// </summary>
public class Bank
{
    // ═══════════════════════════════════════════════════════════════
    //  DÉPÔTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Total des dépôts des ménages (MGA)</summary>
    public double TotalDepotsMenages { get; set; }

    /// <summary>Total des dépôts des entreprises (MGA)</summary>
    public double TotalDepotsEntreprises { get; set; }

    /// <summary>Dépôts à vue (comptes courants) — composante de M1 (MGA)</summary>
    public double DepotsAVue { get; set; }

    /// <summary>Dépôts à terme et comptes d'épargne — composante M2 \ M1 (MGA)</summary>
    public double DepotsATerme { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  AGRÉGATS MONÉTAIRES (BCM)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// M0 — Base monétaire (Monnaie Banque Centrale).
    /// = Billets et pièces en circulation + Réserves des banques commerciales à la BCM.
    /// BCM rapport conjoncturel : ~5 000 Mds MGA (2024).
    /// </summary>
    public double BaseMonetaireM0 { get; set; }

    /// <summary>
    /// M1 — Monnaie au sens étroit.
    /// = Monnaie fiduciaire en circulation + Dépôts à vue.
    /// BCM rapport conjoncturel : ~10 000 Mds MGA (2024).
    /// </summary>
    public double MasseMonetaireM1 { get; set; }

    /// <summary>
    /// M2 — Monnaie au sens large intermédiaire.
    /// = M1 + Dépôts à terme + Comptes d'épargne.
    /// BCM rapport conjoncturel : ~18 000 Mds MGA (2024).
    /// </summary>
    public double MasseMonetaireM2 { get; set; }

    /// <summary>
    /// M3 — Masse monétaire au sens large.
    /// = M2 + Autres actifs financiers (placements, titres négociables).
    /// BCM rapport conjoncturel : ~20 000 Mds MGA (2024).
    /// </summary>
    public double MasseMonetaireM3 { get; set; }

    /// <summary>
    /// Monnaie fiduciaire en circulation (billets + pièces hors banques).
    /// BCM : environ 40-50% de M1 à Madagascar.
    /// </summary>
    public double MonnaieCirculation { get; set; }

    /// <summary>
    /// Multiplicateur monétaire effectif = M3 / M0.
    /// BCM 2024 : environ 3.5-4.0.
    /// </summary>
    public double MultiplicateurMonetaire { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  SOLDE EN COMPTE DES BANQUES (SCB) — BFM
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// SCB — Solde en Compte des Banques à la BFM (MGA).
    /// = Réserves obligatoires + Réserves excédentaires.
    /// Proxy direct de la liquidité bancaire du système.
    /// BFM Sept. 2025 : 2 430,2 Mds MGA (fin de mois).
    /// </summary>
    public double SoldeEnCompteBanques { get; set; }

    /// <summary>
    /// Écart moyen SCB - RO (réserves excédentaires moyennes, MGA).
    /// Détermine la capacité réelle de prêt du système bancaire.
    /// BFM T3 2025 : 126,2 Mds MGA d'excédent moyen.
    /// </summary>
    public double EcartMoyenSCB_RO { get; set; }

    /// <summary>
    /// Flux des avoirs extérieurs (Foreign Assets) du jour (MGA).
    /// Entrées de devises (+) ou sorties (-) qui affectent le SCB.
    /// </summary>
    public double FluxFAJour { get; set; }

    /// <summary>
    /// Intervention nette BFM du jour sur le marché monétaire (MGA).
    /// Injection (+) = la BFM injecte de la liquidité → SCB augmente.
    /// Ponction (-) = la BFM retire de la liquidité → SCB diminue.
    /// </summary>
    public double InterventionNetteBFMJour { get; set; }

    /// <summary>
    /// Flux net du SCB ce jour = FluxFA + InterventionBFM + flux endogènes.
    /// </summary>
    public double FluxSCBJour { get; set; }

    /// <summary>
    /// Encours d'interventions nettes BFM en fin de période (MGA).
    /// Stock cumulé des injections/ponctions non dénouées.
    /// </summary>
    public double EncoursInterventionsBFM { get; set; }

    /// <summary>
    /// Liquidité avant intervention BFM = SCB sans interventions (MGA).
    /// </summary>
    public double LiquiditeAvantIntervention { get; set; }

    /// <summary>
    /// Liquidité après intervention BFM = SCB effectif (MGA).
    /// </summary>
    public double LiquiditeApresIntervention { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  CONTREPARTIES DE LA MASSE MONÉTAIRE (BCM)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Avoirs extérieurs nets (Foreign Assets, FA) de la BCM + banques commerciales (MGA).
    /// = Réserves de change × taux de change - engagements extérieurs.
    /// BCM rapport conjoncturel : évolution trimestrielle des FA.
    /// </summary>
    public double AvoirsExtérieursNets { get; set; }

    /// <summary>
    /// Crédit intérieur net = Crédits à l'économie + Créances nettes sur l'État.
    /// Contre-partie principale de M3 côté actif du bilan consolidé.
    /// </summary>
    public double CreditInterieurNet { get; set; }

    /// <summary>
    /// Créances nettes sur l'État (avances BCM au Trésor - dépôts État).
    /// BCM rapport conjoncturel : ligne distincte du bilan monétaire.
    /// </summary>
    public double CreancesNettesEtat { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  CRÉDITS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Total net des crédits nouvellement accordés depuis le début de la simulation (MGA)</summary>
    public double TotalCreditsAccordes { get; set; }

    /// <summary>Encours total de crédits à l'économie (crédits non remboursés, MGA)</summary>
    public double EncoursCreditEconomie { get; set; }

    /// <summary>Crédits accordés aux entreprises ce jour (MGA)</summary>
    public double CreditsEntreprisesJour { get; set; }

    /// <summary>Crédits accordés aux ménages ce jour (MGA)</summary>
    public double CreditsMenagesJour { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  TAUX D'INTÉRÊT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Taux d'intérêt sur les dépôts (rémunération de l'épargne, annuel).
    /// BCM 2024 : taux créditeur moyen pondéré ~4-5%.
    /// </summary>
    public double TauxInteretDepots { get; set; } = 0.045;

    /// <summary>
    /// Taux d'intérêt sur les crédits (coût du crédit, annuel).
    /// BCM 2024 : taux débiteur moyen pondéré ~14-18%.
    /// </summary>
    public double TauxInteretCredits { get; set; } = 0.16;

    /// <summary>Intérêts versés aux déposants ce jour (MGA)</summary>
    public double InteretsDepotJour { get; set; }

    /// <summary>Intérêts perçus sur les crédits ce jour (MGA)</summary>
    public double InteretsCreditJour { get; set; }

    /// <summary>Marge nette d'intérêt du jour = IntérêtsCrédits - IntérêtsDépôts (MGA)</summary>
    public double MargeNetteInteretJour { get; set; }

    /// <summary>Intérêts cumulés versés aux déposants depuis le début (MGA)</summary>
    public double InteretsDepotsCumules { get; set; }

    /// <summary>Intérêts cumulés perçus sur les crédits depuis le début (MGA)</summary>
    public double InteretsCreditsCumules { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  NON-PERFORMING LOANS (NPL)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Encours de crédits non-performants (NPL, MGA).
    /// BCM 2024 : ratio NPL/crédits ~7-9%.
    /// </summary>
    public double EncoursNPL { get; set; }

    /// <summary>
    /// Ratio NPL = EncoursNPL / EncoursCreditEconomie.
    /// BCM 2024 : 7-9%. Seuil d'alerte FMI > 10%.
    /// </summary>
    public double RatioNPL { get; set; }

    /// <summary>Nouveaux NPL du jour (défauts de remboursement, MGA)</summary>
    public double NouveauxNPLJour { get; set; }

    /// <summary>NPL récupérés/provisionnés ce jour (MGA)</summary>
    public double NPLRecuperesJour { get; set; }

    /// <summary>Provisions cumulées pour créances douteuses (MGA)</summary>
    public double ProvisionsCumulees { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  PARAMÈTRES BCM
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Taux de réserve obligatoire de la banque centrale (~13% à Madagascar)</summary>
    public double TauxReserveObligatoire { get; set; } = 0.13;

    /// <summary>Réserves obligatoires effectives = Dépôts × TauxReserveObligatoire (MGA)</summary>
    public double ReservesObligatoires { get; set; }

    /// <summary>Réserves excédentaires (au-delà des obligatoires, MGA)</summary>
    public double ReservesExcedentaires { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  MICROFINANCE / CRÉDIT SEGMENTÉ
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Données agrégées du secteur microfinance et tontines.
    /// Complète le bilan bancaire pour capturer la segmentation du crédit.
    /// </summary>
    public MicrofinanceData Microfinance { get; set; } = new();
}
