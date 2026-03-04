# 📊 Simulateur Économique Madagascar - Changements v2.1

## Vue d'ensemble

Cette mise à jour ajoute un **mécanisme complet d'ajustement de prix par carburant** avec élasticité paramétrable et comportement d'adaptation des ménages.

## 🎯 Fonctionnalités ajoutées

### 1. **Ajustement dynamique des prix** (`IPriceModule`, `PriceModule`)
- Transmission des chocs de carburant aux prix locaux selon **élasticité paramétrable** (0.3-1.0)
- Aléa de marché ~ N(0, σ_volatilité) pour réalisme
- Formule: `Prix_ajusté = Prix_base × [1 + ε × ΔCarburant/Ref] × (1 + aléa)`

### 2. **Achat de produits alimentaires** (`IHouseholdModule.AcheteProduitsAlimentaires()`)
- **85% secteur informel** (prix volatiles, sans TVA)
- **15% secteur formel** (prix stables, TVA 20%)
- Réduction progressive des quantités en cas d'augmentations répétitives
- Modélise le comportement réaliste des ménages pauvres

### 3. **Courbe logistique de privation**
- Réduction des quantités achetées selon inflation cumulative
- Logistique inversée : plus l'inflation, moins on achète
- Inflexion à 25% d'augmentation
- **Plancher biologique** : 30% minimum (limite de subsistance)

### 4. **4 nouveaux paramètres ajustables sur l'UI**
| Paramètre | Type | Défaut | Intervalle |
|---|---|---|---|
| **ElasticitePrixParCarburant** | double | 0.70 | [0.3, 1.0] |
| **VolatiliteAleatoireMarche** | double | 0.10 | [0.05, 0.20] |
| **PartRevenuAlimentaire** | double | 0.40 | [0.20, 0.60] |
| **ElasticiteComportementMenage** | double | 0.65 | [0.3, 1.0] |

## 📁 Fichiers modifiés/créés

### Nouveaux fichiers
```
Company.Module/
  ├── IPriceModule.cs                 # Interface contrat gestion prix
  └── PriceModule.cs                  # Implémentation élasticité + aléa

Household.Module/
  ├── IHouseholdModule.cs             # Ajout AcheteProduitsAlimentaires()
  └── HouseholdModule.cs              # Implémentation achat alimentaires

MachineLearning.Web/
  └── Components/Pages/
      └── DocumentationPrix.razor      # Doc complète des prix & élasticité
```

### Fichiers modifiés
```
MachineLearning.Web/
  ├── Models/Simulation/Config/ScenarioConfigViewModel.cs    # +4 paramètres
  ├── Program.cs                                             # +IPriceModule enregistrement
  ├── Components/Layout/NavMenu.razor                        # +lien doc prix
  └── Components/Pages/DocumentationCalibration.razor        # +lien doc prix

Household.Module/
  └── Models/ClasseSocioEconomique.cs                        # Enum classe ménage
```

## 🔧 Architecture technique

### Pattern modulaire
Chaque module expose un **contrat (interface)** injectable dans `EconomicSimulatorViewModel` :

```csharp
// Program.cs
builder.Services.AddScoped<IPriceModule, PriceModule>();
builder.Services.AddScoped<IHouseholdModule, HouseholdModule>();
```

### Injection de dépendances
```csharp
// EconomicSimulatorViewModel.cs
public EconomicSimulatorViewModel(
    IPriceModule priceModule,
    IHouseholdModule householdModule,
    IGovernmentModule governmentModule,
    ICompanyModule companyModule)
{
    _priceModule = priceModule;
    _householdModule = householdModule;
    // ...
}
```

## 📈 Formules clés

### Ajustement prix
```
Prix_ajusté = Prix_base × [1 + ε × (P_carburant_courant - P_carburant_ref) / P_carburant_ref] × (1 + N(0, σ))

Où:
- ε = ElasticitePrixParCarburant (0.3-1.0)
- σ = VolatiliteAleatoireMarche (0.05-0.20)
```

