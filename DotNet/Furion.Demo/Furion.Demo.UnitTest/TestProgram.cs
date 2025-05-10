using Furion.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

[assembly: TestFramework("Furion.Demo.UnitTest.TestProgram", "Furion.Demo.UnitTest")]
namespace Furion.Demo.UnitTest;


public class TestProgram : TestStartup
{
    public TestProgram(IMessageSink messageSink) : base(messageSink)
    {
        // 初始化 Furion
        Serve.RunNative(services =>
        {

        });
    }
}
