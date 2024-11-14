#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 8080 443
ENTRYPOINT ["dotnet", "Abp.Ddns.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /src
COPY ["Abp.Ddns.csproj", "."]
RUN dotnet restore "./Abp.Ddns.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "Abp.Ddns.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .