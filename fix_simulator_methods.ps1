$file = "MachineLearning.Web\Models\Simulation\EconomicSimulatorViewModel.cs"
$c = Get-Content $file -Raw

$c = $c -replace 'List<CompanyViewModel>', 'List<AgentCompany>'
$c = $c -replace 'List<HouseholdViewModel>', 'List<AgentHousehold>'
$c = $c -replace 'List<ImporterViewModel>', 'List<AgentImporter>'
$c = $c -replace 'List<ExporterViewModel>', 'List<AgentExporter>'

Set-Content $file $c
