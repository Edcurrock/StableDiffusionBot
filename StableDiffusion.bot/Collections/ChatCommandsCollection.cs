namespace StableDiffusionBot.Collections
{
    public static class ChatCommandsCollection
    {
        public static string BEGIN => "/start";
        public static string CONTINUE => "/continue";
        public static string CREATE => "/create";
        public static string CANCEL => "/cancel";
        public static string PROMT_WAY => "/promt";
        public static string SOURCE_IMG_WAY => "/sourceimage";
        public static string GET_IMAGE_WITHOUT_SOURCE_IMG => "Пропустить и получить изображение";
        public static string RETRY => "Начать заново";
        public static string SAME_PROMPT => "Еще раз";
    }
}
