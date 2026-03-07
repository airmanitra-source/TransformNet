namespace Simulation.Module.Models;

public class DailySnapshot
{
    public int Jour { get; set; }
    public int JourSemaine { get; set; }
    public string NomJour { get; set; } = string.Empty;
    public bool EstJourOuvrable { get; set; }
    public double EpargneMoyenneMenages { get; set; }
    public double EpargneTotaleMenages { get; set; }
    public double ConsommationTotaleMenages { get; set; }
    public double RevenuTotalMenages { get; set; }
    public double TauxEmploi { get; set; }
    public double RemittancesTotales { get; set; }
    public double LoyersImputesTotaux { get; set; }
    public double CotisationsCNaPSSalariales { get; set; }
    public double DepensesRizTotales { get; set; }
    public double RecettesEauJirama { get; set; }
    public double RecettesElectriciteJirama { get; set; }
    public double RecettesTotalesJirama { get; set; }
    public int NbAbonnesEau { get; set; }
    public int NbAbonnesElectricite { get; set; }
    public double ConsommationMenagesKWh { get; set; }
    public double ConsommationEntreprisesKWh { get; set; }
    public double ConsommationEtatKWh { get; set; }
    public double ProductionElecKWh { get; set; }
    public double PrixElectriciteArKWh { get; set; }
    public double FactureElecMoyenneParMenage { get; set; }
    public double TVAJiramaJour { get; set; }
    public double DepensesTransportJirama { get; set; }
    public double ValeurAjouteeJirama { get; set; }
    public double ChiffreAffairesTotalEntreprises { get; set; }
    public double TresorerieMoyenneEntreprises { get; set; }
    public double BeneficeTotalEntreprises { get; set; }
    public double VentesB2CTotales { get; set; }
    public double VentesB2BTotales { get; set; }
    public double DepensesElectriciteEntreprises { get; set; }
    public double CotisationsCNaPSPatronales { get; set; }
    public int NbEntreprisesInformelles { get; set; }
    public double RecettesFiscalesTotales { get; set; }
    public double RecettesIR { get; set; }
    public double RecettesIS { get; set; }
    public double RecettesTVA { get; set; }
    public double DepensesPubliques { get; set; }
    public double DepensesElectriciteEtat { get; set; }
    public double AideInternationale { get; set; }
    public double SubventionsJirama { get; set; }
    public double SalairesFonctionnaires { get; set; }
    public int NbFonctionnaires { get; set; }
    public double SoldeBudgetaire { get; set; }
    public double DettePublique { get; set; }
    public double PIBProxy { get; set; }
    public double FBCF { get; set; }
    public double VariationStocksImportateurs { get; set; }
    public double PIBParValeurAjoutee { get; set; }
    public double PIBParRevenus { get; set; }
    public double ChargesSalarialesTotalesEntreprises { get; set; }
    public double CotisationsCNaPSPatronalesTotales { get; set; }
    public double ExcedentBrutExploitation { get; set; }
    public double ValeurAjouteeAdminPublique { get; set; }
    public double ImportationsCIF { get; set; }
    public double ExportationsFOB { get; set; }
    public double BalanceCommerciale { get; set; }
    public double RecettesDouanieres { get; set; }
    public double DroitsDouaneJour { get; set; }
    public double AcciseJour { get; set; }
    public int NbImportateurs { get; set; }
    public int NbExportateurs { get; set; }
    public double Gini { get; set; }
    public double RatioD9D1 { get; set; }
    public double EpargneQ1 { get; set; }
    public double EpargneQ2 { get; set; }
    public double EpargneQ3 { get; set; }
    public double EpargneQ4 { get; set; }
    public double EpargneQ5 { get; set; }
    public int NbSubsistance { get; set; }
    public int NbInformelBas { get; set; }
    public int NbFormelBas { get; set; }
    public int NbFormelQualifie { get; set; }
    public int NbCadre { get; set; }
    public int NbEntreprisesAssujettiesTVA { get; set; }
    public double PourcentageCoherenceCA { get; set; }
    public double CAMoyenParEmploye { get; set; }
    public double CAMoyenParEmployeTextile { get; set; }
    public double CAMoyenParEmployeCommerce { get; set; }
    public double CAMoyenParEmployeMinier { get; set; }

    // --- Secteur informel ---

    /// <summary>Nombre de ménages travaillant dans le secteur informel.</summary>
    public int NbMenagesInformels { get; set; }

