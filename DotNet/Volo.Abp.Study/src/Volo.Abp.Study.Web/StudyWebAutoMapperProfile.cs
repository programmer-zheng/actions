using AutoMapper;
using Volo.Abp.Study.SettingManagement;
using Volo.Abp.Study.Web.Components.ContactsSyncSettings;

namespace Volo.Abp.Study.Web;

public class StudyWebAutoMapperProfile : Profile
{
    public StudyWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.

        CreateMap<ContactsSyncSettingsDto, UpdateContactsSettingsViewModel>();
    }
}