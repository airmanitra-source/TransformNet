# 🔧 Guide technique - Implémentation des prix et élasticité

## Vue d'ensemble architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    EconomicSimulatorViewModel                    │
│  (Orchestrateur principal de la simulation économique)           │
└─────────────────────────────────────────────────────────────────┘
    ↓                    ↓                    ↓                    ↓
┌──────────────┐  ┌───────────────┐  ┌─────────────┐  ┌──────────────┐
│IPriceModule  │  │IHouseholdMod. │  │ICompanyMod. │  │IGovernmentM.│
│(Élasticité)  │  │(Panier alim.) │  │(Secteurs)   │  │(Fiscalité)   │
└──────────────┘  └───────────────┘  └─────────────┘  └──────────────┘
    ↓                    ↓
┌──────────────────────────────────────────────────────────┐
│ ScenarioConfigViewModel                                  │
│ (Paramètres: ElasticitePrixParCarburant, etc.)          │
└──────────────────────────────────────────────────────────┘
```

## Flux de simulation - Ajustement de prix

### Étape 1 : Initialisation (Program.cs)
```csharp
// Enregistrement des modules comme services scoped
builder.Services.AddScoped<IPriceModule, PriceModule>();
builder.Services.AddScoped<IHouseholdModule, HouseholdModule>();
```

### Étape 2 : Injection dans EconomicSimulatorViewModel
```csharp
private readonly IPriceModule _priceModule;
private readonly IHouseholdModule _householdModule;

public EconomicSimulatorViewModel(
    IPriceModule priceModule,
    IHouseholdModule householdModule)
{
    _priceModule = priceModule;
    _householdModule = householdModule;
}
```

### Étape 3 : Appel lors de SimulerUnJour()
```csharp
public void SimulerUnJour()
{
    // ... code existant ...
    
    // 1. Ajustement prix du jour (carburant → prix locaux)
    double prixMarchangiseAjuste = _priceModule.AjusterPrixParCarburant(
        prixBase: prixMarchandijeReference,
        prixCarburantCourant: _config.PrixCarburantLitre,
        prixCarburantReference: _config.PrixCarburantReference,
        elasticitePrix: _config.ElasticitePrixParCarburant,
        volatiliteAlea: _config.VolatiliteAleatoireMarche,
        random: _random
    );
    
    // 2. Achat de produits alimentaires par ménages
    foreach (var menage in _menages)
    {
        var (coutTotal, coutInf, coutFor, quantiteReduite) = 
            _householdModule.AcheteProduitsAlimentaires(
                depenseAlimentairesJourBase: menage.DepenseAlimentairesJour,
                cumulHaussePrixAlimentaire: menage.CumulHaussePrix,
                elasticiteUtilisateur: _config.ElasticiteComportementMenage,
                revenuDisponible: menage.RevenuJour
            );
        
        menage.DepensesAlimentairesEffectives = coutTotal;
        menage.QuantiteAlimentairesReduite = quantiteReduite;
        
        // Mise à jour cumul pour jour suivant
        menage.CumulHaussePrix += PercentageHausse(coutTotal, menage.DepenseAlimentairesJour);
    }
    
    // ... suite simulation ...
}
```

## IPriceModule - Interface publique

```csharp
public interface IPriceModule
{
    /// <summary>
    /// Calcule le prix ajusté d'une marchandise selon variation carburant.
    /// Applique: Prix = Base × (1 + ε × ΔCarburant/Ref) × (1 + aléa)
    /// </summary>
    double AjusterPrixParCarburant(
        double prixBase,
        double prixCarburantCourant,
        double prixCarburantReference,
        double elasticitePrix,
        double volatiliteAlea,
        Random random
    );
    
    /// <summary>
    /// Calcule le coût du panier alimentaire avec partage informel/formel.
    /// </summary>
    double CalculerCoutPanierAlimentaire(
        double depenseAlimentairesJourBase,
        double prixCarburantCourant,
        double prixCarburantReference,
        double elasticitePrix,
        double volatiliteAlea,
        double partRevenuAlimentaire,
        Random random
    );
    
