using AutoMapper;
using Volo.Abp.Study.CustomSetting;
using Volo.Abp.Study.FlatManage.Dtos;
using Volo.Abp.Study.Web.Pages.FlatManage.FlatInfo.ViewModels;
using Volo.Abp.Study.Web.Components.ContactsSyncSettings;

namespace Volo.Abp.Study.Web;

public class StudyWebAutoMapperProfile : Profile
{
    public StudyWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.

        CreateMap<ContactsSyncSettingsDto, UpdateContactsSettingsViewModel>();
        CreateMap<FlatInfoDto, EditFlatInfoViewModel>();
        CreateMap<CreateFlatInfoViewModel, CreateFlatInfoDto>();
        CreateMap<EditFlatInfoViewModel, UpdateFlatInfoDto>();
    }
}
