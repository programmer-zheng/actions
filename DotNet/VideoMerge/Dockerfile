FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VideoMerge.csproj", "./"]
RUN dotnet restore "VideoMerge.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "VideoMerge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoMerge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoMerge.dll"]
