using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace StableDiffusionBot.Collections
{
    class ChatKeyboardsCollection
    {
        public static async Task SendChooseWayKeyboardInline(ITelegramBotClient botClient, long chatId)
        {
            List<InlineKeyboardButton> result = new List<InlineKeyboardButton>();

            //InlineKeyboardButton cancelButton = new InlineKeyboardButton("");
            //cancelButton.Text = ChatCommandsCollection.CANCEL_TEXT;
            //cancelButton.CallbackData = ChatCommandsCollection.CANCEL;

            //result.AddRange(new InlineKeyboardButton[] { continueButton, createNewButton, cancelButton });

            await SendInlineKeyboard(botClient, chatId, new InlineKeyboardMarkup(result), "Что делаем дальше?");
        }
        public static async Task SendMenuKeyboard(ITelegramBotClient botClient, long chatId, bool samePrompt = false)
        {
            var menu = new[]
            {
                new[] { new KeyboardButton(ChatCommandsCollection.RETRY) }
            };

            if (samePrompt)
            {
                menu[0] = new[] { new KeyboardButton(ChatCommandsCollection.RETRY), new KeyboardButton(ChatCommandsCollection.SAME_PROMPT) };
            }

            await SendKeyboard(botClient, chatId, menu, "Попробуем еще раз?");
        }

        public static async Task SendImageWayKeyboard(ITelegramBotClient botClient, long chatId)
        {
            var menu = new[]
           {
                new[] { new KeyboardButton(ChatCommandsCollection.GET_IMAGE_WITHOUT_SOURCE_IMG), 
                        new KeyboardButton(ChatCommandsCollection.CANCEL) 
                      }
            };
            await SendKeyboard(botClient, chatId, menu, "Выбор действия");
        }

        private static async Task SendKeyboard(ITelegramBotClient botClient, long chatId, KeyboardButton[][] menu, string message)
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(menu)
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true,
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: replyKeyboardMarkup
            );
        }
        private static async Task SendInlineKeyboard(ITelegramBotClient botClient, long chatId, InlineKeyboardMarkup markup, string message)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                replyMarkup: markup
            );
        }
    }
}
