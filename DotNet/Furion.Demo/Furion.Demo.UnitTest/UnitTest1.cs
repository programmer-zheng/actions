using Furion.Demo.Application;
using Furion.Demo.Application.SqlSugar;
using Furion.Demo.Application.System;
using Furion.Demo.Application.System.Dtos;
using Furion.Demo.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shouldly;
using SqlSugar;
using StackExchange.Profiling.Internal;
using Xunit.Abstractions;

namespace Furion.Demo.UnitTest
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _outputHelper;

        private readonly FurionDemoApiAppService demoAppService;

        private readonly ISystemService systemService;

        private readonly MySqlAppService mySqlAppService;

        public UnitTest1(ITestOutputHelper outputHelper, FurionDemoApiAppService demoAppService, ISystemService systemService, MySqlAppService mySqlAppService)
        {
            _outputHelper = outputHelper;
            this.demoAppService = demoAppService;
            this.systemService = systemService;
            this.mySqlAppService = mySqlAppService;
        }

        [Fact]
        public void TestGetConfiguration()
        {
            var connectionConfigs = App.GetConfig<List<ConnectionConfig>>("ConnectionConfigs");
            var str = JsonConvert.SerializeObject(connectionConfigs, formatting: Formatting.Indented);
            connectionConfigs.Any(t => t.ConfigId.ToString() == Consts.MainConfigId).ShouldBeTrue();
            _outputHelper.WriteLine(str);

        }

        [Fact]
        public async Task TestDemoApi()
        {
            var result = demoAppService.SayHello();
            result.ShouldBeEquivalentTo("Hello Furion");

        }

        [Fact]
        public async Task TestDemoService()
        {
            var result = systemService.GetDescription();
            _outputHelper.WriteLine(result);
            result.ShouldNotBeNullOrEmpty();

        }

        [Fact]
        public async Task TestInsert()
        {
            var str = "[{\"sno\":\"152\",\"pointNumber\":\"152A01\",\"pointType\":\"1D\"}]";
            var list = JsonConvert.DeserializeObject<List<CreateTdDataDto>>(str);
            await mySqlAppService.CreateAsync(list);

            var queryResult = await mySqlAppService.QueryDataAsync(new Core.Dtos.QueryDataDto());
            _outputHelper.WriteLine(JsonConvert.SerializeObject(queryResult, formatting: Formatting.Indented));
            queryResult.ShouldNotBeNull();
        }
    }
}