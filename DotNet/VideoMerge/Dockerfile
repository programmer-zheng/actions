FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

########## 方式一：安装解压工具，解压文件后，复制相关文件到指定目录，移除解压目录和源文件 ##########
## 安装 xz-utils 以支持 .tar.xz 文件解压
#RUN apt-get update && apt-get install -y xz-utils \
#    && apt-get clean && rm -rf /var/lib/apt/lists/* 

## 拷贝本地ffmpeg文件包到基础镜像中，解压后拷贝至相关目录
#COPY ffmpeg-release-amd64-static.tar.xz .
#RUN tar -xvf ffmpeg-release-amd64-static.tar.xz \
#    && cp ffmpeg-*-static/ffmpeg /usr/local/bin/ffmpeg \
#    && rm -rf ffmpeg-release-amd64-static.tar.xz ffmpeg-*-static

########## 方式二：通过ADD指令，添加压缩包会自动解压，拷贝文件到相关目录，移除解压目录和源文件 ##########
ADD ffmpeg-release-amd64-static.tar.xz /
RUN cp /ffmpeg-*-static/ffmpeg /usr/local/bin/ffmpeg \
    && rm -rf /ffmpeg-release-amd64-static.tar.xz /ffmpeg-*-static

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VideoMerge.csproj", "./"]
RUN dotnet restore "VideoMerge.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "VideoMerge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoMerge.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN sed -i 's/"BaseDirectory"[^,\n]*/"BaseDirectory":"\/app\/videos"/g' /app/publish/appsettings.json 
# 修改日志级别
RUN sed -i 's/"Default"[^,\n]*/"Default":"Information"/g' /app/publish/appsettings.json 


FROM base AS final
RUN echo "Asia/Shanghai" > /etc/timezone
WORKDIR /app
COPY --from=publish /app/publish .
ENV TZ=Asia/Shanghai
ENTRYPOINT ["dotnet", "VideoMerge.dll"]
