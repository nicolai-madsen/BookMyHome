FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/BookMyHome.Api/BookMyHome.Api.csproj src/BookMyHome.Api/
COPY src/BookMyHome.Domain/BookMyHome.Domain.csproj src/BookMyHome.Domain/
COPY src/BookMyHome.Persistence/BookMyHome.Persistence.csproj src/BookMyHome.Persistence/
COPY src/BookMyHome.Shared/BookMyHome.Shared.csproj src/BookMyHome.Shared/

RUN dotnet restore src/BookMyHome.Api/BookMyHome.Api.csproj

COPY src/ src/
RUN dotnet publish src/BookMyHome.Api/BookMyHome.Api.csproj \
    -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookMyHome.Api.dll"]