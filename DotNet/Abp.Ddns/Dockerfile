#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 8080 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Abp.Ddns.csproj", "."]
RUN dotnet restore "./Abp.Ddns.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Abp.Ddns.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Abp.Ddns.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Abp.Ddns.dll"]