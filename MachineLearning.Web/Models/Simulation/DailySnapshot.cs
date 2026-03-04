namespace MachineLearning.Web.Models.Simulation;

/// <summary>
/// Instantané quotidien de l'état de l'économie simulée.
/// </summary>
public class DailySnapshot
{
    public int Jour { get; set; }

    /// <summary>Jour de la semaine (1=Lundi .. 7=Dimanche)</summary>
    public int JourSemaine { get; set; }

    /// <summary>Nom du jour (Lundi, Mardi, ..., Dimanche)</summary>
    public string NomJour { get; set; } = string.Empty;

    /// <summary>Indique si c'est un jour ouvrable (Lun-Ven)</summary>
    public bool EstJourOuvrable { get; set; }

    // --- Agrégats ménages ---
    public double EpargneMoyenneMenages { get; set; }
    public double EpargneTotaleMenages { get; set; }
    public double ConsommationTotaleMenages { get; set; }
    public double RevenuTotalMenages { get; set; }
    public double TauxEmploi { get; set; }
    /// <summary>Total des transferts de la diaspora reçus ce jour (MGA)</summary>
    public double RemittancesTotales { get; set; }
    /// <summary>Total des loyers imputés ce jour (propriétaires occupants, SCN 2008)</summary>
    public double LoyersImputesTotaux { get; set; }
    /// <summary>Total des cotisations salariales CNaPS ce jour</summary>
    public double CotisationsCNaPSSalariales { get; set; }

    // --- Riz ---
    /// <summary>Total des dépenses de riz de tous les ménages ce jour</summary>
    public double DepensesRizTotales { get; set; }

    // --- JIRAMA (eau + électricité) ---
    /// <summary>Total des paiements eau JIRAMA ce jour</summary>
    public double RecettesEauJirama { get; set; }
    /// <summary>Total des paiements électricité JIRAMA ce jour</summary>
    public double RecettesElectriciteJirama { get; set; }
    /// <summary>Total recettes JIRAMA (eau + électricité) ce jour</summary>
    public double RecettesTotalesJirama { get; set; }
    /// <summary>Nombre de ménages abonnés à l'eau JIRAMA</summary>
    public int NbAbonnesEau { get; set; }
    /// <summary>Nombre de ménages abonnés à l'électricité JIRAMA</summary>
    public int NbAbonnesElectricite { get; set; }
    /// <summary>KWh consommés par les ménages ce jour</summary>
    public double ConsommationMenagesKWh { get; set; }
    /// <summary>KWh consommés par les entreprises ce jour</summary>
    public double ConsommationEntreprisesKWh { get; set; }
    /// <summary>KWh consommés par l'État ce jour (éclairage public + bâtiments)</summary>
    public double ConsommationEtatKWh { get; set; }
    /// <summary>Production estimée totale KWh ce jour (incluant pertes)</summary>
    public double ProductionElecKWh { get; set; }
    /// <summary>Prix moyen de l'électricité Ar/KWh ce jour</summary>
    public double PrixElectriciteArKWh { get; set; }
    /// <summary>Facture électricité moyenne par ménage connecté ce jour (MGA)</summary>
    public double FactureElecMoyenneParMenage { get; set; }
    /// <summary>TVA collectée par la JIRAMA sur les factures eau + électricité ce jour</summary>
    public double TVAJiramaJour { get; set; }
    /// <summary>Total des dépenses de transport pour paiement facture JIRAMA ce jour</summary>
    public double DepensesTransportJirama { get; set; }
    /// <summary>Valeur ajoutée créée par la JIRAMA ce jour (recettes HT - consommations intermédiaires)</summary>
    public double ValeurAjouteeJirama { get; set; }

