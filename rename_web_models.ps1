Get-ChildItem -Path MachineLearning.Web\Models -Recurse -Filter "*.cs" | ForEach-Object {
    if ($_.Name -notlike "*ViewModel.cs") {
        $newName = $_.Name.Replace(".cs", "ViewModel.cs")
        $newPath = Join-Path $_.DirectoryName $newName
        Rename-Item $_.FullName $newName
    }
}
