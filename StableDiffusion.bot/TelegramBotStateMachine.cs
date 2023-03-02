using Telegram.Bot;
using StableDiffusionBot.Collections;
using StableDiffusionBot.Processors;
using StableDiffusionBot.Models;

namespace StableDiffusionBot
{
    using ChatCommand = Func<long, ITelegramBotClient, object, CancellationToken, Task<ChatStatus?>>;
    public class TelegramBotStateMachine
    {
        public class Transition
        {
            public string InputState { get; set; }
            public ChatCommand Action { get; set; }
        }

        private readonly List<Transition> transitions = new List<Transition>();
        private readonly ILogger<TelegramBotStateMachine> _logger;
        private readonly MessageProcessor _messageProcessor;

        public List<Transition> Transitions => transitions;
        public MessageProcessor MessageProcessor => _messageProcessor;

        public TelegramBotStateMachine(ILogger<TelegramBotStateMachine> logger, MessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //Workflow state events
            AddTransition(ChatStatus.BEGIN.ToString(), _messageProcessor.Begin);
            AddTransition(ChatStatus.ADD_PROMPT.ToString(), _messageProcessor.AddPrompt);
            AddTransition(ChatStatus.REQUEST.ToString(), _messageProcessor.DoRequest);
            //AddTransition(ChatStatus.PROCESSING.ToString(), _messageProcessor.Process);
            AddTransition(ChatStatus.CHOOSE_LAST_WAY.ToString(), _messageProcessor.ChooseLastWay);
            //AddTransition(ChatStatus.SETTINGS.ToString(), _messageProcessor.OpenSettings());
            //AddTransition(ChatStatus.CONTINUE_TASK.ToString(), _messageProcessor.ContinueAddTask);
        }

        public async Task<ChatStatus?> Run(string state, long chatId, ITelegramBotClient botClient, object arg, CancellationToken cancellationToken = default)
        {
            Transition? transition = transitions.FirstOrDefault(x => x.InputState == state);

            if (transition == null)
            {
                return null;
            }

            ChatCommand methodDelegate = transition.Action;

            try
            {
                var status = await methodDelegate.Invoke(chatId, botClient, arg, cancellationToken);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transition");
                throw;
            }
        }

        public void AddTransition(string inputState, ChatCommand transition)
        {
            transitions.Add(new Transition
            {
                InputState = inputState,
                Action = transition
            });
        }

        public bool IsStateTrigger(string message)
        {
            return transitions.Any(x => x.InputState.Equals(message.Trim()));
        }
    }
}
