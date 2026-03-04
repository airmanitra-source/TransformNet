using Company.Module.Models;
namespace Company.Module.Models;

/// <summary>
/// Résultat journalier d'un ménage.
/// </summary>
public class DailyHouseholdResult
{
    public double RevenuBrut { get; set; }
    /// <summary>Transfert de la diaspora reçu ce jour (MGA)</summary>
    public double Remittance { get; set; }
    /// <summary>Loyer imputé ce jour (MGA, propriétaires occupants uniquement, SCN 2008)</summary>
    public double LoyerImpute { get; set; }
    /// <summary>Cotisation salariale CNaPS retenue ce jour (1% du salaire brut)</summary>
    public double CotisationCNaPSSalariale { get; set; }
    public double ImpotIR { get; set; }
    /// <summary>Taux effectif d'IRSA pour ce ménage</summary>
    public double TauxEffectifIR { get; set; }
    public double DepensesTransport { get; set; }
    public double TVAPayee { get; set; }
    public double Consommation { get; set; }
    /// <summary>Dépenses de riz du jour (local + importé, en MGA)</summary>
    public double DepensesRiz { get; set; }
    /// <summary>Dépenses eau Jirama du jour (MGA, 0 si pas connecté)</summary>
    public double DepensesEau { get; set; }
    /// <summary>Dépenses électricité Jirama du jour (MGA, 0 si pas connecté)</summary>
    public double DepensesElectricite { get; set; }
    /// <summary>Dépenses transport pour paiement facture Jirama (MGA, 0 sauf jour de paiement mensuel)</summary>
    public double DepensesTransportJirama { get; set; }
    public double EpargneJour { get; set; }
    public double EpargneTotale { get; set; }

    // ─── Achat alimentaire journalier (IHouseholdModule.AcheteProduitsAlimentaires) ───

    /// <summary>
    /// Composant alimentaire calculé par <c>Household.SimulerJournee()</c>
    /// (DepensesAlimentairesJour × facteurInflation × facteurChocPrix × (1 − réductionInterne)).
    /// Utilisé pour isoler la demande non-alimentaire dans le routage B2C.
    /// </summary>
    public double DepensesAlimentairesSimulee { get; set; }

    /// <summary>
    /// Coût total réel du panier alimentaire du jour après application du facteur de
    /// réduction et plafonnement à 80 % du revenu disponible (MGA).
    /// Calculé par <c>IHouseholdModule.AcheteProduitsAlimentaires()</c>.
    /// </summary>
    public double DepensesAlimentaires { get; set; }

    /// <summary>
    /// Part du panier achetée dans le secteur informel (85 % × dépense effective, MGA).
    /// Alimente la demande des entreprises informelles.
    /// </summary>
    public double DepensesAlimentairesInformel { get; set; }

    /// <summary>
    /// Part du panier achetée dans le secteur formel (15 % × dépense effective × 1.20 TVA, MGA).
    /// Alimente la demande des entreprises formelles + TVA collectée.
    /// </summary>
    public double DepensesAlimentairesFormel { get; set; }

    // ─── Élasticités & chocs de prix ───────────────────────────────────────────

    /// <summary>
    /// Facteur multiplicatif appliqué aux prix des biens ce jour.
    /// 1.00 = aucun choc | &gt;1.00 = choc carburant transmis aux prix locaux.
    /// Formule : 1 + (Δcarburant/référence × ElasticitéPrix) + aléa marché.
    /// </summary>
    public double FacteurChocPrix { get; set; } = 1.0;

    /// <summary>
    /// Facteur de réduction des quantités alimentaires achetées ce jour (0.30–1.00).
    /// Retourné par <c>IHouseholdModule.AcheteProduitsAlimentaires</c> via courbe logistique.
    /// 1.00 = aucune privation | 0.30 = plancher biologique (seuil de survie).
    /// </summary>
    public double ReductionQuantiteAlimentaire { get; set; } = 1.0;
}



