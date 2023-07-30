namespace DotnetCoreMvcTestGithubActions;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 处理请求前的逻辑
        string url = context.Request.Path;
        Console.WriteLine($"Received request for URL: {url}");
        // 调用下一个中间件或终止请求
        await _next(context);

        // 处理响应后的逻辑
        int statusCode = context.Response.StatusCode;
        Console.WriteLine($"Returned response with status code: {statusCode}");

    }
}