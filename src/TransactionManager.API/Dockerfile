﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TransactionManager.API/TransactionManager.API.csproj", "src/TransactionManager.API/"]
COPY ["src/TransactionManager.Data/TransactionManager.Data.csproj", "src/TransactionManager.Data/"]
COPY ["src/TransactionManager.Logic/TransactionManager.Logic.csproj", "src/TransactionManager.Logic/"]
COPY ["tests/TransactionManager.IntegrationTests/TransactionManager.IntegrationTests.csproj", "tests/TransactionManager.IntegrationTests/"]
RUN dotnet restore "src/TransactionManager.API/TransactionManager.API.csproj"
COPY . .
WORKDIR "/src/src/TransactionManager.API"
RUN dotnet build "TransactionManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TransactionManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionManager.API.dll"]
