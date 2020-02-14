param([string] $migrations = 'Initial')
$currentPath = Get-Location
Set-Location ../src/Frontend/Jp.UI.SSO

Copy-Item appsettings.json -Destination appsettings-back.json
$settings = Get-Content appsettings.json -raw

# SQL Server Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "SqlServer",'
$settings | set-content appsettings.json

dotnet ef migrations add $migrations'SqlServer' -c ApplicationSsoContext -o Migrations/SqlServer/Identity  -p ..\..\Backend\Jp.Database\Jp.Database.csproj

# MySql Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "MySql",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations'MySql' -c ApplicationSsoContext -o Migrations/MySql/Identity -p ..\..\Backend\Jp.Database\Jp.Database.csproj

# Postgre Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Postgre",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations'Postgre' -c ApplicationSsoContext -o Migrations/Postgre/Identity -p ..\..\Backend\Jp.Database\Jp.Database.csproj

# Sqlite Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Sqlite",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations'Sqlite' -c ApplicationSsoContext -o Migrations/Sqlite/Identity -p ..\..\Backend\Jp.Database\Jp.Database.csproj

Remove-Item appsettings.json
Copy-Item appsettings-back.json -Destination appsettings.json
Set-Location $currentPath