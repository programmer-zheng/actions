# Furion DEMO
## 项目初始化
### 脚手架安装
[furion--sqlsugar-脚手架安装](https://furion.net/docs/template#2722-furion--sqlsugar-%E8%84%9A%E6%89%8B%E6%9E%B6%E5%AE%89%E8%A3%85)
``` bash
# Mvc 模板
dotnet new install Furion.SqlSugar.Template.Mvc

# WebApi 模板
dotnet new install Furion.SqlSugar.Template.Api
```

### 使用脚手架
``` bash
# -n 指定项目名称 
# -f 指定.NET版本
dotnet new furionapi -n Furion.Demo -f net8
```