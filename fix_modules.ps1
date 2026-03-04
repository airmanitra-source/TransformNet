# Fix modules: Correct namespaces and class names (remove ViewModel suffix if accidentally added to logic classes)
# Then ensure they reference each other correctly.

function Fix-Module {
    param($moduleName)
    $path = "$moduleName.Module\Models"
    if (!(Test-Path $path)) { return }

    Get-ChildItem -Path $path -Filter "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        
        # 1. Fix namespace
        $content = $content -replace 'namespace\s+[^;]+;', "namespace $moduleName.Module.Models;"
        
        # 2. Add standard usings for other modules if needed (brute force for now)
        if ($moduleName -ne "Company") {
            if ($content -notmatch "using Company.Module.Models;") {
                $content = "using Company.Module.Models;`n" + $content
            }
        }
        if ($moduleName -ne "Household") {
            if ($content -notmatch "using Household.Module.Models;") {
                $content = "using Household.Module.Models;`n" + $content
            }
        }
        if ($moduleName -ne "Government") {
            if ($content -notmatch "using Government.Module.Models;") {
                $content = "using Government.Module.Models;`n" + $content
            }
        }

        # 3. Final cleanup of class names (remove ViewModel suffix if it crept in)
        $content = $content -replace '\b([a-zA-Z0-9_]+)ViewModel\b', '$1'

        Set-Content $_.FullName $content
    }
}

Fix-Module "Household"
Fix-Module "Company"
Fix-Module "Government"
