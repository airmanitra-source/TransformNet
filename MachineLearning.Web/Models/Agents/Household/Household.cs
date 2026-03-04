using MachineLearning.Web.Models.Agents.Companies;

namespace MachineLearning.Web.Models.Agents.Household;

/// <summary>
/// Représente un ménage malgache dans la simulation économique.
/// Paramètres calibrés sur les réalités macroéconomiques de Madagascar.
/// </summary>
public class Household
{
    private static int _nextId = 1;

    public static void ResetIdCounter() => _nextId = 1;

    public int Id { get; } = _nextId++;
    public string Name { get; set; } = string.Empty;

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

    /// <summary>Dépenses logement/divers par jour (≈ 1 500 MGA), hors transport, riz et JIRAMA</summary>
    public double DepensesDiversJour { get; set; } = 1_500;

    /// <summary>Indique si le ménage est propriétaire occupant (pour loyer imputé, SCN 2008)</summary>
    public bool EstProprietaire { get; set; }

    // --- Consommation de riz ---
    /// <summary>Dépenses quotidiennes de riz (calculé lors de l'initialisation, en MGA)</summary>
    public double DepensesRizJour { get; set; }

    // --- Eau et électricité (JIRAMA) ---
    /// <summary>Indique si le ménage est connecté à l'eau courante JIRAMA</summary>
    public bool AccesEau { get; set; }

    /// <summary>Indique si le ménage est connecté à l'électricité JIRAMA</summary>
    public bool AccesElectricite { get; set; }

    /// <summary>Dépenses eau JIRAMA par jour (MGA, 0 si pas connecté)</summary>
    public double DepensesEauJour { get; set; }

    /// <summary>Consommation d'électricité par jour en KWh (0 si pas connecté). Source : Tableau 21 JIRAMA/ORE.</summary>
    public double ConsommationElecKWhJour { get; set; }

    /// <summary>Dépenses électricité JIRAMA par jour (MGA) = ConsommationElecKWhJour × PrixArKWh</summary>
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

    /// <summary>Total des paiements JIRAMA (eau + électricité) depuis le début</summary>
    public double TotalDepensesJirama { get; set; }

    /// <summary>Total des dépenses de transport pour paiement facture JIRAMA depuis le début</summary>
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

