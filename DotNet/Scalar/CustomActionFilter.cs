using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Scalar;

public class CustomActionFilter : IAsyncResultFilter, IAsyncExceptionFilter
{
    // public void OnResultExecuting(ResultExecutingContext context)
    // {
    //     if (context.Result is ObjectResult result)
    //     {
    //         var rsp = new ApiResponse()
    //         {
    //             Code = result.StatusCode ?? 200, Message = "OK",
    //             Data = result.Value
    //         };
    //         context.Result = new ObjectResult(rsp) { StatusCode = result.StatusCode };
    //     }
    // }
    //
    // public void OnResultExecuted(ResultExecutedContext context)
    // {
    // }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult result)
        {
            var rsp = new ApiResponse()
            {
                Code = result.StatusCode ?? 200, Message = "success",
                Data = result.Value
            };
            context.Result = new ObjectResult(rsp) { StatusCode = result.StatusCode };
            await next();
        }
    }

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
            Data = null
        });
        return Task.CompletedTask;
    }
}

public class ApiResponse
{
    public int Code { get; set; }

    public string Message { get; set; }

    public object Data { get; set; }
}

public class CustomException : Exception
{
    public int Code { get; set; }

    public CustomException(int code, string message) : base(message)
    {
        Code = code;
    }
}