    /// <summary>Nombre de ménages auto-employés (micro-entrepreneurs informels).</summary>
    public int NbMenagesAutoEmploi { get; set; }

    /// <summary>Revenu informel annexe total journalier de tous les ménages (MGA).</summary>
    public double RevenusInformelsAnnexesJour { get; set; }

    /// <summary>Productivité moyenne effective des entreprises informelles (MGA/employé/jour).</summary>
    public double ProductiviteMoyenneInformel { get; set; }

    /// <summary>Productivité moyenne effective des entreprises formelles (MGA/employé/jour).</summary>
    public double ProductiviteMoyenneFormel { get; set; }

    /// <summary>Chiffre d'affaires total des entreprises informelles (MGA/jour).</summary>
    public double ChiffreAffairesInformel { get; set; }

    /// <summary>Chiffre d'affaires total des entreprises formelles (MGA/jour).</summary>
    public double ChiffreAffairesFormel { get; set; }

    /// <summary>Valeur ajoutée totale des entreprises informelles (MGA/jour).</summary>
    public double ValeurAjouteeInformel { get; set; }

    /// <summary>Valeur ajoutée totale des entreprises formelles (MGA/jour).</summary>
    public double ValeurAjouteeFormel { get; set; }

    /// <summary>
    /// Part du secteur informel dans le PIB proxy (0-1).
    /// Madagascar : ~40-55% du PIB est informel (INSTAT/Banque Mondiale).
    /// </summary>
    public double PartInformelDansPIB { get; set; }

    /// <summary>Nombre total d'employés dans les entreprises informelles.</summary>
    public int NbEmployesSecteurInformel { get; set; }

    /// <summary>Taux d'emploi informel = NbEmployesSecteurInformel / (NbEmployesSecteurInformel + NbSalariesSecteurFormel).</summary>
    public double TauxEmploiInformel { get; set; }

    /// <summary>Revenu moyen journalier des ménages informels (salaire + annexe, MGA).</summary>
    public double RevenuMoyenMenagesInformels { get; set; }

    /// <summary>Revenu moyen journalier des ménages formels (MGA).</summary>
    public double RevenuMoyenMenagesFormels { get; set; }

    /// <summary>Trésorerie moyenne des entreprises informelles (MGA).</summary>
    public double TresorerieMoyenneInformel { get; set; }

    /// <summary>Trésorerie moyenne des entreprises formelles (MGA).</summary>
    public double TresorerieMoyenneFormel { get; set; }

    // --- Secteur Bancaire et Masse Monétaire ---
    public double MasseMonetaireM3 { get; set; }
    public double TotalDepotsBancaires { get; set; }
    public double TotalCreditsAccordes { get; set; }

    // --- Agrégats monétaires BCM (M0/M1/M2/M3) ---

    /// <summary>M0 — Base monétaire (billets en circulation + réserves banques à BCM, MGA).</summary>
    public double BaseMonetaireM0 { get; set; }

    /// <summary>M1 — Monnaie au sens étroit (fiduciaire + dépôts à vue, MGA).</summary>
    public double MasseMonetaireM1 { get; set; }

    /// <summary>M2 — Monnaie au sens large intermédiaire (M1 + dépôts à terme, MGA).</summary>
    public double MasseMonetaireM2 { get; set; }

    /// <summary>Monnaie fiduciaire en circulation (billets + pièces hors banques, MGA).</summary>
    public double MonnaieCirculation { get; set; }

    /// <summary>Multiplicateur monétaire effectif = M3 / M0.</summary>
    public double MultiplicateurMonetaire { get; set; }

    /// <summary>Dépôts à vue (comptes courants, MGA).</summary>
    public double DepotsAVue { get; set; }

    /// <summary>Dépôts à terme (comptes d'épargne, MGA).</summary>
    public double DepotsATerme { get; set; }

    // --- Contreparties de la masse monétaire ---

    /// <summary>Avoirs extérieurs nets (FA, MGA). BCM rapport conjoncturel.</summary>
    public double AvoirsExterieursNets { get; set; }

    /// <summary>Crédit intérieur net (crédits à l'économie + créances sur l'État, MGA).</summary>
    public double CreditInterieurNet { get; set; }

    /// <summary>Créances nettes sur l'État (avances BCM au Trésor, MGA).</summary>
    public double CreancesNettesEtat { get; set; }

    /// <summary>Encours total de crédits à l'économie (MGA).</summary>
    public double EncoursCreditEconomie { get; set; }

