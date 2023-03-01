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
                "AGAIN_SAME" => ChatStatus.AGAIN_SAME,
                _ => null,
            };

            return result;
        }
    }
}
