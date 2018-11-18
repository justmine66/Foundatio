namespace Foundatio
{
    public interface IOptionsBuilder
    {
        object Target { get; }
    }

    public interface IOptionsBuilder<out T> : IOptionsBuilder
    {
        T Build();
    }
}
