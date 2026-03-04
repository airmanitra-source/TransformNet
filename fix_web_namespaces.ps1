param($path)
Get-ChildItem -Path $path -Filter "*ViewModel.cs" -Recurse | ForEach-Object {
    $relativePath = $_.DirectoryName.Replace((Get-Item .).FullName, "").Replace("\", ".").TrimStart(".")
    $newNamespace = "MachineLearning.Web.Models." + $relativePath.Replace("MachineLearning.Web.Models.", "")
    
    $c = Get-Content $_.FullName -Raw
    # If the class name doesn't have ViewModel suffix, add it (should be there already but just in case)
    # But wait, we want to make sure they reference the Module models if needed.
    
    # For now, just fix the namespace
    $c = $c -replace '^namespace .*;', "namespace $newNamespace;"
    Set-Content $_.FullName $c
}
