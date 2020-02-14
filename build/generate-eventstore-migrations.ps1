param([string] $migrations = 'Initial')
$currentPath = Get-Location

# # SSO EventStore
# Set-Location ../tests/SSO.host

# Copy-Item appsettings.json -Destination appsettings-back.json
# $settings = Get-Content appsettings.json -raw

# # SQL Server Migration
# $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "SqlServer",'
# $settings | set-content appsettings.json

# dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore  -p ..\..\src\SSO\JPProject.Sso.EntityFrameworkCore.Sql\JPProject.Sso.EntityFrameworkCore.SqlServer.csproj

# # MySql Migration
# $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "MySql",'
# $settings | set-content 'appsettings.json'

# dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\SSO\JPProject.Sso.EntityFrameworkCore.MySql\JPProject.Sso.EntityFrameworkCore.MySql.csproj

# # Postgre Migration
# $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Postgre",'
# $settings | set-content 'appsettings.json'

# dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\SSO\JPProject.Sso.EntityFrameworkCore.PostgreSQL\JPProject.Sso.EntityFrameworkCore.PostgreSQL.csproj

# # Sqlite Migration
# $settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Sqlite",'
# $settings | set-content 'appsettings.json'

# dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\SSO\JPProject.Sso.EntityFrameworkCore.Sqlite\JPProject.Sso.EntityFrameworkCore.Sqlite.csproj

# Remove-Item appsettings.json
# Copy-Item appsettings-back.json -Destination appsettings.json
# Set-Location $currentPath

# Admin-UI API
Set-Location ../tests/Admin.host

Copy-Item appsettings.json -Destination appsettings-back.json
$settings = Get-Content appsettings.json -raw

# SQL Server Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "SqlServer",'
$settings | set-content appsettings.json

dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore  -p ..\..\src\Admin\JPProject.Admin.EntityFrameworkCore.Sql\JPProject.Admin.EntityFrameworkCore.Sql.csproj

# MySql Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "MySql",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\Admin\JPProject.Admin.EntityFrameworkCore.MySql\JPProject.Admin.EntityFrameworkCore.MySql.csproj

# Postgre Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Postgre",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\Admin\JPProject.Admin.EntityFrameworkCore.PostgreSQL\JPProject.Admin.EntityFrameworkCore.PostgreSQL.csproj

# Sqlite Migration
$settings = $settings -replace '"DatabaseType".*', '"DatabaseType": "Sqlite",'
$settings | set-content 'appsettings.json'

dotnet ef migrations add $migrations -c EventStoreContext -o Migrations/EventStore -p ..\..\src\Admin\JPProject.Admin.EntityFrameworkCore.Sqlite\JPProject.Admin.EntityFrameworkCore.Sqlite.csproj

Remove-Item appsettings.json
Copy-Item appsettings-back.json -Destination appsettings.json
Set-Location $currentPath