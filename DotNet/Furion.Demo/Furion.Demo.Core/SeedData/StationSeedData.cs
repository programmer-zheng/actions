using Furion.Demo.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Core.SeedData;

public class StationSeedData : ISqlSugarEntitySeedData<StationEntity>
{
    public IEnumerable<StationEntity> HasData()
    {
        var list = Enumerable.Range(1, 254).Select(t => new StationEntity()
        {
            Id = t,
            ip = "127.0.0.1",
            port = 11000 + t,
            sno = t.ToString("D3"),
        }).ToList();
        return list;
    }
}