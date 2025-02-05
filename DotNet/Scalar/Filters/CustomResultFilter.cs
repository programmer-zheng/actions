using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Scalar.Filters;

public class CustomResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode == null)
        {
            var rsp = new ApiResponse()
            {
                Code = objectResult.StatusCode ?? 200, Message = "success",
                Data = objectResult.Value
            };
            context.Result = new ObjectResult(rsp) { StatusCode = objectResult.StatusCode };
        }

        await next();
    }
}