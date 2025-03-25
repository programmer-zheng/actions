using Furion;
using System.Reflection;

namespace Furion.Demo.Web.Entry;

public class SingleFilePublish : ISingleFilePublish
{
    public Assembly[] IncludeAssemblies()
    {
        return Array.Empty<Assembly>();
    }

    public string[] IncludeAssemblyNames()
    {
        return new[]
        {
            "Furion.Demo.Application",
            "Furion.Demo.Core",
            "Furion.Demo.Web.Core"
        };
    }
}