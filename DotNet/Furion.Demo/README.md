# Furion DEMO
## ��Ŀ��ʼ��
### ���ּܰ�װ
[furion--sqlsugar-���ּܰ�װ](https://furion.net/docs/template#2722-furion--sqlsugar-%E8%84%9A%E6%89%8B%E6%9E%B6%E5%AE%89%E8%A3%85)
``` bash
# Mvc ģ��
dotnet new install Furion.SqlSugar.Template.Mvc

# WebApi ģ��
dotnet new install Furion.SqlSugar.Template.Api
```

### ʹ�ý��ּ�
``` bash
# -n ָ����Ŀ���� 
# -f ָ��.NET�汾
dotnet new furionapi -n Furion.Demo -f net8
```


## ��������Docker����
### tdengine
``` bash
docker run -itd --name tdengine --restart always -p 6030:6030 -p 6041:6041 -p 6043:6043 -p 6044-6049:6044-6049 -p 6044-6045:6044-6045/udp -p 6060:6060 tdengine/tdengine:3.3.2.0
```