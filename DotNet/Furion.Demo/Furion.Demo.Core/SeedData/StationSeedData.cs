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
        return [new StationEntity { Id = 1, ip = "127.0.0.1", port = 1025, sno = "001", }];
    }
}
