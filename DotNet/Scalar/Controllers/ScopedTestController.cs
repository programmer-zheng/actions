using Microsoft.AspNetCore.Mvc;
using Scalar.ServiceLifetime;

namespace Scalar.Controllers;

public class ScopedTestController : ControllerBase
{
    private readonly IScopedService _scopedService;

    public ScopedTestController(IScopedService scopedService)
    {
        _scopedService = scopedService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_scopedService.GetGuid());
    }
}