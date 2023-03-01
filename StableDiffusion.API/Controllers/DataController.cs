using Microsoft.AspNetCore.Mvc;
using StableDiffusion.Services.Services;
using System.Net;

namespace StableDiffusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : Controller
    {
        private readonly ILogger<DataController> _logger;
        private readonly IDataService _service;

        public DataController(ILogger<DataController> logger, IDataService service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpPost("chats")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateChatAsync(long chatId)
        {
            var command = new CreateChatCommand
            {
                ChatId = chatId
            };

            await _service.CreateChatAsync(command);

            if (!command.IsValid)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpGet("chats/{chatId}")]
        [ProducesResponseType(typeof(ChatInfoResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetChatInfoAsync(long chatId)
        {
            var info = await _service.GetChatInfoAsync(chatId);

            if (info is null)
            {
                return BadRequest();
            }

            return Ok(new ChatInfoResponse
            {
                Status = info.Status,
                LastGeneratedId = info.LastGeneratedId,
                Prompt = info.Prompt,
                Height = info.Height,
                Width = info.Width
            });
        }

        [HttpPut("chats/{chatId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateChatAsync(long chatId, UpdateChatStatusModel model)
        {
            var command = new EditChatCommand
            {
                ChatId = chatId,
                Status = model.Status,
                GeneratedImageId = model.GeneratedImageId
            };

            await _service.EditChatAsync(command);

            if (!command.IsValid)
            {
                return BadRequest();
            }

            return NoContent();
        }


        [HttpPut("settings/{chatId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateUserSettingsAsync(long chatId, UpdateUserSettingsModel model)
        {
            var command = new EditChatSettingsCommand
            {
                ChatId = chatId,
                Height = model.Height,
                Width = model.Width,
                Prompt = model.Prompt
            };

            await _service.EditChatSettingsAsync(command);

            if (!command.IsValid)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }

    #region models
    public class UpdateChatStatusModel
    {
        public string? Status { get; set; }
        public string? GeneratedImageId { get; set; }
    }
    public class UpdateUserSettingsModel
    {
        public string? Prompt { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class ChatInfoResponse
    {
        public string Status { get; set; }
        public string? Prompt { get; set; }
        public string? LastGeneratedId { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
    #endregion
}