    /// <summary>
    /// Simule une journée pour ce ménage :
    /// 1. Recevoir salaire journalier (si employé ET jour de travail)
    /// 2. Payer l'IRSA (barème progressif par tranche)
    /// 3. Payer le transport (aller-retour travail, uniquement les jours travaillés)
    /// 4. Payer le riz (dépense incompressible quotidienne, riz local + importé)
    /// 5. Payer l'eau et l'électricité JIRAMA (si connecté)
    ///    + Transport mensuel pour payer la facture à l'agence JIRAMA
    /// 6. Consommer (dépenses incompressibles tous les jours + part discrétionnaire)
    /// 7. Épargner le reste
    /// 
    /// Hypothèse jours ouvrables :
    /// - Les employés ne travaillent PAS le weekend (samedi/dimanche) sauf secteur Commerce
    /// - Les dépenses incompressibles (alimentaires, riz, JIRAMA, divers) s'appliquent 7j/7
    /// - Le transport ne s'applique que les jours travaillés
    /// - Le salaire est versé uniquement les jours travaillés (salaire mensuel / nb jours ouvrables ~22)
    /// </summary>
    public DailyHouseholdResult SimulerJournee(
        Government.Government etat,
        double prixCarburant,
        Companies.Jirama? jirama,
        bool estJourDeTravail = true,
        int jourCourant = 1,
        double coutTransportJirama = 0)
    {
        var result = new DailyHouseholdResult();

        // 1. Revenu du jour (uniquement si employé ET jour de travail)
        // On ajuste le salaire journalier : salaire mensuel / 22 jours ouvrables (au lieu de /30)
        // pour que le total mensuel soit correct sur 22 jours ouvrables
        bool travailleAujourdHui = EstEmploye && estJourDeTravail;
        double salaireJourOuvrable = SalaireMensuel / 22.0; // 22 jours ouvrables/mois
        double revenuBrut = travailleAujourdHui ? salaireJourOuvrable : 0;
        result.RevenuBrut = revenuBrut;

        // 2. IRSA progressif (uniquement les jours travaillés)
        double impotIRSA = travailleAujourdHui ? etat.CalculerIRSAJournalier(SalaireMensuel) : 0;
        result.ImpotIR = impotIRSA;
        result.TauxEffectifIR = SalaireMensuel > 0 ? etat.TauxEffectifIRSA(SalaireMensuel) : 0;
        TotalImpotsPaye += impotIRSA;

        double revenuNet = revenuBrut - impotIRSA;

        // 3. Transport (seulement si jour de travail, il faut aller travailler)
        double depensesTransport = travailleAujourdHui ? CalculerDepensesTransportJour(prixCarburant) : 0;
        // Ajuster par l'inflation (le carburant suit l'inflation)
        double facteurInflationJour = 1.0 + (etat.TauxInflation / 365.0);
        depensesTransport *= facteurInflationJour;
        result.DepensesTransport = depensesTransport;
        TotalTransport += depensesTransport;

        // 4. Riz (dépense quotidienne incompressible, tous les jours)
        double depensesRiz = DepensesRizJour * facteurInflationJour;
        result.DepensesRiz = depensesRiz;
        TotalDepensesRiz += depensesRiz;

        // 5. Eau et électricité JIRAMA (quotidien, tous les jours si connecté)
        double depensesEau = AccesEau ? DepensesEauJour * facteurInflationJour : 0;
        // Électricité : KWh × prix Ar/KWh (prix suit l'inflation)
        double prixKWhAjuste = jirama != null ? jirama.PrixElectriciteArKWh * facteurInflationJour : 0;
        double kwhConsommes = AccesElectricite ? ConsommationElecKWhJour : 0;
        double depensesElectricite = kwhConsommes * prixKWhAjuste;
        result.DepensesEau = depensesEau;
        result.DepensesElectricite = depensesElectricite;
        TotalDepensesJirama += depensesEau + depensesElectricite;

        // Transport pour payer la facture JIRAMA à l'agence (1 fois par mois, si connecté eau ou élec)
        bool estJourPaiementJirama = (AccesEau || AccesElectricite) && jourCourant % 30 == 0;
        double depensesTransportJirama = estJourPaiementJirama ? coutTransportJirama * facteurInflationJour : 0;
        result.DepensesTransportJirama = depensesTransportJirama;
        TotalTransportJirama += depensesTransportJirama;

        // Enregistrer le paiement dans la trésorerie JIRAMA (avec extraction TVA)
        if (jirama != null)
        {
            jirama.EnregistrerPaiementMenage(depensesEau, depensesElectricite, kwhConsommes, etat.TauxTVA);
        }

        // 6. Consommation
        // Dépenses incompressibles = alimentaires + divers + riz + JIRAMA + transport JIRAMA (tous les jours) + transport travail (jours travaillés)
        double depensesIncompressibles = (DepensesAlimentairesJour + DepensesDiversJour) * facteurInflationJour
                                         + depensesTransport
                                         + depensesRiz
                                         + depensesEau + depensesElectricite
                                         + depensesTransportJirama;

        double budgetDiscretionnaire = Math.Max(0, (revenuNet - depensesIncompressibles) * PropensionConsommation);
        double consommationTotale = depensesIncompressibles + budgetDiscretionnaire;

        // TVA payée sur la consommation (hors transport public qui n'est pas soumis à TVA)
        double consommationSoumiseTVA = consommationTotale
            - (Transport == ModeTransport.TransportPublic ? Math.Min(depensesTransport, consommationTotale) : 0);
        double tvaPayee = consommationSoumiseTVA * (etat.TauxTVA / (1.0 + etat.TauxTVA));
        result.TVAPayee = tvaPayee;

        // Vérifier si le ménage peut couvrir ses dépenses
        if (consommationTotale > revenuNet)
        {
            double deficit = consommationTotale - revenuNet;
            if (Epargne >= deficit)
            {
                Epargne -= deficit;
            }
            else
            {
                consommationTotale = revenuNet + Epargne;
                Epargne = 0;
            }
        }

        result.Consommation = consommationTotale;
        TotalConsomme += consommationTotale;

        // 7. Épargne
        double resteApresConsommation = revenuNet - consommationTotale;
        if (resteApresConsommation > 0)
        {
            Epargne += resteApresConsommation;
        }
        result.EpargneJour = Math.Max(0, resteApresConsommation);
        result.EpargneTotale = Epargne;

        return result;
    }
}
