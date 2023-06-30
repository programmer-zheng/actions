using Microsoft.AspNetCore.Mvc.Filters;

namespace DotnetCoreMvcTestGithubActions;

public class LoggingFilter : IActionFilter
{

    public void OnActionExecuting(ActionExecutingContext context)
    {
        Console.WriteLine("Executing custom filter from LoggingFilter");

    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        Console.WriteLine("OnActionExecuted custom filter from LoggingFilter");
    }
}