    /// <summary>
    /// Estime réduction de quantités suite aux augmentations répétitives.
    /// Retourne facteur multiplicateur [0.3, 1.0].
    /// </summary>
    double CalculerFacteurReductionQuantites(
        double cumulHaussePrix,
        double elasticiteUtilisateur
    );
}
```

## PriceModule - Implémentation

### AjusterPrixParCarburant()
```csharp
public double AjusterPrixParCarburant(
    double prixBase,
    double prixCarburantCourant,
    double prixCarburantReference,
    double elasticitePrix,
    double volatiliteAlea,
    Random random)
{
    if (prixCarburantReference <= 0) return prixBase;

    // 1. Calcul variation relative carburant
    double deltaPrix = (prixCarburantCourant - prixCarburantReference) 
                      / prixCarburantReference;

    // 2. Application élasticité
    double facteurElasticite = 1.0 + (elasticitePrix * deltaPrix);

    // 3. Aléa ~ N(0, volatiliteAlea) via Box-Muller
    double u1 = random.NextDouble();
    double u2 = random.NextDouble();
    double z = Math.Sqrt(-2.0 * Math.Log(u1)) 
             * Math.Cos(2.0 * Math.PI * u2);
    double alea = 1.0 + (volatiliteAlea * z);

    // 4. Calcul prix final avec floor
    double prixAjuste = prixBase * facteurElasticite * alea;
    return Math.Max(prixAjuste, prixBase * 0.1);
}
```

### CalculerFacteurReductionQuantites()
```csharp
public double CalculerFacteurReductionQuantites(
    double cumulHaussePrix,
    double elasticiteUtilisateur)
{
    if (cumulHaussePrix <= 0) return 1.0;

    // Courbe logistique : 1 / (1 + e^(k*(x-x0)))
    double k = 0.15 / Math.Max(elasticiteUtilisateur, 0.1);
    double x0 = 25.0;  // Point d'inflexion à 25%
    
    double facteur = 1.0 / (1.0 + Math.Exp(k * (cumulHaussePrix - x0)));
    
    // Floor biologique
    return Math.Max(facteur, 0.30);
}
```

## IHouseholdModule - Ajout à l'interface

```csharp
public interface IHouseholdModule
{
    // ... méthodes existantes ...
    
    /// <summary>
    /// Simule l'achat quotidien de produits alimentaires.
    /// - 85% informel (volatiles)
    /// - 15% formel (stables, TVA 20%)
    /// - Réduit quantités si inflation répétitive
    /// </summary>
    (double CoutTotal, double CoutInformel, double CoutFormel, double QuantiteReduite) 
        AcheteProduitsAlimentaires(
            double depenseAlimentairesJourBase,
            double cumulHaussePrixAlimentaire,
            double elasticiteUtilisateur,
            double revenuDisponible
        );
}
```

## HouseholdModule - Implémentation

```csharp
public (double CoutTotal, double CoutInformel, double CoutFormel, double QuantiteReduite)
    AcheteProduitsAlimentaires(
        double depenseAlimentairesJourBase,
        double cumulHaussePrixAlimentaire,
        double elasticiteUtilisateur,
        double revenuDisponible)
{
    // 1. Calcul facteur de réduction
    double facteurReduction = CalculerFacteurReductionQuantites(
        cumulHaussePrixAlimentaire, 
        elasticiteUtilisateur
    );

    // 2. Dépense effective après réduction
    double depenseEffective = depenseAlimentairesJourBase * facteurReduction;

    // 3. Partage informel/formel
    double coutInformel = depenseEffective * 0.85;
    double coutFormel = depenseEffective * 0.15 * 1.20;  // TVA 20%

    // 4. Total avec plafond vs revenu
    double coutTotal = Math.Min(
        coutInformel + coutFormel, 
        revenuDisponible * 0.80  // Garder 20% pour autres
    );

    return (coutTotal, coutInformel, coutFormel, facteurReduction);
}

private double CalculerFacteurReductionQuantites(
    double cumulHaussePrix,
    double elasticiteRutilisateur)
{
    if (cumulHaussePrix <= 0) return 1.0;
    
    double k = 0.15 / Math.Max(elasticiteRutilisateur, 0.1);
    double x0 = 25.0;
    
    double facteur = 1.0 / (1.0 + Math.Exp(k * (cumulHaussePrix - x0)));
    return Math.Max(facteur, 0.30);
}
```

## Paramètres ScenarioConfigViewModel

```csharp
public class ScenarioConfigViewModel
{
    /// <summary>
    /// Élasticité-prix : transmission carburant → prix locaux
    /// 0.3 = faible (commerce absorbe)
    /// 0.7 = modérée (réaliste Madagascar)
    /// 1.0 = complète (100% du choc)
    /// </summary>
    public double ElasticitePrixParCarburant { get; set; } = 0.70;

    /// <summary>
    /// Volatilité aléatoire : ±σ de variation quotidienne
    /// 0.05 = stable
    /// 0.10 = normal
    /// 0.15 = très volatile
    /// </summary>
    public double VolatiliteAleatoireMarche { get; set; } = 0.10;

    /// <summary>
    /// Part du revenu mensuel consacrée à l'alimentation
    /// 0.20 = riches (20%)
    /// 0.40 = pauvres (40%)
    /// 0.60 = ultra-pauvres (60%)
    /// </summary>
    public double PartRevenuAlimentaire { get; set; } = 0.40;

    /// <summary>
    /// Élasticité du comportement du ménage face aux prix
    /// 0.3 = peu sensible (accepte augmentations)
    /// 0.65 = normal (réaliste)
    /// 1.0 = très sensible (réduit vite quantités)
    /// </summary>
    public double ElasticiteComportementMenage { get; set; } = 0.65;

    /// <summary>
    /// Prix de référence du carburant pour calibrage
    /// Typiquement le prix initial de simulation
    /// </summary>
    public double PrixCarburantReference { get; set; } = 5_500;
}
```

## Tests unitaires - Exemples

```csharp
[TestClass]
public class PriceModuleTests
{
    private PriceModule _priceModule;
    private Random _random;

