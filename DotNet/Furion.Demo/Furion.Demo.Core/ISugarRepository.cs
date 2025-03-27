using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core
{
    public interface ISugarRepository<T> : ISugarRepository, ISimpleClient<T> where T : class, new()
    {
    }

    public class SugarRepository<T> : SimpleClient<T>, ISugarRepository<T> where T : class, new()
    {
        public SugarRepository(ISqlSugarClient db)
        {
            base.Context = db;
        }
    }
}
