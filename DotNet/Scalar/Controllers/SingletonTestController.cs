using Microsoft.AspNetCore.Mvc;
using Scalar.ServiceLifetime;

namespace Scalar.Controllers;

public class SingletonTestController : ControllerBase
{
    private readonly ISingletonService _singletonService;

    public SingletonTestController(ISingletonService singletonService)
    {
        _singletonService = singletonService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_singletonService.GetGuid());
    }
}