    // --- Taux d'intérêt et marge bancaire ---

    /// <summary>Taux d'intérêt sur les dépôts (annuel, ex: 0.045 = 4.5%).</summary>
    public double TauxInteretDepots { get; set; }

    /// <summary>Taux d'intérêt sur les crédits (annuel, ex: 0.16 = 16%).</summary>
    public double TauxInteretCredits { get; set; }

    /// <summary>Intérêts versés aux déposants ce jour (MGA).</summary>
    public double InteretsDepotJour { get; set; }

    /// <summary>Intérêts perçus sur les crédits ce jour (MGA).</summary>
    public double InteretsCreditJour { get; set; }

    /// <summary>Marge nette d'intérêt du jour (MGA).</summary>
    public double MargeNetteInteretJour { get; set; }

    /// <summary>Intérêts cumulés versés aux déposants (MGA).</summary>
    public double InteretsDepotsCumules { get; set; }

    /// <summary>Intérêts cumulés perçus sur les crédits (MGA).</summary>
    public double InteretsCreditsCumules { get; set; }

    // --- Non-Performing Loans (NPL) ---

    /// <summary>Encours de crédits non-performants (NPL, MGA).</summary>
    public double EncoursNPL { get; set; }

    /// <summary>Ratio NPL = NPL / Encours crédits (0-1). BCM 2024 : 7-9%.</summary>
    public double RatioNPL { get; set; }

    /// <summary>Nouveaux NPL du jour (MGA).</summary>
    public double NouveauxNPLJour { get; set; }

    /// <summary>NPL récupérés/provisionnés ce jour (MGA).</summary>
    public double NPLRecuperesJour { get; set; }

    /// <summary>Provisions cumulées pour créances douteuses (MGA).</summary>
    public double ProvisionsCumulees { get; set; }

    /// <summary>Crédits accordés aux entreprises ce jour (MGA).</summary>
    public double CreditsEntreprisesJour { get; set; }

    /// <summary>Crédits accordés aux ménages ce jour (MGA).</summary>
    public double CreditsMenagesJour { get; set; }

    // --- Solde en Compte des Banques (SCB) — BFM ---

    /// <summary>SCB — Solde en Compte des Banques à la BFM (MGA).</summary>
    public double SoldeEnCompteBanques { get; set; }

    /// <summary>Écart moyen SCB - RO = réserves excédentaires (MGA).</summary>
    public double EcartMoyenSCB_RO { get; set; }

    /// <summary>Flux des FA (Foreign Assets) du jour (MGA).</summary>
    public double FluxFAJour { get; set; }

    /// <summary>Intervention nette BFM du jour (MGA). + = injection, - = ponction.</summary>
    public double InterventionNetteBFMJour { get; set; }

    /// <summary>Flux net du SCB ce jour (MGA).</summary>
    public double FluxSCBJour { get; set; }

    /// <summary>Encours d'interventions nettes BFM cumulé (MGA).</summary>
    public double EncoursInterventionsBFM { get; set; }

    /// <summary>Liquidité avant intervention BFM (MGA).</summary>
    public double LiquiditeAvantIntervention { get; set; }

    /// <summary>Liquidité après intervention BFM (MGA).</summary>
    public double LiquiditeApresIntervention { get; set; }

    // ─── Achat alimentaire journalier (IHouseholdModule.AcheteProduitsAlimentaires) ───

    /// <summary>Somme des paniers alimentaires journaliers de tous les ménages (MGA).</summary>
    public double DepensesAlimentairesTotales { get; set; }

    /// <summary>Part informelle des achats alimentaires (85 %, marchés de quartier, MGA).</summary>
    public double DepensesAlimentairesInformelTotal { get; set; }

    /// <summary>Part formelle des achats alimentaires (15 % + TVA 20 %, épiceries, MGA).</summary>
    public double DepensesAlimentairesFormelTotal { get; set; }

    /// <summary>
    /// Facteur de réduction moyen des quantités alimentaires (0.30–1.00).
    /// 1.00 = aucune privation | 0.30 = plancher biologique.
    /// </summary>
    public double FacteurReductionAlimentaireMoyen { get; set; }

    // ─── Loisirs et vacances ──────────────────────────────────────────────────

    /// <summary>Total des dépenses de loisirs (sorties + vacances) tous ménages (MGA).</summary>
    public double DepensesLoisirsTotales { get; set; }

