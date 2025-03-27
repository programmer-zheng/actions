using Furion.Demo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System;

public class TdEngineAppservice : IDynamicApiController
{
    // https://docs.taosdata.com/develop/sql/

    // https://www.donet5.com/Home/Doc?typeId=2566

    private readonly ISugarRepository<PointDataEntity> repository;

    public TdEngineAppservice(ISugarRepository<PointDataEntity> repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public async Task Insert()
    {
        await repository.InsertAsync(new PointDataEntity { PointNumber = "152A01", PointValue = 23.4 });
    }

    [HttpGet]
    public async Task<object> Test()
    {
        var list = await repository.GetListAsync();
        return list;
    }
}
