using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Study.Permissions;
using Volo.Abp.Study.FlatManage.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Volo.Abp.Study.FlatManage;


public class FlatInfoAppService : CrudAppService<FlatInfo, FlatInfoDto, Guid, FlatInfoGetListInput, CreateFlatInfoDto, UpdateFlatInfoDto>,
    IFlatInfoAppService
{
    protected override string GetPolicyName { get; set; } = StudyPermissions.FlatInfo.Default;
    protected override string GetListPolicyName { get; set; } = StudyPermissions.FlatInfo.Default;
    protected override string CreatePolicyName { get; set; } = StudyPermissions.FlatInfo.Create;
    protected override string UpdatePolicyName { get; set; } = StudyPermissions.FlatInfo.Update;
    protected override string DeletePolicyName { get; set; } = StudyPermissions.FlatInfo.Delete;

    public FlatInfoAppService(IRepository<FlatInfo, Guid> repository) : base(repository)
    {
    }

    protected override async Task<IQueryable<FlatInfo>> CreateFilteredQueryAsync(FlatInfoGetListInput input)
    {
        // TODO: AbpHelper generated
        return (await base.CreateFilteredQueryAsync(input))
            .WhereIf(!input.FlatName.IsNullOrWhiteSpace(), x => x.FlatName.Contains(input.FlatName))
            .WhereIf(input.EnName != null, x => x.EnName == input.EnName)
            .WhereIf(input.DeviceType != null, x => x.DeviceType == input.DeviceType)
            .WhereIf(input.ConfigStr != null, x => x.ConfigStr == input.ConfigStr)
            .WhereIf(input.IsDeleted != null, x => x.IsDeleted == input.IsDeleted)
            ;
    }
}
