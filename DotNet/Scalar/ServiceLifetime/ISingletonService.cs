namespace Scalar.ServiceLifetime;

public interface ISingletonService
{
    string GetGuid();
}
public class SingletonService : ISingletonService
{
    public string GetGuid() => Guid.NewGuid().ToString();
}