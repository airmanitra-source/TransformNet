# Strip methods from ViewModels in Web project to make them simple data containers.
# Exclude EconomicSimulatorViewModel as it's the main controller.

function Cleanup-ViewModel {
    param($file)
    $content = Get-Content $file -Raw
    
    # Very rough regex to remove methods: public void|double|List etc (...) { ... }
    # This is dangerous. Let's try instead to find all properties and reconstruct the class.
    
    $namespace = [regex]::Match($content, 'namespace\s+[^;{]+[;{]').Value
    $className = [regex]::Match($content, 'public\s+(class|enum|record)\s+([a-zA-Z0-9_]+)').Value
    
    $properties = [regex]::Matches($content, 'public\s+(?!class|enum|record)([a-zA-Z0-9_<>?.,]+\s+[a-zA-Z0-9_]+\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*[^;]+;)?|static\s+readonly\s+[^;]+;|[a-zA-Z0-9_]+\s+[a-zA-Z0-9_]+\s*=>\s*[^;]+;)')
    
    $newContent = $namespace + "`n`n" + $className + "`n{`n"
    foreach ($p in $properties) {
        $newContent += "    " + $p.Value + "`n"
    }
    $newContent += "}`n"
    
    Set-Content $file $newContent
}

# Apply to Agents ViewModels
Get-ChildItem -Path "MachineLearning.Web\Models\Agents" -Recurse -Filter "*ViewModel.cs" | ForEach-Object {
    if ($_.Length -gt 0) {
        Cleanup-ViewModel $_.FullName
    }
}

# Apply to Simulation Result ViewModels (except the simulator itself)
Get-ChildItem -Path "MachineLearning.Web\Models\Simulation" -Recurse -Filter "*ViewModel.cs" | ForEach-Object {
    if ($_.Name -ne "EconomicSimulatorViewModel.cs" -and $_.Length -gt 0) {
        Cleanup-ViewModel $_.FullName
    }
}
