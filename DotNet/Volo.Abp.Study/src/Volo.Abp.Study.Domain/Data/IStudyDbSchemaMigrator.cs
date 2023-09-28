using System.Threading.Tasks;

namespace Volo.Abp.Study.Data;

public interface IStudyDbSchemaMigrator
{
    Task MigrateAsync();
}
