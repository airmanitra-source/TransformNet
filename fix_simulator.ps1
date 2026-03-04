# Fix EconomicSimulatorViewModel.cs:
# 1. Aliases for Module Agent classes
# 2. Map Config ViewModels properties to logic types if needed (casts)

$file = "MachineLearning.Web\Models\Simulation\EconomicSimulatorViewModel.cs"
$c = Get-Content $file -Raw

# Correcting using blocks
$c = $c -replace 'using AgentCompany = Company\.Module\.Models\.Company;', 'using AgentCompany = Company.Module.Models.Company;'
# No changes needed if already there, but let's ensure we have all needed aliases

$aliases = @"
using AgentCompany = Company.Module.Models.Company;
using AgentHousehold = Household.Module.Models.Household;
using AgentGovernment = Government.Module.Models.Government;
using AgentImporter = Company.Module.Models.Importer;
using AgentExporter = Company.Module.Models.Exporter;
using AgentJirama = Company.Module.Models.Jirama;
using ESecteurActivite = Company.Module.Models.ESecteurActivite;
using ECategorieImport = Company.Module.Models.ECategorieImport;
using ECategorieExport = Company.Module.Models.ECategorieExport;
using ClasseSocioEconomique = Company.Module.Models.ClasseSocioEconomique;
using ModeTransport = Company.Module.Models.ModeTransport;
"@

# Fix Agent instantiations throughout the file
$c = $c -replace 'new Household\b', 'new AgentHousehold'
$c = $c -replace 'new Company\b', 'new AgentCompany'
$c = $c -replace 'new Importer\b', 'new AgentImporter'
$c = $c -replace 'new Exporter\b', 'new AgentExporter'
$c = $c -replace 'new Jirama\b', 'new AgentJirama'
$c = $c -replace 'new Government\b', 'new AgentGovernment'

# Fix type references in private fields and loops
$c = $c -replace 'List<Household>', 'List<AgentHousehold>'
$c = $c -replace 'List<Company>', 'List<AgentCompany>'
$c = $c -replace 'List<Importer>', 'List<AgentImporter>'
$c = $c -replace 'List<Exporter>', 'List<AgentExporter>'
$c = $c -replace 'Dictionary<int, Company>', 'Dictionary<int, AgentCompany>'

# Fix static calls
$c = $c -replace '\bHousehold\.ResetIdCounter', 'AgentHousehold.ResetIdCounter'
$c = $c -replace '\bCompany\.ResetIdCounter', 'AgentCompany.ResetIdCounter'
$c = $c -replace '\bCompany\.GetProductiviteParSecteur', 'AgentCompany.GetProductiviteParSecteur'
$c = $c -replace '\bCompany\.GetTresorerieInitialeParSecteur', 'AgentCompany.GetTresorerieInitialeParSecteur'

# Fix Config property mismatches (mapping ViewModel enums to Module enums via cast)
$c = $c -replace '_config\.TresorerieInitialeParSecteur', '(_config.TresorerieInitialeParSecteur.ToDictionary(k => (ESecteurActivite)k.Key, v => v))'
$c = $c -replace '_config\.CIFJourParCategorie', '(_config.CIFJourParCategorie.ToDictionary(k => (ECategorieImport)k.Key, v => v))'
$c = $c -replace '_config\.FOBJourParCategorie', '(_config.FOBJourParCategorie.ToDictionary(k => (ECategorieExport)k.Key, v => v))'
$c = $c -replace '_config\.CIFCalibresJour', '(_config.CIFCalibresJour.ToDictionary(k => (ECategorieImport)k.Key, v => v))'
$c = $c -replace '_config\.FOBCalibresJour', '(_config.FOBCalibresJour.ToDictionary(k => (ECategorieExport)k.Key, v => v))'

Set-Content $file $c
