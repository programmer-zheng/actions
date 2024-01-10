using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Study.FlatManage;
using Volo.Abp.Study.FlatManage.Dtos;
using Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;

namespace Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo;

public class CreateModalModel : StudyPageModel
{
    [BindProperty]
    public CreateFlatInfoViewModel ViewModel { get; set; }

    private readonly IFlatInfoAppService _service;

    public CreateModalModel(IFlatInfoAppService service)
    {
        _service = service;
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        var dto = ObjectMapper.Map<CreateFlatInfoViewModel, CreateFlatInfoDto>(ViewModel);
        await _service.CreateAsync(dto);
        return NoContent();
    }
}