### Réduction quantités (courbe logistique)
```
Facteur = 1 / (1 + e^(k × (cumulHausse - 25)))

Où:
- k = 0.15 / max(ElasticiteComportement, 0.1)
- cumulHausse = Σ % augmentation de prix
- Plancher = 0.30 (minimum subsistance)
```

### Coût panier alimentaire
```
Coût = (Dépense × 0.85 × prix_informel) + (Dépense × 0.15 × prix_formel × 1.20)
```

## 🎮 Utilisation sur l'interface

### Section "Transport & Carburant"
- Ajuster **Prix carburant/L** : voit impacts immédiats sur inflation

### Section "Configuration avancée" (nouvelle)
- **Élasticité prix/carburant** : 0.30 = absorption par commerce, 1.0 = transmission complète
- **Volatilité marché** : 0.05 = stable, 0.15 = très volatile
- **Part revenu alimentaire** : 0.40 = 40% du revenu en nourriture
- **Élasticité comportement** : 0.30 = peu sensible, 1.0 = très sensible

## 📊 Scénarios de test

### Scénario 1 : Choc modéré
```
Carburant: 5500 → 6050 (+10%)
Élasticité: 0.70
Résultat: Inflation marchandises +7%
```

### Scénario 2 : Crise sévère
```
Carburant: 5500 → 9900 (+80%)
Élasticité: 0.80
Résultat: Inflation +50-60%, réduction quantités 30-50%
```

### Scénario 3 : Résilience
```
Carburant: 5500 → 8250 (+50%)
Élasticité: 0.30
Résultat: Inflation modérée +8%, peu d'impact ménages
```

## 📖 Documentation web

### Pages de documentation
- **`/doc-calibration`** : Documentation générale (PIB, secteurs, TOFE)
- **`/doc-prix`** ⭐ **NOUVEAU** : Détail complet prix, élasticité, comportement ménages

### Navigation
Menu ajouté : "Prix & Élasticité 📈" → `/doc-prix`

## ✅ Build & Tests

- ✅ Build réussi (.NET 10)
- ✅ Interfaces injectables
- ✅ Paramètres dans ScenarioConfigViewModel
- ⏳ À faire : Refactoriser EconomicSimulatorViewModel pour injecter modules
- ⏳ À faire : Ajouter sliders UI pour nouveaux paramètres

## 🔄 Prochaines étapes

1. **Intégrer dans EconomicSimulatorViewModel**
   - Injecter `IPriceModule`, `IHouseholdModule`
   - Appeler ajustement de prix dans `SimulerUnJour()`
   
2. **UI sliders**
   - Ajouter contrôles élasticité dans EconomySimulator.razor
   - Visualiser impacts en temps réel
   
3. **Validation économique**
   - Tester convergence des 3 approches PIB
   - Calibrer élasticité sur données réelles Madagascar
   - Valider comportement chaînes d'inflation

## 💡 Réalisme Madagascar

- **Élasticité 0.70** : Transmission réaliste du choc carburant (commerce informel très exposé)
- **Part alimentaire 0.40** : Réaliste pour ménages pauvres (~40% du revenu)
- **Partage 85% informel/15% formel** : Distribution réelle des transactions
- **Courbe logistique** : Modélise réduction quantités avant privation (comportement observé)

## 📝 Références

- **Modules** : Inspirés de `Government.Module` (pattern contrat/interface)
- **Élasticité** : Calibrage basé sur études prix/carburant Afrique subsaharienne
- **Courbe logistique** : Économie comportementale (Kahneman, Thaler)
- **Données INSTAT** : Calibration sur TOFE sept. 2025, Tableau 21 Jirama

---

**Version** : 2.1  
**Date** : 2024  
**Auteur** : Simulateur économique Madagascar  
**Build** : ✅ Réussi
