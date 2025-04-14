using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Furion.Demo.Application;

/// <summary>
/// 系统服务接口
/// </summary>
public class SystemAppService : IDynamicApiController
{
    private readonly ISystemService _systemService;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly ILogger<SystemAppService> _logger;

    public SystemAppService(ISystemService systemService, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, ILogger<SystemAppService> logger)
    {
        _systemService = systemService;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 获取系统描述
    /// </summary>
    /// <returns></returns>
    public string GetDescription()
    {
        return _systemService.GetDescription();
    }

    [HttpGet]
    [NonUnify]
    public string GenerateUrl()
    {
        _logger.LogInformation(_httpContextAccessor.HttpContext.ToString());
        return _linkGenerator.GetUriByRouteValues(
            _httpContextAccessor.HttpContext,
            "default",
            values: new { id = "1234" });
    }
}