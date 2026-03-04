# This script suffixes classes with ViewModel and fixes namespaces in Web project.
# It excludes classes that already have the suffix.

Get-ChildItem -Path MachineLearning.Web\Models -Recurse -Filter "*.cs" | ForEach-Object {
    $c = Get-Content $_.FullName -Raw
    
    # 1. Fix Namespaces: MachineLearning.Web.Models.<FolderStructure>
    $relativePath = $_.DirectoryName.Replace((Get-Item "MachineLearning.Web\Models").FullName, "").Replace("\", ".").TrimStart(".")
    $newNamespace = "MachineLearning.Web.Models"
    if ($relativePath -ne "") { $newNamespace += ".$relativePath" }
    $c = $c -replace '^namespace .*;', "namespace $newNamespace;"

    # 2. Suffix class/enum names if missing
    # We look for 'public class X', 'public enum X', 'public record X'
    # Use regex to find names and append ViewModel if not present.
    $types = "class", "enum", "record"
    foreach ($type in $types) {
        $regex = "(public|private|protected|internal)\s+$type\s+([a-zA-Z0-9_]+)"
        $matches = [regex]::Matches($c, $regex)
        foreach ($m in $matches) {
            $name = $m.Groups[2].Value
            if ($name -notlike "*ViewModel") {
                $newName = $name + "ViewModel"
                # Use word boundary to replace only the type name
                $c = $c -replace "\b$name\b", $newName
            }
        }
    }
    
    Set-Content $_.FullName $c
}
