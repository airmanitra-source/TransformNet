using MachineLearning.Web.Models.Simulation.Config;

namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Instantané quotidien de l'état de l'économie simulée.
/// </summary>
public class DailySnapshot
{
    public int Jour { get; set; }

    // --- Agrégats ménages ---
    public double EpargneMoyenneMenages { get; set; }
    public double EpargneTotaleMenages { get; set; }
    public double ConsommationTotaleMenages { get; set; }
    public double RevenuTotalMenages { get; set; }
    public double TauxEmploi { get; set; }

    // --- Agrégats entreprises ---
    public double ChiffreAffairesTotalEntreprises { get; set; }
    public double TresorerieMoyenneEntreprises { get; set; }
    public double BeneficeTotalEntreprises { get; set; }
    public double VentesB2CTotales { get; set; }
    public double VentesB2BTotales { get; set; }

    // --- Agrégats État ---
    public double RecettesFiscalesTotales { get; set; }
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double DepensesPubliques { get; set; }
    public double SoldeBudgetaire { get; set; }
    public double DettePublique { get; set; }

    // --- Indicateurs macro ---
    /// <summary>PIB proxy = C + I + G + (X - M)</summary>
    public double PIBProxy { get; set; }

    // --- Commerce extérieur ---
    /// <summary>Total des importations CIF du jour</summary>
    public double ImportationsCIF { get; set; }
    /// <summary>Total des exportations FOB du jour</summary>
    public double ExportationsFOB { get; set; }
    /// <summary>Balance commerciale = Export - Import</summary>
    public double BalanceCommerciale { get; set; }
    /// <summary>Recettes douanières totales du jour (DD + Accise + TVA import + Taxe export)</summary>
    public double RecettesDouanieres { get; set; }
    /// <summary>Droits de douane du jour</summary>
    public double DroitsDouaneJour { get; set; }
    /// <summary>Droits d'accise du jour</summary>
    public double AcciseJour { get; set; }
    /// <summary>Nombre d'importateurs</summary>
    public int NbImportateurs { get; set; }
    /// <summary>Nombre d'exportateurs</summary>
    public int NbExportateurs { get; set; }

    // --- Distribution des revenus ---
    /// <summary>Coefficient de Gini courant</summary>
    public double Gini { get; set; }
    /// <summary>Ratio interdécile D9/D1</summary>
    public double RatioD9D1 { get; set; }
    /// <summary>Épargne moyenne par quintile (Q1 = plus pauvres)</summary>
    public double EpargneQ1 { get; set; }
    public double EpargneQ2 { get; set; }
    public double EpargneQ3 { get; set; }
    public double EpargneQ4 { get; set; }
    public double EpargneQ5 { get; set; }
    /// <summary>Nombre de ménages par classe socio-économique</summary>
    public int NbSubsistance { get; set; }
    public int NbInformelBas { get; set; }
    public int NbFormelBas { get; set; }
    public int NbFormelQualifie { get; set; }
    public int NbCadre { get; set; }
}

/// <summary>
/// Résultat complet d'une simulation.
/// </summary>
public class SimulationResult
{
    public ScenarioConfig Scenario { get; set; } = new();
    public List<DailySnapshot> Snapshots { get; set; } = [];
    public int JoursSimules { get; set; }
    public bool EstTerminee { get; set; }
    public bool EnCours { get; set; }

    // --- Résumé final ---
    public double EpargneFinale => Snapshots.Count > 0 ? Snapshots[^1].EpargneTotaleMenages : 0;
    public double RecettesFiscalesFinales => Snapshots.Count > 0 ? Snapshots[^1].RecettesFiscalesTotales : 0;
    public double DettePubliqueFinale => Snapshots.Count > 0 ? Snapshots[^1].DettePublique : 0;
    public double PIBFinal => Snapshots.Count > 0 ? Snapshots[^1].PIBProxy : 0;
}
