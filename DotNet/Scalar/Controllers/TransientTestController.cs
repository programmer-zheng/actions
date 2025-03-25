using Microsoft.AspNetCore.Mvc;
using Scalar.ServiceLifetime;

namespace Scalar.Controllers;

public class TransientTestController : ControllerBase
{
    private readonly ITransientService _transientService;

    public TransientTestController(ITransientService transientService)
    {
        _transientService = transientService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_transientService.GetGuid());
    }
}