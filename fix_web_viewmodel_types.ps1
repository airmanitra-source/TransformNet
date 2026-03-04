# Fix property types in Web ViewModels: ESecteurActivite -> ESecteurActiviteViewModel etc.

$path = "MachineLearning.Web\Models"
Get-ChildItem -Path $path -Recurse -Filter "*ViewModel.cs" | ForEach-Object {
    $c = Get-Content $_.FullName -Raw
    
    $c = $c -replace '\bESecteurActivite\b', 'ESecteurActiviteViewModel'
    $c = $c -replace '\bECategorieImport\b', 'ECategorieImportViewModel'
    $c = $c -replace '\bECategorieExport\b', 'ECategorieExportViewModel'
    $c = $c -replace '\bClasseSocioEconomique\b', 'ClasseSocioEconomiqueViewModel'
    $c = $c -replace '\bModeTransport\b', 'ModeTransportViewModel'
    
    Set-Content $_.FullName $c
}