    /// <summary>Total des dépenses d'éducation tous ménages (MGA).</summary>
    public double DepensesEducationTotales { get; set; }

    /// <summary>Total des dépenses de santé tous ménages (MGA).</summary>
    public double DepensesSanteTotales { get; set; }

    /// <summary>Nombre de ménages malades ce jour.</summary>
    public int NbMenagesMalades { get; set; }

    /// <summary>Total des loyers locatifs payés par les ménages locataires (MGA).</summary>
    public double DepensesLoyerLocatifTotales { get; set; }

    /// <summary>Total des dépenses de construction de maison des locataires bâtisseurs (MGA).</summary>
    public double DepensesConstructionMaisonTotales { get; set; }

    /// <summary>Part de la construction allant au secteur BTP (MGA).</summary>
    public double DepensesConstructionBTPTotales { get; set; }

    /// <summary>Part de la construction allant aux commerces de quincaillerie (MGA).</summary>
    public double DepensesConstructionQuincaillerieTotales { get; set; }

    /// <summary>Part de la construction allant au transport informel des matériaux (MGA).</summary>
    public double DepensesConstructionTransportInformelTotales { get; set; }

    /// <summary>Nombre de ménages en sortie weekend ce jour.</summary>
    public int NbMenagesEnSortie { get; set; }

    /// <summary>Nombre de ménages en vacances ce jour.</summary>
    public int NbMenagesEnVacances { get; set; }

    /// <summary>
    /// Facteur de réduction moyen des loisirs dû à la hausse des prix (0-1).
    /// 1.0 = aucune réduction | 0.0 = loisirs totalement supprimés.
    /// </summary>
    public double FacteurReductionLoisirsMoyen { get; set; }

    // ─── Transport (ITransportationModule) ──────────────────────────────────────

    /// <summary>Total des dépenses de transport tous ménages (MGA).</summary>
    public double DepensesTransportTotales { get; set; }

    /// <summary>Part des dépenses de transport routée vers le secteur informel (taxi-be, garagistes, MGA).</summary>
    public double DepensesTransportInformel { get; set; }

    /// <summary>Part des dépenses de transport routée vers le secteur formel (bus, stations-service, MGA).</summary>
    public double DepensesTransportFormel { get; set; }

    /// <summary>Part du transport liée au carburant (moto + voiture, alimente les importateurs, MGA).</summary>
    public double DepensesTransportCarburant { get; set; }

    // ─── Tourisme & Emploi formel (recalibration mensuelle) ─────────────────────

    /// <summary>
    /// CA cumulé des entreprises HôtellerieTourisme (proxy des recettes touristiques, MGA).
    /// Utilisé comme comparable aux données BCM "apport en devises des visiteurs non-résidents".
    /// </summary>
    public double RecettesTourismeCumulees { get; set; }

    /// <summary>
    /// Nombre total de salariés dans les entreprises formelles (non-informelles).
    /// Proxy du stock d'affiliés CNaPS dans la simulation.
    /// </summary>
    public int NbSalariesSecteurFormel { get; set; }

    /// <summary>
    /// Nombre d'entreprises dans le secteur HôtellerieTourisme.
    /// </summary>
    public int NbEntreprisesTourisme { get; set; }

    // ─── Saisonnalité agricole ──────────────────────────────────────────────────

    /// <summary>Jour calendaire dans l'année (1-365).</summary>
    public int JourCalendaire { get; set; }

    /// <summary>Mois calendaire (1-12).</summary>
    public int MoisCalendaire { get; set; }

    /// <summary>Nom de la saison agricole courante (ex: "Soudure (kere)").</summary>
    public string SaisonCourante { get; set; } = "";

    /// <summary>True si on est en période de soudure (fév-avril).</summary>
    public bool EstPeriodeSoudure { get; set; }

    /// <summary>Facteur de productivité agricole saisonnier (0.3-1.8).</summary>
    public double FacteurProductiviteAgricole { get; set; } = 1.0;

    /// <summary>Facteur de prix du riz saisonnier (0.7-1.6).</summary>
    public double FacteurPrixRiz { get; set; } = 1.0;

    /// <summary>Facteur de prix alimentaire général saisonnier (0.85-1.3).</summary>
    public double FacteurPrixAlimentaire { get; set; } = 1.0;

    /// <summary>Facteur tourisme saisonnier (0.35-1.6).</summary>
    public double FacteurTourisme { get; set; } = 1.0;

