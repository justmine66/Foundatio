namespace Foundatio
{
    public class OptionsBuilder<T> : IOptionsBuilder<T>, IOptionsBuilder where T : class, new()
    {
        public T Target { get; } = new T();

        object IOptionsBuilder.Target => Target;

        public virtual T Build()
        {
            return Target;
        }
    }
}
