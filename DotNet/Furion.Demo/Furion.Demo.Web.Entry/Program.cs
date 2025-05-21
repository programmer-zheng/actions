
Serve.Run(RunOptions.Default.WithArgs(args));
/*
// 在 appsettings.json 中配置 HTTPS
// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-9.0#configure-https-in-appsettingsjson

// 直接在代码中配置证书
// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-9.0#configure-http-protocols-in-code
Serve.Run(RunOptions.Default.WithArgs(args)
    .ConfigureBuilder(webBuilder =>
    {
        webBuilder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5000);
            serverOptions.ListenAnyIP(5001, listenOptions => {
                listenOptions.UseHttps("certificate4.pfx", "123456");
            });
        });
    }));*/