using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Volo.Abp.Study.Pages;

public class Index_Tests : StudyWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
