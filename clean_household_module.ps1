# Ensure Household.Module only depends on Company.Module

function Clean-HouseholdModule {
    Get-ChildItem -Path "Household.Module\Models" -Filter "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        
        # Ensure correct using for Company (where Results/Enums now live)
        if ($content -notmatch "using Company.Module.Models;") {
            $content = "using Company.Module.Models;`n" + $content
        }
        
        # Remove using to Government
        $content = $content -replace 'using\s+Government.Module.Models;[ \t\r\n]*', ''
        
        Set-Content $_.FullName $content
    }
}

Clean-HouseholdModule
