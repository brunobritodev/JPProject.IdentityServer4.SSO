FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Backend/Jp.Api.Management/Jp.Api.Management.csproj", "Backend/Jp.Api.Management/"]
RUN dotnet restore "Backend/Jp.Api.Management/Jp.Api.Management.csproj"
COPY src/ .
WORKDIR "/src/Backend/Jp.Api.Management"
RUN dotnet build "Jp.Api.Management.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Jp.Api.Management.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Jp.Api.Management.dll"]