namespace Foundatio.Serializer
{
    public static class DefaultSerializer
    {
        public static ISerializer Instance
        {
            get;
            set;
        } = new MessagePackSerializer(null, false);

    }
}
