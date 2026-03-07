namespace Simulation.Module.Models;

/// <summary>
/// Données extraites du Tableau de Bord de l'Économie (TBE) publié par l'INSTAT Madagascar.
/// Le TBE est un PDF trimestriel contenant des séries mensuelles sur ~24 mois glissants.
///
/// Tableaux parsés :
///   - Tableau 26 : Travailleurs nouvellement affiliés à la CNaPS (par mois)
///   - Tableau 29 : Opérations Globales du Trésor — RECETTES (cumul annuel, Mds MGA)
///   - Tableau 31 : Indicateurs monétaires — M3, crédit à l'économie (Mds MGA)
///   - Tableau 32 : Exportations FOB mensuelles (millions MGA)
///   - Tableau 33 : Importations CIF mensuelles (millions MGA)
///   - Tableau 34 : Indicateurs du secteur tourisme (arrivées, devises DTS)
/// </summary>
public class InstatTbeData
{
    /// <summary>Numéro du TBE (ex: 60).</summary>
    public int NumeroTbe { get; set; }

    /// <summary>Mois de référence de la publication (ex: "Octobre 2025").</summary>
    public string PeriodeReference { get; set; } = "";

    /// <summary>Date/heure de la collecte.</summary>
    public DateTime DateCollecte { get; set; }

    /// <summary>URL du PDF source.</summary>
    public string UrlSource { get; set; } = "";

    /// <summary>Enregistrements mensuels extraits, triés par date décroissante.</summary>
    public List<InstatTbeMensuel> Mois { get; set; } = [];

    /// <summary>Erreurs de parsing rencontrées.</summary>
    public List<string> Erreurs { get; set; } = [];

    /// <summary>Indique si au moins quelques mois de données ont été extraits.</summary>
    public bool EstExploitable => Mois.Count > 0 && Mois.Any(m =>
        m.M3Mds.HasValue || m.ExportsFobMillions.HasValue || m.NouveauxAffiliesCNaPS.HasValue);
}

/// <summary>
/// Données d'un mois extrait du TBE INSTAT.
/// </summary>
public class InstatTbeMensuel
{
    /// <summary>Année du mois (ex: 2025).</summary>
    public int Annee { get; set; }

    /// <summary>Mois calendaire (1-12).</summary>
    public int Mois { get; set; }

    /// <summary>Label brut extrait du PDF (ex: "janv 25").</summary>
    public string Label { get; set; } = "";

    // ── Monétaire (Tableau 31) ──────────────────────────────

    /// <summary>Masse monétaire M3 en fin de mois (Milliards MGA). Source: BFM via TBE Tableau 31.</summary>
    public double? M3Mds { get; set; }

    /// <summary>Crédit à l'économie court terme (Milliards MGA).</summary>
    public double? CreditEconomieCourtTermeMds { get; set; }

    /// <summary>Crédit à l'économie moyen/long terme (Milliards MGA).</summary>
    public double? CreditEconomieMoyenLongTermeMds { get; set; }

    /// <summary>Taux de change moyen MGA/USD du mois.</summary>
    public double? TauxChangeMgaUsd { get; set; }

    // ── Finances publiques (Tableau 29) ─────────────────────

    /// <summary>Recettes fiscales cumulées depuis janvier (Milliards MGA). Source: DGT/MEF via TBE Tableau 29.</summary>
    public double? RecettesFiscalesCumulMds { get; set; }

    /// <summary>Recettes totales cumulées depuis janvier (Milliards MGA).</summary>
    public double? RecettesTotalesCumulMds { get; set; }

    // ── Commerce extérieur (Tableaux 32-33) ─────────────────

    /// <summary>Exportations FOB du mois (millions MGA). Source: DSE/INSTAT via TBE Tableau 32.</summary>
    public double? ExportsFobMillions { get; set; }

    /// <summary>Importations CIF du mois (millions MGA). Source: DSE/INSTAT via TBE Tableau 33.</summary>
    public double? ImportsCifMillions { get; set; }

    // ── Tourisme (Tableau 34) ───────────────────────────────

    /// <summary>Arrivées de visiteurs non-résidents aux frontières. Source: MTA via TBE Tableau 34.</summary>
    public int? ArriveesTouristes { get; set; }

    /// <summary>Apport en devises des visiteurs (millions DTS). Source: MTA via TBE Tableau 34.</summary>
    public double? DevisesTourismeMillionsDTS { get; set; }

    // ── Emploi formel (Tableau 26) ──────────────────────────

    /// <summary>Nombre total de travailleurs nouvellement affiliés à la CNaPS ce mois. Source: CNaPS via TBE Tableau 26.</summary>
    public int? NouveauxAffiliesCNaPS { get; set; }
}
