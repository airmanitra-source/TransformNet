using Company.Module.Models;

namespace Household.Module.Models;

/// <summary>
/// Représente un ménage malgache dans la simulation économique.
/// Paramètres calibrés sur les réalités macroéconomiques de Madagascar.
/// </summary>
public class Household
{
    private static int _nextId = 1;

    public static void ResetIdCounter() => _nextId = 1;

    public int Id { get; } = _nextId++;

    // --- Zone de résidence (dualité urbain/rural) ---
    /// <summary>
    /// Zone de résidence du ménage (Urbain ~30%, Rural ~70%).
    /// Détermine les comportements de consommation, prix, accès aux services.
    /// Source : INSTAT RGPH 2018.
    /// </summary>
    public ZoneResidence Zone { get; set; } = ZoneResidence.Urbain;

    /// <summary>
    /// Indique si le ménage pratique l'autoconsommation agricole.
    /// Concerne principalement les ménages ruraux (~80% des ruraux).
    /// ~40% de la production agricole rurale est autoconsommée.
    /// Source : INSTAT EPM — comptes des ménages ruraux.
    /// </summary>
    public bool PratiqueAutoconsommation { get; set; }

    /// <summary>
    /// Valeur monétaire imputée de l'autoconsommation journalière (MGA).
    /// Cette production est consommée sans transaction monétaire.
    /// Non comptée dans le PIB marchand mais dans le PIB total (SCN 2008).
    /// </summary>
    public double AutoconsommationJour { get; set; }

    /// <summary>Total cumulé de l'autoconsommation imputée depuis le début.</summary>
    public double TotalAutoconsommation { get; set; }

    // --- Revenus (en Ariary - MGA) ---
    /// <summary>Salaire mensuel brut (SMIG Madagascar ≈ 200 000 MGA)</summary>
    public double SalaireMensuel { get; set; } = 200_000;

    /// <summary>Salaire journalier dérivé (salaire mensuel / 30)</summary>
    public double SalaireJournalier => SalaireMensuel / 30.0;

    /// <summary>Classe socio-économique déterminée par la distribution salariale</summary>
    public ClasseSocioEconomique Classe { get; set; } = ClasseSocioEconomique.FormelBas;

    // --- Épargne ---
    /// <summary>Montant total épargné (compte bancaire)</summary>
    public double Epargne { get; set; } = 50_000;

    /// <summary>Taux d'épargne du ménage (part du revenu net épargnée, ~10%)</summary>
    public double TauxEpargne { get; set; } = 0.10;

    // --- Consommation ---
    /// <summary>Propension marginale à consommer (~75% du revenu net)</summary>
    public double PropensionConsommation { get; set; } = 0.75;

    /// <summary>Dépenses alimentaires journalières incompressibles (≈ 4 000 MGA)</summary>
    public double DepensesAlimentairesJour { get; set; } = 4_000;

    /// <summary>Dépenses logement/divers par jour (≈ 1 500 MGA), hors transport, riz et Jirama</summary>
    public double DepensesDiversJour { get; set; } = 1_500;

    /// <summary>Indique si le ménage est propriétaire occupant (pour loyer imputé, SCN 2008)</summary>
    public bool EstProprietaire { get; set; }

    /// <summary>Nombre total d'enfants dans le ménage.</summary>
    public int NombreEnfants { get; set; }

    /// <summary>Nombre d'enfants scolarisés exposés à la dépense d'éducation.</summary>
    public int NombreEnfantsScolarises { get; set; }

    /// <summary>Durée annuelle de la dépense d'éducation en jours.</summary>
    public int DureeDepenseEducationJours { get; set; }

    /// <summary>Loyer journalier payé par les locataires.</summary>
    public double LoyerJournalier { get; set; }

    /// <summary>Indique si le ménage locataire est en train de construire sa maison.</summary>
    public bool EstEnConstructionMaison { get; set; }

    /// <summary>Durée totale de construction de la maison en jours.</summary>
    public int DureeConstructionMaisonJours { get; set; }

    /// <summary>Nombre de jours restants pour la construction de la maison.</summary>
    public int JoursConstructionMaisonRestants { get; set; }

    /// <summary>Budget journalier consacré à la construction de la maison.</summary>
    public double BudgetConstructionMaisonJour { get; set; }

    // --- Consommation de riz ---
    /// <summary>Dépenses quotidiennes de riz (calculé lors de l'initialisation, en MGA)</summary>
    public double DepensesRizJour { get; set; }

    // --- Eau et électricité (Jirama) ---
    /// <summary>Indique si le ménage est connecté à l'eau courante Jirama</summary>
    public bool AccesEau { get; set; }

    /// <summary>Indique si le ménage est connecté à l'électricité Jirama</summary>
    public bool AccesElectricite { get; set; }

