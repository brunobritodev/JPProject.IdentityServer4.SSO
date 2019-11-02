Set-Location ../src/Frontend/Jp.UI.SSO

Copy-Item appsettings.json -Destination appsettings-back.json

# SQL Server Migration
$settings = Get-Content 'appsettings.json' -raw
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "SqlServer",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add Initial -c EventStoreContext -o Migrations/SqlServer/EventStore
dotnet ef migrations add Initial -c ApplicationSsoContext -o Migrations/SqlServer/SSO

# MySql Migration
$settings = Get-Content 'appsettings.json' -raw
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "MySql",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add Initial -c EventStoreContext -o Migrations/MySql/EventStore
dotnet ef migrations add Initial -c ApplicationSsoContext -o Migrations/MySql/SSO

# Postgre Migration
$settings = Get-Content 'appsettings.json' -raw
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Postgre",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add Initial -c EventStoreContext -o Migrations/Postgre/EventStore
dotnet ef migrations add Initial -c ApplicationSsoContext -o Migrations/Postgre/SSO

# Sqlite Migration
$settings = Get-Content 'appsettings.json' -raw
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Sqlite",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add Initial -c EventStoreContext -o Migrations/Sqlite/EventStore
dotnet ef migrations add Initial -c ApplicationSsoContext -o Migrations/Sqlite/SSO