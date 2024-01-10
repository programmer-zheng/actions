using System;
using System.ComponentModel;
using Volo.Abp.Application.Dtos;

namespace Volo.Abp.Study.FlatManage.Dtos;

[Serializable]
public class FlatInfoGetListInput : PagedAndSortedResultRequestDto
{
    public string? FlatName { get; set; }

    public string? EnName { get; set; }

    public FlatRouteDeviceTypeEnum? DeviceType { get; set; }

    public string? ConfigStr { get; set; }

    public bool? IsDeleted { get; set; }
}