FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NPOI-Excel-Operation-Web/NPOI-Excel-Operation-Web.csproj", "NPOI-Excel-Operation-Web/"]
RUN dotnet restore "NPOI-Excel-Operation-Web/NPOI-Excel-Operation-Web.csproj"
COPY . .
WORKDIR "/src/NPOI-Excel-Operation-Web"
RUN dotnet build "NPOI-Excel-Operation-Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NPOI-Excel-Operation-Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NPOI-Excel-Operation-Web.dll"]
