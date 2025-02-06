using Scalar.Filters;

namespace Scalar.Middleware;

public class CustomNotFoundMiddleware
{
    private readonly RequestDelegate _next;

    public CustomNotFoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
        if (context.Response.StatusCode == 404 && context.Request.Path.ToString().StartsWith("/api"))
        {
            context.Response.StatusCode = 200; // 或者你想要的其他状态码
            context.Response.ContentType = "application/json";

            var response = new ApiResponse
            {
                Code = 404,
                Message = "The requested resource was not found.",
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}