    /// <summary>Facteur d'emploi agricole saisonnier (0.6-1.4).</summary>
    public double FacteurEmploiAgricole { get; set; } = 1.0;

    // ─── Inflation endogène ──────────────────────────────────────────────────

    /// <summary>Taux d'inflation annualisé endogène calculé ce jour (ex: 0.08 = 8%).</summary>
    public double TauxInflationEndogene { get; set; }

    /// <summary>Composante demand-pull (Phillips) de l'inflation.</summary>
    public double InflationDemandPull { get; set; }

    /// <summary>Composante cost-push (carburant + imports) de l'inflation.</summary>
    public double InflationCostPush { get; set; }

    /// <summary>Composante monétaire (excès M3 vs PIB) de l'inflation.</summary>
    public double InflationMonetaire { get; set; }

    /// <summary>Composante anticipations adaptatives de l'inflation.</summary>
    public double InflationAnticipations { get; set; }

    /// <summary>Output gap = (PIB_effectif - PIB_potentiel) / PIB_potentiel.</summary>
    public double OutputGap { get; set; }

    /// <summary>Écart de chômage = taux_chômage - NAIRU.</summary>
    public double EcartChomage { get; set; }

    /// <summary>Croissance annualisée de M3.</summary>
    public double CroissanceM3 { get; set; }

    /// <summary>Croissance annualisée du PIB.</summary>
    public double CroissancePIB { get; set; }

    /// <summary>Anticipations d'inflation (annualisées).</summary>
    public double AnticipationsInflation { get; set; }

    // ─── Taux de change dynamique MGA/USD ──────────────────────────────────────

    /// <summary>Taux de change MGA par USD (ex: 4500 = 1 USD = 4500 MGA).</summary>
    public double TauxChangeMGAParUSD { get; set; }

    /// <summary>Variation journalière du taux de change (ex: 0.0002 = +0.02%).</summary>
    public double VariationChangeJournaliere { get; set; }

    /// <summary>Dépréciation annualisée du MGA (ex: 0.07 = 7%/an).</summary>
    public double DepreciationAnnualisee { get; set; }

    /// <summary>Réserves de change BCM en USD.</summary>
    public double ReservesBCMUSD { get; set; }

    /// <summary>Réserves BCM en mois d'importations.</summary>
    public double ReservesMoisImports { get; set; }

    /// <summary>Intervention BCM du jour en USD (vente de devises).</summary>
    public double InterventionBCMUSD { get; set; }

    /// <summary>Solde net de devises du jour en USD.</summary>
    public double SoldeDevisesJourUSD { get; set; }

    /// <summary>Indice de pression sur le change (-1 à +1, positif = dépréciation).</summary>
    public double IndicePressionChange { get; set; }

    // ─── Règle de Taylor (taux directeur endogène BCM) ──────────────────────

    /// <summary>Taux directeur effectif BCM ce jour (annuel, ex: 0.095 = 9.5%).</summary>
    public double TauxDirecteurEffectif { get; set; }

    /// <summary>Taux directeur Taylor (théorique, avant lissage).</summary>
    public double TauxDirecteurTaylor { get; set; }

    /// <summary>Composante output gap de la règle de Taylor.</summary>
    public double TaylorComposanteOutputGap { get; set; }

    /// <summary>Composante écart d'inflation de la règle de Taylor.</summary>
    public double TaylorComposanteEcartInflation { get; set; }

    /// <summary>Variation effective du taux directeur ce jour (bps/jour).</summary>
    public double TaylorVariationEffective { get; set; }

    // ─── Dynamique du marché du travail ─────────────────────────────────────

    /// <summary>Nombre total d'embauches ce jour toutes entreprises confondues.</summary>
    public int EmbauchesJour { get; set; }

    /// <summary>Nombre total de licenciements ce jour toutes entreprises confondues.</summary>
    public int LicenciementsJour { get; set; }

    /// <summary>Variation nette d'emploi ce jour (embauches - licenciements).</summary>
    public int VariationNetteEmploi { get; set; }

    /// <summary>Taux d'utilisation moyen de la capacité de production des entreprises (0-1+).</summary>
    public double TauxUtilisationCapaciteMoyen { get; set; }

    /// <summary>Nombre d'entreprises en situation de stress de trésorerie (trésorerie négative).</summary>
    public int NbEntreprisesEnStress { get; set; }

    /// <summary>Nombre total d'employés dans toutes les entreprises (hors fonctionnaires).</summary>
    public int TotalEmployesEntreprises { get; set; }

