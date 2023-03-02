using StableDiffusionBot.Models;

namespace StableDiffusionBot.Extensions
{
    public static class StringExtensions
    {
        public static ChatStatus? ToChatStatus(this string source)
        {
            ChatStatus? result = source switch
            {
                "BEGIN" => ChatStatus.BEGIN,
                "ADD_PROMPT" => ChatStatus.ADD_PROMPT,
                "REQUEST" => ChatStatus.REQUEST,
                "PROCESSING" => ChatStatus.PROCESSING,
                "CHOOSE_LAST_WAY" => ChatStatus.CHOOSE_LAST_WAY,
                _ => null,
            };

            return result;
        }
    }
}
