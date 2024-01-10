using System.ComponentModel.DataAnnotations;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;

public class FlatInfoFilterInput
{
    [FormControlSize(AbpFormControlSize.Default)]
    [Display(Name = "FlatInfoFlatName")]
    public string? FlatName { get; set; }

    [FormControlSize(AbpFormControlSize.Default)]
    [Display(Name = "FlatInfoEnName")]
    public string? EnName { get; set; }

    [FormControlSize(AbpFormControlSize.Default)]
    [Display(Name = "FlatInfoDeviceType")]
    public FlatRouteDeviceTypeEnum? DeviceType { get; set; }

    [FormControlSize(AbpFormControlSize.Default)]
    [Display(Name = "FlatInfoConfigStr")]
    public string? ConfigStr { get; set; }

}