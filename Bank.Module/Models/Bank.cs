namespace Bank.Module.Models;

/// <summary>
/// Modèle représentant l'agrégation du secteur bancaire de la simulation (Banque Centrale + Banques Commerciales).
/// Traque la création monétaire et les dépôts.
/// </summary>
public class Bank
{
    /// <summary>Total des dépôts des ménages (MGA)</summary>
    public double TotalDepotsMenages { get; set; }

    /// <summary>Total des dépôts des entreprises (MGA)</summary>
    public double TotalDepotsEntreprises { get; set; }

    /// <summary>Total net des crédits nouvellement accordés depuis le début de la simulation (MGA)</summary>
    public double TotalCreditsAccordes { get; set; }

    /// <summary>Masse monétaire M3 approchée (Dépôts + Cash estimé) (MGA)</summary>
    public double MasseMonetaireM3 { get; set; }

    /// <summary>Taux de réserve obligatoire de la banque centrale (~13% à Madagascar)</summary>
    public double TauxReserveObligatoire { get; set; } = 0.13;
}
