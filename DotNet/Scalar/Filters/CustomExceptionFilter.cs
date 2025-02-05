using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Scalar.Filters;

public class CustomExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var code = 500;
        if (context.Exception is CustomException customException)
        {
            code = customException.Code;
        }

        context.Result = new ObjectResult(new ApiResponse()
        {
            Code = code, // context.HttpContext.Response.StatusCode,
            Message = context.Exception.Message,
        });
        return Task.CompletedTask;
    }
}