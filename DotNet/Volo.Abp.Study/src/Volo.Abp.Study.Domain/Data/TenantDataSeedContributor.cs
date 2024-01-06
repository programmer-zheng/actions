using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace Volo.Abp.Study.Data;

public class TenantDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ITenantManager _tenantManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IdentityUserManager _identityUserManager;
    private readonly ITenantRepository _tenantRepository;

    public TenantDataSeedContributor(ITenantManager tenantManager, IGuidGenerator guidGenerator, IdentityUserManager identityUserManager, ITenantRepository tenantRepository)
    {
        _tenantManager = tenantManager;
        _guidGenerator = guidGenerator;
        _identityUserManager = identityUserManager;
        _tenantRepository = tenantRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var tenantName = "asdf";
        var tenant = await _tenantRepository.FindByNameAsync(tenantName);
        if (tenant == null)
        {
            // TenantManage只返回实例化后的Tenant对象，并不会往数据库中插入，由 TenantRepository保存至数据库中
            tenant = await _tenantManager.CreateAsync(tenantName);
            await _tenantRepository.InsertAsync(tenant);

            // 此处不需要手动创建租户下对应的 admin 账号，StudyDbMigrationService 类中的 SeedDataAsync 方法会在遍历数据库中的租户时，为相关租户创建一个默认的 admin 账号
        }
    }
}