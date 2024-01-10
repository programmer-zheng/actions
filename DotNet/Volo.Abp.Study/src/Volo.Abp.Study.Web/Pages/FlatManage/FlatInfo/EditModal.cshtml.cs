using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Study.FlatManage;
using Volo.Abp.Study.FlatManage.Dtos;
using Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;

namespace Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo;

public class EditModalModel : StudyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EditFlatInfoViewModel ViewModel { get; set; }

    private readonly IFlatInfoAppService _service;

    public EditModalModel(IFlatInfoAppService service)
    {
        _service = service;
    }

    public virtual async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        ViewModel = ObjectMapper.Map<FlatInfoDto, EditFlatInfoViewModel>(dto);
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        var dto = ObjectMapper.Map<EditFlatInfoViewModel, UpdateFlatInfoDto>(ViewModel);
        await _service.UpdateAsync(Id, dto);
        return NoContent();
    }
}