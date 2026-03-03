namespace MachineLearning.Web.Models.Simulation.Companies;

/// <summary>
/// Représente une entreprise (société) malgache dans la simulation.
/// Vend des produits/services aux ménages (B2C) et entre entreprises (B2B).
/// </summary>
public class Company
{
    private static int _nextId = 1;

    public int Id { get; } = _nextId++;
    public string Name { get; set; } = string.Empty;

    // --- Finances (en Ariary - MGA) ---
    /// <summary>Trésorerie disponible</summary>
    public double Tresorerie { get; set; } = 5_000_000;

    /// <summary>Chiffre d'affaires cumulé</summary>
    public double ChiffreAffairesCumule { get; set; }

    /// <summary>Charges cumulées (salaires + achats B2B + taxes)</summary>
    public double ChargesCumulees { get; set; }

    // --- Production ---
    /// <summary>Nombre d'employés</summary>
    public int NombreEmployes { get; set; } = 10;

    /// <summary>Salaire mensuel moyen versé par employé (MGA)</summary>
    public double SalaireMoyenMensuel { get; set; } = 200_000;

    /// <summary>Productivité par employé par jour (valeur produite en MGA)</summary>
    public double ProductiviteParEmployeJour { get; set; } = 15_000;

    /// <summary>Marge bénéficiaire sur les ventes (~20%)</summary>
    public double MargeBeneficiaire { get; set; } = 0.20;

    // --- Ventes B2B ---
    /// <summary>Part du chiffre d'affaires provenant du B2B (~30%)</summary>
    public double PartB2B { get; set; } = 0.30;

    /// <summary>Total des achats B2B effectués (charges)</summary>
    public double TotalAchatsB2B { get; set; }

    // --- Impôts ---
    /// <summary>Total impôt sur les sociétés payé (IS)</summary>
    public double TotalImpotsSociete { get; set; }

    /// <summary>Total TVA collectée et reversée</summary>
    public double TotalTVACollectee { get; set; }

    /// <summary>
    /// Simule une journée pour cette entreprise :
    /// 1. Production basée sur les employés
    /// 2. Ventes B2C (demande des ménages) et B2B
    /// 3. Paiement des salaires (prorata journalier)
    /// 4. Paiement de l'IS sur le bénéfice
    /// 5. Collecte et reversement de la TVA
    /// </summary>
    public DailyCompanyResult SimulerJournee(
        double demandeConsommationMenages,
        double tauxIS,
        double tauxTVA,
        double tauxInflation,
        double tauxDirecteur)
    {
        var result = new DailyCompanyResult();

        // 1. Capacité de production du jour
        double productionJour = NombreEmployes * ProductiviteParEmployeJour;

        // 2. Ventes
        // B2C : limitée par la demande des ménages et la capacité de production
        double capaciteB2C = productionJour * (1.0 - PartB2B);
        double ventesB2C = Math.Min(demandeConsommationMenages, capaciteB2C);

        // B2B : proportionnel à la production
        double ventesB2B = productionJour * PartB2B;
        double ventesTotales = ventesB2C + ventesB2B;

        result.VentesB2C = ventesB2C;
        result.VentesB2B = ventesB2B;
        ChiffreAffairesCumule += ventesTotales;

        // 3. TVA collectée sur les ventes
        double tvaCollectee = ventesTotales * (tauxTVA / (1.0 + tauxTVA));
        result.TVACollectee = tvaCollectee;
        TotalTVACollectee += tvaCollectee;

        // 4. Charges salariales journalières
        double chargesSalariales = (NombreEmployes * SalaireMoyenMensuel) / 30.0;
        result.ChargesSalariales = chargesSalariales;

        // 5. Coût des achats B2B (matières premières, services)
        double coutProduction = productionJour * (1.0 - MargeBeneficiaire) - chargesSalariales;
        coutProduction = Math.Max(0, coutProduction);
        result.AchatsB2B = coutProduction * PartB2B;
        TotalAchatsB2B += result.AchatsB2B;

        // 6. Bénéfice avant impôt
        double chargesTotales = chargesSalariales + result.AchatsB2B;
        double beneficeAvantImpot = ventesTotales - tvaCollectee - chargesTotales;
        result.BeneficeAvantImpot = beneficeAvantImpot;

        ChargesCumulees += chargesTotales;

        // 7. Impôt sur les sociétés (IS)
        double impotIS = beneficeAvantImpot > 0 ? beneficeAvantImpot * tauxIS : 0;
        result.ImpotIS = impotIS;
        TotalImpotsSociete += impotIS;

        // 8. Mise à jour de la trésorerie
        double fluxNet = ventesTotales - tvaCollectee - chargesTotales - impotIS;
        Tresorerie += fluxNet;
        result.FluxNetJour = fluxNet;
        result.Tresorerie = Tresorerie;

        // 9. Effet du taux directeur sur le coût du crédit (simplifié)
        // Un taux directeur élevé augmente le coût de financement
        double coutFinancementJour = Math.Max(0, Tresorerie * -1) * (tauxDirecteur / 365.0);
        if (Tresorerie < 0)
        {
            Tresorerie -= coutFinancementJour;
            result.CoutFinancement = coutFinancementJour;
        }

        return result;
    }

    public static void ResetIdCounter() => _nextId = 1;
}

public class DailyCompanyResult
{
    public double VentesB2C { get; set; }
    public double VentesB2B { get; set; }
    public double ChargesSalariales { get; set; }
    public double AchatsB2B { get; set; }
    public double TVACollectee { get; set; }
    public double ImpotIS { get; set; }
    public double BeneficeAvantImpot { get; set; }
    public double FluxNetJour { get; set; }
    public double Tresorerie { get; set; }
    public double CoutFinancement { get; set; }
}
