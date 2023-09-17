FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* 
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["VideoMerge.csproj", "./"]
RUN dotnet restore "VideoMerge.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "VideoMerge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoMerge.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN sed -i 's/"BaseDirectory"[^,\n]*/"BaseDirectory":"\/app\/videos"/g' /app/publish/appsettings.json 


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoMerge.dll"]