    // ─── Chocs climatiques (cyclones) ───────────────────────────────────────

    /// <summary>True si un cyclone est actif ce jour.</summary>
    public bool CycloneActif { get; set; }

    /// <summary>Intensité du cyclone (0-1, 0 si aucun cyclone).</summary>
    public double CycloneIntensite { get; set; }

    /// <summary>Nom du cyclone en cours ou dernier cyclone.</summary>
    public string CycloneNom { get; set; } = "";

    /// <summary>Jour dans le cyclone en cours (0 si pas de cyclone).</summary>
    public int CycloneJourDans { get; set; }

    /// <summary>Facteur de productivité dû au cyclone (1.0 = pas d'impact).</summary>
    public double CycloneFacteurProductivite { get; set; } = 1.0;

    /// <summary>Facteur de prix alimentaires dû au cyclone (1.0 = pas d'impact).</summary>
    public double CycloneFacteurPrixAlimentaire { get; set; } = 1.0;

    /// <summary>True si on est en phase de reconstruction post-cyclone.</summary>
    public bool CyclonePhaseReconstruction { get; set; }

    /// <summary>Jour dans la phase de reconstruction (0 si pas de reconstruction).</summary>
    public int CycloneJourReconstruction { get; set; }

    /// <summary>Facteur de boost BTP pendant la reconstruction (1.0 = pas de boost).</summary>
    public double CycloneFacteurDemandeBTP { get; set; } = 1.0;

    /// <summary>Facteur de boost quincaillerie pendant la reconstruction (1.0 = pas de boost).</summary>
    public double CycloneFacteurDemandeQuincaillerie { get; set; } = 1.0;

    /// <summary>Facteur de boost transport matériaux pendant la reconstruction (1.0 = pas de boost).</summary>
    public double CycloneFacteurDemandeTransportMateriaux { get; set; } = 1.0;

    /// <summary>Nombre de ménages en reconstruction post-cyclone ce jour.</summary>
    public int NbMenagesEnReconstructionCyclone { get; set; }

    /// <summary>Total des dépenses de reconstruction cyclone ce jour (MGA).</summary>
    public double DepensesReconstructionCycloneJour { get; set; }

    /// <summary>Part BTP des dépenses de reconstruction cyclone ce jour (MGA).</summary>
    public double DepensesReconstructionBTPJour { get; set; }

    /// <summary>Part quincaillerie des dépenses de reconstruction cyclone ce jour (MGA).</summary>
    public double DepensesReconstructionQuincaillerieJour { get; set; }

    /// <summary>Part transport informel des dépenses de reconstruction cyclone ce jour (MGA).</summary>
    public double DepensesReconstructionTransportJour { get; set; }

    // ─── Investissement productif (FBCF micro) ──────────────────────────────

    /// <summary>FBCF privée micro : somme des investissements productifs de toutes les entreprises (MGA).</summary>
    public double FBCFPriveeMicro { get; set; }

    /// <summary>Dépréciation totale du capital de toutes les entreprises (MGA).</summary>
    public double DepreciationCapitalTotal { get; set; }

    /// <summary>Stock de capital total de toutes les entreprises (MGA).</summary>
    public double StockCapitalTotal { get; set; }

    /// <summary>Nombre d'entreprises qui ont investi ce jour.</summary>
    public int NbEntreprisesInvestisseuses { get; set; }

    /// <summary>Facteur moyen de productivité capital (1.0 = aucun gain).</summary>
    public double FacteurProductiviteCapitalMoyen { get; set; } = 1.0;

    // ─── Matrice input-output (flux inter-sectoriels) ───────────────────────

    /// <summary>Total des consommations intermédiaires inter-sectorielles (MGA).</summary>
    public double ConsommationsIntermediairesInterSectorielles { get; set; }

    /// <summary>
    /// Demande reçue par le secteur Agriculture via les achats inter-sectoriels (MGA).
    /// </summary>
    public double DemandeInduiteAgriculture { get; set; }

    /// <summary>Demande reçue par le secteur Construction via les achats inter-sectoriels (MGA).</summary>
    public double DemandeInduiteConstruction { get; set; }

    /// <summary>Demande reçue par le secteur Services via les achats inter-sectoriels (MGA).</summary>
    public double DemandeInduiteServices { get; set; }

    /// <summary>Demande reçue par le secteur Commerces via les achats inter-sectoriels (MGA).</summary>
    public double DemandeInduiteCommerces { get; set; }
}


