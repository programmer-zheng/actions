using Microsoft.AspNetCore.Mvc;

namespace DotnetCoreMvcTestGithubActions.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DemoController : ControllerBase
{
    
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Hello World");
    }
}