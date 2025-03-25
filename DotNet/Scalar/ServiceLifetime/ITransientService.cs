namespace Scalar.ServiceLifetime;

public interface ITransientService
{
    string GetGuid();
}

public class TransientService : ITransientService
{
    public string GetGuid() => Guid.NewGuid().ToString();
}