    [TestInitialize]
    public void Setup()
    {
        _priceModule = new PriceModule();
        _random = new Random(42);
    }

    [TestMethod]
    public void AjusterPrixParCarburant_SansCrocCarburant_RetornePrixBase()
    {
        double prixBase = 1000;
        double resultat = _priceModule.AjusterPrixParCarburant(
            prixBase: prixBase,
            prixCarburantCourant: 5500,
            prixCarburantReference: 5500,
            elasticitePrix: 0.70,
            volatiliteAlea: 0.10,
            random: _random
        );
        
        // Résultat proche de prixBase (±aléa)
        Assert.IsTrue(resultat > 900 && resultat < 1100);
    }

    [TestMethod]
    public void AjusterPrixParCarburant_AvecCrocPlus10Pourcent_RetornePlusElevé()
    {
        double prixBase = 1000;
        double resultat = _priceModule.AjusterPrixParCarburant(
            prixBase: prixBase,
            prixCarburantCourant: 6050,      // +10%
            prixCarburantReference: 5500,
            elasticitePrix: 0.70,
            volatiliteAlea: 0.0,             // Pas d'aléa
            random: new Random(42)
        );
        
        // Attendu : 1000 × (1 + 0.70 × 0.10) = 1070
        Assert.AreEqual(1070, resultat, 1.0);
    }

    [TestMethod]
    public void CalculerFacteurReductionQuantites_CumulHausse25Pourcent_RetorneEnviron50Pourcent()
    {
        double resultat = _priceModule.CalculerFacteurReductionQuantites(
            cumulHaussePrix: 25.0,
            elasticiteUtilisateur: 0.65
        );
        
        // Point d'inflexion : environ 0.5
        Assert.IsTrue(resultat > 0.4 && resultat < 0.6);
    }
}
```

## Intégration avec EconomicSimulatorViewModel

### À faire

1. **Ajouter injections dans constructeur**
   ```csharp
   public EconomicSimulatorViewModel(
       IPriceModule priceModule,
       IHouseholdModule householdModule,
       // ... autres modules ...
   )
   ```

2. **Appeler ajustement dans SimulerUnJour()**
   ```csharp
   // Pour chaque ménage
   var (coutTotal, _, _, quantiteReduite) = _householdModule.AcheteProduitsAlimentaires(
       menage.DepenseAlimentairesJourBase,
       menage.CumulHaussePrix,
       _config.ElasticiteComportementMenage,
       menage.RevenuJour
   );
   ```

3. **Stocker résultats dans DailySnapshotViewModel**
   ```csharp
   snapshot.CoutMoyenPanierAlimentaire = coutMoyenTotal;
   snapshot.FacteurMoyenReductionQuantites = quantiteMoyenneReduite;
   snapshot.CumulHaussePrixMoyen = cumulHausseMoyen;
   ```

4. **Ajouter sliders UI dans EconomySimulator.razor**
   ```razor
   <div class="mb-3">
       <label>Élasticité prix/carburant : @_config.ElasticitePrixParCarburant.ToString("F2")</label>
       <input type="range" class="form-range" 
              min="0.3" max="1.0" step="0.05"
              @bind="_config.ElasticitePrixParCarburant" />
   </div>
   ```

## Performance & optimisation

- **Aléa Gaussienne** : Box-Muller (O(1), rapide)
- **Logistique** : Calcul O(1), pas de boucle
- **Parallélisation possible** : Appel par ménage peut être parallélisé via PLINQ

```csharp
// Optimization future
var resultats = _menages.AsParallel()
    .Select(m => _householdModule.AcheteProduitsAlimentaires(...))
    .ToList();
```

## Débogage

### Tracer un choc de prix
```csharp
var prixInitial = 1000;
var prixCarburant = 5500;

for (int jour = 0; jour < 365; jour++)
{
    prixCarburant += 5;  // +5 MGA/jour
    var prixAjuste = _priceModule.AjusterPrixParCarburant(
        prixInitial, prixCarburant, 5500, 0.70, 0.10, _random
    );
    Console.WriteLine($"Jour {jour}: Carburant={prixCarburant}, Prix={prixAjuste:F0}");
}
```

### Valider convergence inflation
```csharp
// Vérifier que inflation cumulative correspond aux paramètres
double inflationCalculée = (prixFinal - prixInitial) / prixInitial;
double inflationAttendueMin = 0.70 * (crocCarburant * 0.9);  // -10% aléa
double inflationAttendueMax = 0.70 * (crocCarburant * 1.1);  // +10% aléa

Assert.IsTrue(
    inflationCalculée >= inflationAttendueMin && 
    inflationCalculée <= inflationAttendueMax
);
```

---

**Document technique** : Implémentation prix/élasticité  
**Dernière mise à jour** : 2024  
**Status** : ✅ Modules créés, ⏳ Intégration EconomicSimulator
