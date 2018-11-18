namespace Foundatio
{
    public delegate TBuilder Builder<TBuilder, TOptions>(TBuilder builder) 
        where TBuilder : class, IOptionsBuilder<TOptions>, new();
}
