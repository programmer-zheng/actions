using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace Volo.Abp.Study.FlatManage;

public class FlatInfo : Entity<Guid>, ISoftDelete
{
    [Required]
    public required string FlatName { get; set; }

    public string? EnName { get; set; }

    public FlatRouteDeviceTypeEnum DeviceType { get; set; } = FlatRouteDeviceTypeEnum.Mikrotik;

    public string? ConfigStr { get; set; }

    public bool IsDeleted { get; } = false;
}