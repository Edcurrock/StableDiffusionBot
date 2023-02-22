using Microsoft.Extensions.Logging;
using StableDiffusion.Services.Clients;
using StableDiffusion.Services.Models;

namespace StableDiffusion.Services.Services
{
    public class StableDiffusionService : IAIImagesService
    {
        private readonly StableDiffusionClient _client;
        private readonly ILogger<StableDiffusionService> _logger;

        public StableDiffusionService(StableDiffusionClient client, ILogger<StableDiffusionService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GenerateImageResult> GenerateImageAsync(GenerateImageParams parameters)
        {
            var request = new GenerateRequest
            {
                Payload = new GeneratePayload
                {
                    Prompt = parameters.Prompt?.Trim() ?? "", //TODO исправить
                    SourceImg = parameters.SourceImg?.Trim()
                }
            };

            try
            {
                var result = await _client.GenerateImageAsync(request);

                return new GenerateImageResult
                {
                    Id = result.Id
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Attempt to generate image has failed. Internal error");

                return new GenerateImageResult{};
            }
        }

        //TODO удалить
        //public async Task<GetImageResult> GetImageAsync(string id) 
        //{
        //    var result = await _client.GetGeneratedImageAsync(id);
        //    var status = GetImageStatus.Unknown;

        //    if (result.Finished == 1)
        //    {
        //        status = GetImageStatus.Ready;
        //    }
        //    else if (result.Processing == 1)
        //    {
        //        status = GetImageStatus.Processing;
        //    }
        //    else if (result.Faulted || !result.Generations.Any() || 
        //        string.IsNullOrWhiteSpace(result.Generations.FirstOrDefault().Img))
        //    {
        //        status = GetImageStatus.Failed;
        //    }

        //    if (status != GetImageStatus.Ready)
        //    {
        //        return new GetImageResult
        //        {
        //            Status = status
        //        };
        //    }

        //    var img = result.Generations.FirstOrDefault().Img;

        //    return new GetImageResult
        //    {
        //        Img = Convert.FromBase64String(img),
        //        Status = status
        //    };
        //}

        public async Task<GetImageResult> GetImageAsync(string id)
        {
            while (true)
            {
                ImageResponse? result = default;
                try
                {
                    result = await _client.GetGeneratedImageAsync(id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Attempt to get generated image response has failed. Internal");

                    return new GetImageResult
                    {
                        Status = GetImageStatus.Failed
                    };
                }

                if (result.Finished == 1 && result.Generations.Any() &&
                        !string.IsNullOrWhiteSpace(result.Generations.FirstOrDefault().Img))
                {
                    var img = result.Generations.FirstOrDefault().Img;

                    var getImageResult = new GetImageResult
                    {
                        Status = GetImageStatus.Ready
                    };

                    var data = Array.Empty<byte>();
                    if (Convert.TryFromBase64String(img, data, out _))
                    {
                        getImageResult.Img = data;
                    }
                    else 
                    {
                        try
                        {
                            var imgFromUrl = await _client.GetImageFromGeneratedUrl(img);
                            getImageResult.Img = imgFromUrl;
                        }
                        catch(Exception e)
                        {
                            _logger.LogError(e, "Attempt to get generated image from url has failed. Internal");

                            return new GetImageResult
                            {
                                Status = GetImageStatus.Failed
                            };
                        }
                    }

                    return getImageResult;
                }

                await Task.Delay(8000); //TODO найти подходящее время
            }
        }
    }
}
