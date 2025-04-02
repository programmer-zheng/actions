using SqlSugar;

namespace Furion.Demo.Core;

public interface ISugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
{
}

