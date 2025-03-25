namespace Scalar.ServiceLifetime;

public interface IScopedService
{
    string GetGuid();
}
public class ScopedService : IScopedService
{
    public string GetGuid() => Guid.NewGuid().ToString();
}

