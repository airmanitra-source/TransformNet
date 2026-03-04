namespace MachineLearning.Web.Models.Agents.Companies;

public class JiramaViewModel
{
    public int NbAbonnesEau { get; set; }
    public int NbAbonnesElectricite { get; set; }
    public double TarifEauJour { get; set; } = 500;
    public double PrixElectriciteArKWh { get; set; } = 653;
    public double ConsommationElecMenageKWhJour { get; set; } = 1.03;
    public double TarifElectriciteJour => ConsommationElecMenageKWhJour * PrixElectriciteArKWh;
    public double PartProductionHydraulique { get; set; } = 0.516;
    public double TauxPertesDistribution { get; set; } = 0.289;
    public double PartConsommationMenages { get; set; } = 0.474;
    public double PartConsommationIndustrie { get; set; } = 0.519;
    public double PartConsommationEclairagePublic { get; set; } = 0.0075;
    public double ConsommationMenagesKWhJour { get; set; }
    public double ConsommationEntreprisesKWhJour { get; set; }
    public double ConsommationEtatKWhJour { get; set; }
    public double ProductionKWhJour { get; set; }
    public double RecettesHTJour { get; set; }
    public int NombreEmployes { get; set; }
    public double SalaireMoyenMensuel { get; set; }
    public double ChargesSalarialesJour => NombreEmployes * SalaireMoyenMensuel / 30.0;
    public double ValeurAjouteeJour { get; set; }
    public double TVACollecteeJour { get; set; }
    public double TotalTVACollectee { get; set; }
    public double TotalRecettesEau { get; set; }
    public double TotalRecettesElectricite { get; set; }
    public double TotalRecettes => TotalRecettesEau + TotalRecettesElectricite;
    public double TotalConsommationMenagesKWh { get; set; }
    public double TotalConsommationEntreprisesKWh { get; set; }
    public double TotalConsommationEtatKWh { get; set; }
    public double TotalRecettesElectriciteEntreprises { get; set; }
    public double TotalRecettesElectriciteEtat { get; set; }
    public double TotalProductionKWh { get; set; }
}




