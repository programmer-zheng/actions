FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
# COPY ["DotnetCoreMvcTestGithubActions/DotnetCoreMvcTestGithubActions.csproj", "DotnetCoreMvcTestGithubActions/"]
COPY . .
RUN dotnet restore "DotnetCoreMvcTestGithubActions/DotnetCoreMvcTestGithubActions.csproj"
# WORKDIR "/src/DotnetCoreMvcTestGithubActions"
RUN dotnet build "DotnetCoreMvcTestGithubActions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotnetCoreMvcTestGithubActions.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotnetCoreMvcTestGithubActions.dll"]
