param([string] $migrations = 'Initial', [string] $db = "sqlserver")
$currentPath = Get-Location
Set-Location ../src/Frontend/Jp.UI.SSO

$settings = Get-Content appsettings.json -raw

# SQL Server Migration
if ($db -eq "sqlserver") {
    $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "SqlServer",'
    $settings | set-content appsettings.json

    dotnet ef migrations add $migrations -c SsoContext  -p ..\..\Backend\Jp.Database\Jp.Database.csproj
}



# MySql Migration
if ($db -eq "mysql") {
    $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "MySql",'
    $settings | set-content 'appsettings.json'

    dotnet ef migrations add $migrations -c SsoContext -p ..\..\Backend\Jp.Database\Jp.Database.csproj
}

# Postgre Migration
if ($db -eq "postgre") {
    $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Postgre",'
    $settings | set-content 'appsettings.json'
    dotnet ef migrations add $migrations -c SsoContext -p ..\..\Backend\Jp.Database\Jp.Database.csproj
}

# Sqlite Migration

if ($db -eq "sqlite") {
    $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Sqlite",'
    $settings | set-content 'appsettings.json'
    dotnet ef migrations add $migrations -c SsoContext -p ..\..\Backend\Jp.Database\Jp.Database.csproj
}

Set-Location $currentPath
