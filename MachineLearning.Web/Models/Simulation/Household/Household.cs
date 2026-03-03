namespace MachineLearning.Web.Models.Simulation.Household;

/// <summary>
/// Mode de transport utilisé par le ménage pour aller travailler.
/// </summary>
public enum ModeTransport
{
    /// <summary>Transport en commun (taxi-be, bus) : 600 MGA l'aller, 1 200 MGA aller-retour</summary>
    TransportPublic,
    /// <summary>Moto personnelle : coût carburant variable</summary>
    Moto,
    /// <summary>Voiture personnelle : coût carburant + entretien</summary>
    Voiture
}

/// <summary>
/// Représente un ménage malgache dans la simulation économique.
/// Paramètres calibrés sur les réalités macroéconomiques de Madagascar.
/// </summary>
public class Household
{
    private static int _nextId = 1;

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

    /// <summary>Dépenses logement/divers par jour (≈ 1 500 MGA), hors transport</summary>
    public double DepensesDiversJour { get; set; } = 1_500;

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

    // --- Emploi ---
    /// <summary>Indique si le ménage est employé</summary>
    public bool EstEmploye { get; set; } = true;

    /// <summary>Identifiant de l'entreprise employeuse</summary>
    public int? EmployeurId { get; set; }

    /// <summary>
    /// Simule une journée pour ce ménage :
    /// 1. Recevoir salaire journalier (si employé)
    /// 2. Payer l'IRSA (barème progressif par tranche)
    /// 3. Payer le transport (aller-retour travail)
    /// 4. Consommer (dépenses incompressibles + part discrétionnaire)
    /// 5. Épargner le reste
    /// </summary>
    public DailyHouseholdResult SimulerJournee(Government etat, double prixCarburant)
    {
        var result = new DailyHouseholdResult();

        // 1. Revenu du jour
        double revenuBrut = EstEmploye ? SalaireJournalier : 0;
        result.RevenuBrut = revenuBrut;

        // 2. IRSA progressif (calculé par le barème de l'État)
        double impotIRSA = EstEmploye ? etat.CalculerIRSAJournalier(SalaireMensuel) : 0;
        result.ImpotIR = impotIRSA;
        result.TauxEffectifIR = SalaireMensuel > 0 ? etat.TauxEffectifIRSA(SalaireMensuel) : 0;
        TotalImpotsPaye += impotIRSA;

        double revenuNet = revenuBrut - impotIRSA;

        // 3. Transport (seulement si employé, il faut aller travailler)
        double depensesTransport = EstEmploye ? CalculerDepensesTransportJour(prixCarburant) : 0;
        // Ajuster par l'inflation (le carburant suit l'inflation)
        double facteurInflationJour = 1.0 + (etat.TauxInflation / 365.0);
        depensesTransport *= facteurInflationJour;
        result.DepensesTransport = depensesTransport;
        TotalTransport += depensesTransport;

        // 4. Consommation
        double depensesIncompressibles = DepensesAlimentairesJour + DepensesDiversJour + depensesTransport;
        depensesIncompressibles *= facteurInflationJour;
        // Ne pas appliquer l'inflation 2 fois sur le transport
        depensesIncompressibles = (DepensesAlimentairesJour + DepensesDiversJour) * facteurInflationJour
                                 + depensesTransport;

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

        // 5. Épargne
        double resteApresConsommation = revenuNet - consommationTotale;
        if (resteApresConsommation > 0)
        {
            Epargne += resteApresConsommation;
        }
        result.EpargneJour = Math.Max(0, resteApresConsommation);
        result.EpargneTotale = Epargne;

        return result;
    }

    public static void ResetIdCounter() => _nextId = 1;
}

public class DailyHouseholdResult
{
    public double RevenuBrut { get; set; }
    public double ImpotIR { get; set; }
    /// <summary>Taux effectif d'IRSA pour ce ménage</summary>
    public double TauxEffectifIR { get; set; }
    public double DepensesTransport { get; set; }
    public double TVAPayee { get; set; }
    public double Consommation { get; set; }
    public double EpargneJour { get; set; }
    public double EpargneTotale { get; set; }
}