    // --- Agrégats entreprises ---
    public double ChiffreAffairesTotalEntreprises { get; set; }
    public double TresorerieMoyenneEntreprises { get; set; }
    public double BeneficeTotalEntreprises { get; set; }
    public double VentesB2CTotales { get; set; }
    public double VentesB2BTotales { get; set; }
    /// <summary>Total des dépenses d'électricité JIRAMA des entreprises ce jour (MGA)</summary>
    public double DepensesElectriciteEntreprises { get; set; }
    /// <summary>Total des cotisations patronales CNaPS ce jour</summary>
    public double CotisationsCNaPSPatronales { get; set; }
    /// <summary>Nombre d'entreprises informelles</summary>
    public int NbEntreprisesInformelles { get; set; }

    // --- Agrégats État ---
    public double RecettesFiscalesTotales { get; set; }
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double DepensesPubliques { get; set; }
    /// <summary>Dépenses d'électricité JIRAMA de l'État ce jour (MGA)</summary>
    public double DepensesElectriciteEtat { get; set; }
    /// <summary>Aide internationale reçue ce jour</summary>
    public double AideInternationale { get; set; }
    /// <summary>Subventions versées à la JIRAMA ce jour</summary>
    public double SubventionsJirama { get; set; }
    /// <summary>Masse salariale des fonctionnaires ce jour</summary>
    public double SalairesFonctionnaires { get; set; }
    /// <summary>Nombre de ménages fonctionnaires dans la simulation</summary>
    public int NbFonctionnaires { get; set; }
    public double SoldeBudgetaire { get; set; }
    public double DettePublique { get; set; }

    // --- Indicateurs macro ---
    /// <summary>PIB par la demande = C + FBCF + G + (X - M)</summary>
    public double PIBProxy { get; set; }

    /// <summary>FBCF estimée ce jour (Formation Brute de Capital Fixe)</summary>
    public double FBCF { get; set; }

    /// <summary>
    /// Variation de stocks importateurs (ΔS = revente - CIF).
    /// Négatif si constitution de stocks (CIF > revente), positif si déstockage.
    /// Composante I dans le PIB par la demande : I = FBCF + ΔS.
    /// </summary>
    public double VariationStocksImportateurs { get; set; }

    /// <summary>PIB par la valeur ajoutée = Somme(VA de toutes les entreprises)</summary>
    public double PIBParValeurAjoutee { get; set; }

    /// <summary>PIB par les revenus = Rémunération salariés + EBE + Impôts sur production</summary>
    public double PIBParRevenus { get; set; }

    // --- Décomposition PIB revenus ---
    /// <summary>Total des charges salariales versées par les entreprises ce jour (MGA)</summary>
    public double ChargesSalarialesTotalesEntreprises { get; set; }
    /// <summary>Total des cotisations patronales CNaPS ce jour (toutes entreprises formelles)</summary>
    public double CotisationsCNaPSPatronalesTotales { get; set; }
    /// <summary>Excédent Brut d'Exploitation (EBE) total des entreprises ce jour</summary>
    public double ExcedentBrutExploitation { get; set; }
    /// <summary>Valeur ajoutée de l'administration publique (= salaires fonctionnaires)</summary>
    public double ValeurAjouteeAdminPublique { get; set; }

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

    // --- Validation cohérence CA ---
    /// <summary>Nombre d'entreprises dépassant le seuil TVA (400M MGA/an)</summary>
    public int NbEntreprisesAssujettiesTVA { get; set; }

    /// <summary>Pourcentage d'entreprises avec CA cohérent vs secteur (ratio 0.5-2.0)</summary>
    public double PourcentageCoherenceCA { get; set; }

    /// <summary>CA moyen par employé tous secteurs confondus (MGA/jour)</summary>
    public double CAMoyenParEmploye { get; set; }

    /// <summary>CA moyen par employé secteur Textile (MGA/jour)</summary>
    public double CAMoyenParEmployeTextile { get; set; }

    /// <summary>CA moyen par employé secteur Commerce/Services (MGA/jour)</summary>
    public double CAMoyenParEmployeCommerce { get; set; }

    /// <summary>CA moyen par employé secteur Minier (MGA/jour)</summary>
    public double CAMoyenParEmployeMinier { get; set; }
}