    /// <summary>Dépenses eau Jirama par jour (MGA, 0 si pas connecté)</summary>
    public double DepensesEauJour { get; set; }

    /// <summary>Consommation d'électricité par jour en KWh (0 si pas connecté). Source : Tableau 21 Jirama/ORE.</summary>
    public double ConsommationElecKWhJour { get; set; }

    /// <summary>Dépenses électricité Jirama par jour (MGA) = ConsommationElecKWhJour × PrixArKWh</summary>
    public double DepensesElectriciteJour { get; set; }

    // --- Transport ---
    /// <summary>Mode de transport pour aller au travail</summary>
    public ModeTransport Transport { get; set; } = ModeTransport.TransportPublic;

    /// <summary>Distance domicile-travail en km (pour moto/voiture)</summary>
    public double DistanceDomicileTravailKm { get; set; } = 10;

    /// <summary>
    /// Calcule les dépenses de transport journalières (aller-retour).
    /// - Transport public : tarif fixe aller-retour (2 x 600 MGA = 1 200 MGA)
    /// - Moto : consommation ~3L/100km, prix carburant variable
    /// - Voiture : consommation ~8L/100km, prix carburant variable + entretien ~500 MGA/jour
    /// </summary>
    public double CalculerDepensesTransportJour(double prixCarburantLitre)
    {
        double distanceAllerRetour = DistanceDomicileTravailKm * 2;

        return Transport switch
        {
            ModeTransport.TransportPublic => 600 * 2, // 600 MGA aller + 600 MGA retour
            ModeTransport.Moto => (distanceAllerRetour / 100.0) * 3.0 * prixCarburantLitre,    // ~3L/100km
            ModeTransport.Voiture => (distanceAllerRetour / 100.0) * 8.0 * prixCarburantLitre   // ~8L/100km
                                     + 500, // entretien journalier moyen
            _ => 1_200
        };
    }

    /// <summary>Total consommé depuis le début de la simulation</summary>
    public double TotalConsomme { get; set; }

    /// <summary>Total des impôts IRSA payés depuis le début</summary>
    public double TotalImpotsPaye { get; set; }

    /// <summary>Total des dépenses de transport depuis le début</summary>
    public double TotalTransport { get; set; }

    /// <summary>Total des dépenses de riz depuis le début</summary>
    public double TotalDepensesRiz { get; set; }

    /// <summary>Total des paiements Jirama (eau + électricité) depuis le début</summary>
    public double TotalDepensesJirama { get; set; }

    /// <summary>Total des dépenses de transport pour paiement facture Jirama depuis le début</summary>
    public double TotalTransportJirama { get; set; }

    // --- Emploi ---
    /// <summary>Indique si le ménage est employé</summary>
    public bool EstEmploye { get; set; } = true;

    /// <summary>Identifiant de l'entreprise employeuse (null si fonctionnaire ou chômeur)</summary>
    public int? EmployeurId { get; set; }

    /// <summary>
    /// Indique si le chef de ménage est fonctionnaire (agent public).
    /// Les fonctionnaires travaillent lun-ven, sont payés par l'État,
    /// et leur salaire provient du TOFE (SalaireMoyenFonctionnaireMensuel).
    /// </summary>
    public bool EstFonctionnaire { get; set; }

    // --- Secteur informel ---

    /// <summary>
    /// Indique si le ménage travaille dans le secteur informel.
    /// Déterminé à l'initialisation selon l'entreprise employeuse.
    /// ~85% de l'emploi à Madagascar est informel (INSTAT ENEMPSI).
    /// </summary>
    public bool EstDansSecteurInformel { get; set; }

    /// <summary>
    /// Revenu journalier complémentaire issu d'activités informelles annexes (MGA).
    /// Petit commerce ambulant, vente de rue, artisanat, travaux journaliers.
    /// Typique : 1 000-5 000 MGA/jour pour les ménages Subsistance/InformelBas.
    /// Ce revenu s'ajoute au salaire et n'est ni taxé ni soumis au CNaPS.
    /// Source : INSTAT EPM — revenu mixte des ménages informels.
    /// </summary>
    public double RevenuInformelJournalier { get; set; }

    /// <summary>
    /// Indique si le ménage est auto-employé (micro-entrepreneur informel sans employeur).
    /// Les auto-employés n'ont pas d'EmployeurId et tirent leur revenu directement
    /// de leur activité (agriculture de subsistance, petit commerce, artisanat).
    /// Concerne ~60% des travailleurs informels à Madagascar.
    /// </summary>
    public bool EstAutoEmploi { get; set; }

    /// <summary>Total des revenus informels annexes cumulés depuis le début.</summary>
    public double TotalRevenusInformels { get; set; }

