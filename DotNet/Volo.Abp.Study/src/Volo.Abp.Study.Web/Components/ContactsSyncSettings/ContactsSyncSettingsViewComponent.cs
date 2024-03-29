﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using Volo.Abp.Study.CustomSetting;

namespace Volo.Abp.Study.Web.Components.ContactsSyncSettings;

public class ContactsSyncSettingsViewComponent : AbpViewComponent
{
    /*
     * 参考链接
     * https://github.com/abpframework/abp/tree/7.4.3/modules/setting-management/src/Volo.Abp.SettingManagement.Web/Pages/SettingManagement/Components/EmailSettingGroup
     */

    private readonly IContactsSyncSettingsAppService _contactsSyncSettingsAppService;

    public ContactsSyncSettingsViewComponent(IContactsSyncSettingsAppService contactsSyncSettingsAppService)
    {
        _contactsSyncSettingsAppService = contactsSyncSettingsAppService;
    }

    public virtual async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await _contactsSyncSettingsAppService.GetAsync();
        var model = ObjectMapper.Map<ContactsSyncSettingsDto, UpdateContactsSettingsViewModel>(settings);

        model.SyncProviderNames = new List<SelectListItem>
        {
            new() { Text = "钉钉", Value = "DingDing" },
            new() { Text = "企业微信", Value = "WeWork" }
        }.ToArray();
        return View("~/Components/ContactsSyncSettings/Default.cshtml", model);
    }
}

public class UpdateContactsSettingsViewModel
{
    [DynamicFormIgnore]
    public SelectListItem[] SyncProviderNames { get; set; }

    [Display(Name = "SyncEnabled")]
    public bool SyncEnabled { get; set; }

    [SelectItems("SyncProviderNames")]
    [Display(Name = "SyncProviderName")]
    [Required]
    public string ProviderName { get; set; }
}