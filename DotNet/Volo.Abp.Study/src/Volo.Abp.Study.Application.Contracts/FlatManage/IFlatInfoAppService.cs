using System;
using Volo.Abp.Study.FlatManage.Dtos;
using Volo.Abp.Application.Services;

namespace Volo.Abp.Study.FlatManage;


public interface IFlatInfoAppService :
    ICrudAppService< 
        FlatInfoDto, 
        Guid, 
        FlatInfoGetListInput,
        CreateFlatInfoDto,
        UpdateFlatInfoDto>
{

}