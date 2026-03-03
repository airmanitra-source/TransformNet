namespace MachineLearning.Web.Models.Agents.Companies;

/// <summary>
/// Modèle de la JIRAMA (Jiro sy Rano Malagasy) — monopole public eau et électricité.
///
/// Réalités Madagascar (source : Tableau 21 JIRAMA/ORE, jan-juin 2025) :
/// - Seul fournisseur d'eau courante et d'électricité à Madagascar
/// - Entreprise publique chroniquement déficitaire (subventionnée par l'État)
/// - Production : ~986 023 MWh/semestre, ~51,6% hydraulique / ~48,4% thermique
/// - Prix moyen : 653 Ar/KWh (tendance +4,7%/an)
/// - Consommation totale : ~701 336 MWh/semestre
///   - Ménages : 332 284 MWh (47,4%), ~30,8 KWh/ménage connecté/mois
///   - Industries et services : 363 763 MWh (51,9%)
///   - Éclairage public : 5 290 MWh (0,75%)
/// - Pertes de distribution : ~28,9% (très élevé, norme ~8-12%)
/// - Taux d'accès électricité : ~30% (65% urbain, 10% rural)
/// - Taux d'accès eau courante : ~25% (55% urbain, 10% rural)
/// - Facture moy. ménage : élec. ~20 100 MGA/mois (~670 MGA/j), eau ~15 000 MGA/mois (~500 MGA/j)
///
/// Source : JIRAMA, ORE, Banque Mondiale, INSTAT
/// </summary>
public class Jirama : Company
{
    // --- Abonnés ---
    /// <summary>Nombre de ménages abonnés à l'eau</summary>
    public int NbAbonnesEau { get; set; }

    /// <summary>Nombre de ménages abonnés à l'électricité</summary>
    public int NbAbonnesElectricite { get; set; }

    // --- Tarification ---
    /// <summary>Tarif eau par ménage par jour (MGA)</summary>
    public double TarifEauJour { get; set; } = 500;

    /// <summary>Prix moyen de l'électricité en Ar/KWh (source : Tableau 21)</summary>
    public double PrixElectriciteArKWh { get; set; } = 653;

    /// <summary>Consommation moyenne d'électricité par ménage connecté par jour (KWh)</summary>
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;

    /// <summary>Tarif électricité par ménage connecté par jour (MGA) = KWh × prix</summary>
    public double TarifElectriciteJour => ConsommationElecMenageKWhJour * PrixElectriciteArKWh;

    // --- Production ---
    /// <summary>Part de la production hydraulique (0-1). ~51,6% jan-juin 2025.</summary>
    public double PartProductionHydraulique { get; set; } = 0.516;

    /// <summary>Taux de pertes de distribution (~28,9%). Prod - Conso / Prod.</summary>
    public double TauxPertesDistribution { get; set; } = 0.289;

    // --- Consommation par secteur ---
    /// <summary>Part consommation ménages dans la conso. totale (~47,4%)</summary>
    public double PartConsommationMenages { get; set; } = 0.474;

    /// <summary>Part consommation industries et services (~51,9%)</summary>
    public double PartConsommationIndustrie { get; set; } = 0.519;

    /// <summary>Part éclairage public (~0,75%)</summary>
    public double PartConsommationEclairagePublic { get; set; } = 0.0075;

    // --- Cumuls journaliers ---
    /// <summary>Total KWh consommés par les ménages aujourd'hui</summary>
    public double ConsommationMenagesKWhJour { get; set; }

    /// <summary>Total KWh consommés par les entreprises aujourd'hui</summary>
    public double ConsommationEntreprisesKWhJour { get; set; }

    /// <summary>Total KWh consommés par l'État aujourd'hui (éclairage public + bâtiments)</summary>
    public double ConsommationEtatKWhJour { get; set; }

    /// <summary>Total KWh produits aujourd'hui (estimation)</summary>
    public double ProductionKWhJour { get; set; }

    /// <summary>Recettes HT totales du jour (eau + électricité, tous secteurs confondus)</summary>
    public double RecettesHTJour { get; set; }

    /// <summary>Charges salariales JIRAMA du jour (NombreEmployes × SalaireMoyenMensuel / 30)</summary>
    public double ChargesSalarialesJour => NombreEmployes * SalaireMoyenMensuel / 30.0;

    /// <summary>
    /// Valeur ajoutée JIRAMA du jour.
    /// VA = Recettes HT - Consommations intermédiaires (combustible thermique + maintenance).
    /// Le combustible thermique représente le principal coût intermédiaire.
    /// Estimation : part thermique × production KWh × coût combustible/KWh (~350 Ar/KWh thermique).
    /// Maintenance réseau : ~5% des recettes.
    /// </summary>
    public double ValeurAjouteeJour { get; set; }

    // --- TVA collectée sur les factures ---
    /// <summary>TVA collectée sur les factures eau + électricité ce jour (à reverser à l'État)</summary>
    public double TVACollecteeJour { get; set; }

    /// <summary>Total de la TVA collectée depuis le début de la simulation</summary>
    public double TotalTVACollectee { get; set; }

    // --- Cumuls depuis début simulation ---
    /// <summary>Total des recettes eau depuis le début de la simulation (HT)</summary>
    public double TotalRecettesEau { get; set; }

    /// <summary>Total des recettes électricité depuis le début de la simulation (HT)</summary>
    public double TotalRecettesElectricite { get; set; }

    /// <summary>Total des recettes (eau + électricité, HT)</summary>
    public double TotalRecettes => TotalRecettesEau + TotalRecettesElectricite;

    /// <summary>Total KWh consommés par les ménages depuis le début</summary>
    public double TotalConsommationMenagesKWh { get; set; }

