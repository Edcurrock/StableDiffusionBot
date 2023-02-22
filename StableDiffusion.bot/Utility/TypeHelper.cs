namespace StableDiffusionBot.Utility
{
    public static class TypeHelper
    {
        public static T GetTypedValue<T>(object value)
        {
            if (value.GetType() == typeof(T))
            {
                return (T)value;
            }
            throw new Exception("Cast error");
        }
    }
}
