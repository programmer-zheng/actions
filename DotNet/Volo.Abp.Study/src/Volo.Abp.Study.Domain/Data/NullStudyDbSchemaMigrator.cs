using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Study.Data;

/* This is used if database provider does't define
 * IStudyDbSchemaMigrator implementation.
 */
public class NullStudyDbSchemaMigrator : IStudyDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