    /// <summary>Total KWh consommés par les entreprises depuis le début</summary>
    public double TotalConsommationEntreprisesKWh { get; set; }

    /// <summary>Total KWh consommés par l'État depuis le début</summary>
    public double TotalConsommationEtatKWh { get; set; }

    /// <summary>Total des recettes électricité entreprises depuis le début (HT)</summary>
    public double TotalRecettesElectriciteEntreprises { get; set; }

    /// <summary>Total des recettes électricité État depuis le début (HT)</summary>
    public double TotalRecettesElectriciteEtat { get; set; }

    /// <summary>Total KWh produits depuis le début</summary>
    public double TotalProductionKWh { get; set; }

    /// <summary>
    /// Prépare le début d'une nouvelle journée (reset des compteurs journaliers).
    /// </summary>
    public void DebutJournee()
    {
        ConsommationMenagesKWhJour = 0;
        ConsommationEntreprisesKWhJour = 0;
        ConsommationEtatKWhJour = 0;
        ProductionKWhJour = 0;
        TVACollecteeJour = 0;
        RecettesHTJour = 0;
        ValeurAjouteeJour = 0;
    }

    /// <summary>
    /// Enregistre le paiement d'un ménage pour l'eau et/ou l'électricité.
    /// Les montants reçus sont TTC. La JIRAMA extrait la TVA (20%) qu'elle
    /// reversera à l'État. Seule la part HT alimente la trésorerie JIRAMA.
    /// Met à jour la trésorerie, les cumuls, et les compteurs KWh.
    /// </summary>
    public void EnregistrerPaiementMenage(double montantEau, double montantElectricite, double kwh, double tauxTVA)
    {
        // Extraire la TVA incluse dans les montants TTC
        double facteurTVA = tauxTVA / (1.0 + tauxTVA);
        double tvaEau = montantEau * facteurTVA;
        double tvaElec = montantElectricite * facteurTVA;
        double tvaTotale = tvaEau + tvaElec;

        double montantEauHT = montantEau - tvaEau;
        double montantElecHT = montantElectricite - tvaElec;

        TotalRecettesEau += montantEauHT;
        TotalRecettesElectricite += montantElecHT;
        // La trésorerie reçoit seulement la part HT (la TVA est due à l'État)
        Tresorerie += montantEauHT + montantElecHT;
        ChiffreAffairesCumule += montantEau + montantElectricite;
        RecettesHTJour += montantEauHT + montantElecHT;

        // TVA collectée (à reverser à l'État)
        TVACollecteeJour += tvaTotale;
        TotalTVACollectee += tvaTotale;

        ConsommationMenagesKWhJour += kwh;
        TotalConsommationMenagesKWh += kwh;
    }

    /// <summary>
    /// Enregistre le paiement d'une entreprise pour l'électricité.
    /// Les montants reçus sont TTC. La JIRAMA extrait la TVA.
    /// </summary>
    public void EnregistrerPaiementEntreprise(double montantElectricite, double kwh, double tauxTVA)
    {
        double facteurTVA = tauxTVA / (1.0 + tauxTVA);
        double tvaElec = montantElectricite * facteurTVA;
        double montantHT = montantElectricite - tvaElec;

        TotalRecettesElectriciteEntreprises += montantHT;
        TotalRecettesElectricite += montantHT;
        Tresorerie += montantHT;
        ChiffreAffairesCumule += montantElectricite;
        RecettesHTJour += montantHT;

        TVACollecteeJour += tvaElec;
        TotalTVACollectee += tvaElec;

        ConsommationEntreprisesKWhJour += kwh;
        TotalConsommationEntreprisesKWh += kwh;
    }

    /// <summary>
    /// Enregistre le paiement de l'État pour l'électricité (éclairage public + bâtiments).
    /// L'État ne paie pas de TVA sur ses propres consommations (exonéré).
    /// </summary>
    public void EnregistrerPaiementEtat(double montantElectricite, double kwh)
    {
        TotalRecettesElectriciteEtat += montantElectricite;
        TotalRecettesElectricite += montantElectricite;
        Tresorerie += montantElectricite;
        ChiffreAffairesCumule += montantElectricite;
        RecettesHTJour += montantElectricite;

        ConsommationEtatKWhJour += kwh;
        TotalConsommationEtatKWh += kwh;
    }

    /// <summary>
    /// Finalise la journée : calcule la production totale à partir de la consommation réelle
    /// de tous les secteurs (ménages + entreprises + État), puis la valeur ajoutée JIRAMA.
    /// Production = consommation totale / (1 - taux pertes)
    /// VA = Recettes HT - Consommations intermédiaires (combustible thermique + maintenance)
    /// </summary>
    public void FinJournee()
    {
        double consoTotale = ConsommationMenagesKWhJour
                           + ConsommationEntreprisesKWhJour
                           + ConsommationEtatKWhJour;
        ProductionKWhJour = TauxPertesDistribution < 1.0
            ? consoTotale / (1.0 - TauxPertesDistribution)
            : consoTotale;
        TotalProductionKWh += ProductionKWhJour;

        // Valeur ajoutée JIRAMA = Recettes HT - Consommations intermédiaires
        // Consommations intermédiaires :
        // 1. Combustible thermique : part thermique × production × ~350 Ar/KWh (coût fuel/diesel)
        // 2. Maintenance réseau : ~5% des recettes (entretien lignes, transformateurs, etc.)
        double coutCombustibleThermique = (1.0 - PartProductionHydraulique) * ProductionKWhJour * 350;
        double coutMaintenance = RecettesHTJour * 0.05;
        double consommationsIntermediaires = coutCombustibleThermique + coutMaintenance;
        ValeurAjouteeJour = RecettesHTJour - consommationsIntermediaires;
    }
}
