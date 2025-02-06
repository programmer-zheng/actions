using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Scalar.Filters;

public class CustomValidFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errorMessages = context.ModelState.Values
                .Where(t => t.ValidationState == ModelValidationState.Invalid)
                .Select(t => t.Errors.FirstOrDefault()?.ErrorMessage ?? "")
                .ToList();
            var rsp = new ApiResponse
            {
                Code = 400,
                Message = string.Join(",", errorMessages)
            };
            context.Result = new ObjectResult(rsp)
            {
                StatusCode = 200
            };
        }
        else
        {
            await next();
        }
    }
}