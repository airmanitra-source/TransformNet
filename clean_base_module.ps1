# Remove external dependencies from Company.Module to make it the base.

function Clean-CompanyModule {
    Get-ChildItem -Path "Company.Module\Models" -Filter "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        
        # Change namespace for moved files if any (but I moved them to Company.Module.Models)
        $content = $content -replace 'namespace\s+Household.Module.Models;', 'namespace Company.Module.Models;'
        $content = $content -replace 'namespace\s+Government.Module.Models;', 'namespace Company.Module.Models;'
        
        # Remove usings to other modules
        $content = $content -replace 'using\s+Household.Module.Models;[ \t\r\n]*', ''
        $content = $content -replace 'using\s+Government.Module.Models;[ \t\r\n]*', ''
        
        Set-Content $_.FullName $content
    }
}

Clean-CompanyModule