    // --- Loisirs et vacances ---
    /// <summary>Budget sortie weekend de base (MGA par sortie)</summary>
    public double BudgetSortieWeekend { get; set; }

    /// <summary>Budget vacances de base (MGA par séjour)</summary>
    public double BudgetVacances { get; set; }

    /// <summary>Probabilité de base de faire une sortie un jour de weekend (0-1)</summary>
    public double ProbabiliteSortieWeekend { get; set; }

    /// <summary>Fréquence des vacances en jours (~90 = trimestriel, 0 = jamais)</summary>
    public int FrequenceVacancesJours { get; set; }

    /// <summary>Probabilité de base de partir en vacances quand la période arrive (0-1)</summary>
    public double ProbabiliteVacances { get; set; }

    /// <summary>Durée des vacances en jours</summary>
    public int DureeVacancesJours { get; set; }

    /// <summary>Jour de la dernière vacance prise</summary>
    public int DerniereVacanceJour { get; set; }

    /// <summary>Jours de vacance restants (si > 0, le ménage est actuellement en vacances)</summary>
    public int JoursVacanceRestants { get; set; }

    /// <summary>Total des dépenses loisirs cumulées depuis le début</summary>
    public double TotalDepensesLoisirs { get; set; }

    /// <summary>Total des dépenses d'éducation cumulées depuis le début.</summary>
    public double TotalDepensesEducation { get; set; }

    /// <summary>Total des dépenses de santé cumulées depuis le début.</summary>
    public double TotalDepensesSante { get; set; }

    /// <summary>Total des loyers locatifs payés depuis le début.</summary>
    public double TotalDepensesLoyer { get; set; }

    /// <summary>Total des dépenses de construction de maison depuis le début.</summary>
    public double TotalDepensesConstructionMaison { get; set; }

    // ═══════════════════════════════════════════
    //  RECONSTRUCTION POST-CYCLONE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Indique si le ménage est en phase de reconstruction post-cyclone.
    /// Quand true, le ménage dépense pour réparer son toit (tôle), ses murs (briques),
    /// créant une demande vers le BTP, la quincaillerie et le transport informel.
    /// </summary>
    public bool EstEnReconstructionCyclone { get; set; }

    /// <summary>
    /// Nombre de jours restants de reconstruction post-cyclone.
    /// Décrémenté chaque jour. Quand atteint 0, EstEnReconstructionCyclone passe à false.
    /// </summary>
    public int JoursReconstructionCycloneRestants { get; set; }

    /// <summary>
    /// Budget journalier de reconstruction post-cyclone (MGA).
    /// Couvre : tôle ondulée, briques, ciment, main d'œuvre maçon.
    /// Réparti entre BTP (55%), quincaillerie (30%) et transport informel (15%).
    /// </summary>
    public double BudgetReconstructionCycloneJour { get; set; }

    /// <summary>Total des dépenses de reconstruction cyclone cumulées depuis le début.</summary>
    public double TotalDepensesReconstructionCyclone { get; set; }

    // ═══════════════════════════════════════════
    //  SÉCHERESSE GRAND SUD (KERE)
    // ═══════════════════════════════════════════

    /// <summary>
    /// Indique si le ménage est affecté par la sécheresse (kere) du Grand Sud.
    /// Contrairement aux cyclones : pas de reconstruction BTP, mais aide alimentaire
    /// et migration interne vers les villes.
    /// </summary>
    public bool EstAffecteSecheresse { get; set; }

    /// <summary>
    /// Nombre de jours restants sous l'effet de la sécheresse.
    /// Pendant cette période : production agricole réduite, dépendance aide alimentaire.
    /// </summary>
    public int JoursSecheresseRestants { get; set; }

    /// <summary>
    /// Indique si le ménage a migré depuis le Grand Sud vers une zone urbaine
    /// suite à la sécheresse (migration interne climatique).
    /// </summary>
    public bool AMigreDepuisSud { get; set; }

    /// <summary>Total de l'aide alimentaire reçue pendant la sécheresse (MGA).</summary>
    public double TotalAideAlimentaireSecheresse { get; set; }

    // ═══════════════════════════════════════════
    //  MICROFINANCE / CRÉDIT SEGMENTÉ
    // ═══════════════════════════════════════════

    /// <summary>
    /// Type de crédit auquel le ménage a accès.
    /// Bancaire formel (~10%), Microfinance IMF (~25%), Tontine informelle (~30%), Aucun (~35%).
    /// </summary>
    public TypeCredit AccesCredit { get; set; } = TypeCredit.Aucun;

    /// <summary>Encours de crédit du ménage (toutes sources confondues, MGA).</summary>
    public double EncoursCreditMenage { get; set; }

    /// <summary>Total des intérêts payés sur les crédits (MGA).</summary>
    public double TotalInteretsCredit { get; set; }

}



