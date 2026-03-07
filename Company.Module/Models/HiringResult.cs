namespace Company.Module.Models;

/// <summary>
/// Résultat de la décision d'ajustement de l'emploi d'une entreprise.
///
/// Modélise le marché du travail malgache :
///   - Embauche si la demande excède la capacité ET la trésorerie est saine
///   - Licenciement si la trésorerie est en stress prolongé (N jours consécutifs)
///   - Rigidité à la baisse : l'informel ajuste vite, le formel est plus lent (code du travail)
///
/// Calibrage INSTAT ENEMPSI :
///   - Taux de rotation ~15-20%/an dans le formel
///   - Informel : ajustement quasi-instantané (journaliers agricoles)
///   - Seuil d'embauche : capacité utilisée > 85% pendant 7+ jours
///   - Seuil de licenciement : trésorerie negative pendant 15+ jours (formel) ou 5 jours (informel)
/// </summary>
public class HiringResult
{
    /// <summary>Variation nette d'employés ce jour (+embauche, -licenciement, 0=statu quo).</summary>
    public int VariationEmployes { get; set; }

    /// <summary>Nombre d'embauches ce jour.</summary>
    public int Embauches { get; set; }

    /// <summary>Nombre de licenciements ce jour.</summary>
    public int Licenciements { get; set; }

    /// <summary>Nombre total d'employés après ajustement.</summary>
    public int NouveauNombreEmployes { get; set; }

    /// <summary>Taux d'utilisation de la capacité de production (demande / capacité, 0-1+).</summary>
    public double TauxUtilisationCapacite { get; set; }

    /// <summary>Nombre de jours consécutifs de stress de trésorerie.</summary>
    public int JoursStressConsecutifs { get; set; }

    /// <summary>Nombre de jours consécutifs de demande excédentaire.</summary>
    public int JoursDemandeExcedentaireConsecutifs { get; set; }

    /// <summary>Raison de la décision (diagnostic).</summary>
    public string Raison { get; set; } = "Statu quo";
}
