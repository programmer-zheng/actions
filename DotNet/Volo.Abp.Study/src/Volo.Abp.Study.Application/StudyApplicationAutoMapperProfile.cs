using Volo.Abp.Study.FlatManage;
using Volo.Abp.Study.FlatManage.Dtos;
using AutoMapper;

namespace Volo.Abp.Study;

public class StudyApplicationAutoMapperProfile : Profile
{
    public StudyApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<FlatInfo, FlatInfoDto>();
        CreateMap<CreateFlatInfoDto, FlatInfo>(MemberList.Source);
        CreateMap<UpdateFlatInfoDto, FlatInfo>(MemberList.Source);
    }
}
