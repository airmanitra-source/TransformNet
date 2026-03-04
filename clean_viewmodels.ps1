param($path)
Get-ChildItem -Path $path -Filter "*.cs" -Recurse | ForEach-Object {
    $c = Get-Content $_.FullName -Raw
    $c = $c -replace 'ViewModel.Module', 'Module'
    $c = $c -replace 'ViewModel', ''
    Set-Content $_.FullName $c
}
