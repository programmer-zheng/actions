using System;
using System.ComponentModel.DataAnnotations;

namespace Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;

public class EditFlatInfoViewModel
{
    [Display(Name = "FlatInfoFlatName")]
    public string FlatName { get; set; }

    [Display(Name = "FlatInfoEnName")]
    public string? EnName { get; set; }

    [Display(Name = "FlatInfoDeviceType")]
    public FlatRouteDeviceTypeEnum DeviceType { get; set; }

    [Display(Name = "FlatInfoConfigStr")]
    public string? ConfigStr { get; set; }

    [Display(Name = "FlatInfoIsDeleted")]
    public bool IsDeleted { get; set; }
}
