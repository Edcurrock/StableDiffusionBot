using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Services.Clients
{
    public class StableDiffusionClient : BaseClient
    {
        public StableDiffusionClient(HttpClient client) : base(client)
        {
        }

        public async Task<GenerateResponse> GenerateImageAsync(GenerateRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = await PostAsync(StableDiffusionFunctions.GenerateAsync, request.Payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GenerateResponse>(content);
        }

        public async Task<ImageResponse> GetGeneratedImageAsync(string id)
        {
            var func = StableDiffusionFunctions.GetFunctionWithReplacedId(StableDiffusionFunctions.Status, id);

            return await GetAsync<ImageResponse>(func);
        }

        public async Task<byte[]> GetImageFromGeneratedUrl(string url) // TODO перенести
        {
            return await GetByteArrayAsync(url);
        }
    }

    #region models
    public class GenerateResponse //TODO сделать цензуру!
    {
        [JsonProperty("id")]
        public string? Id { get; set; }
    }

    public class GenerateRequest
    {
        public GeneratePayload Payload { get; set; }
    }

    public class GeneratePayload
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("source_image", NullValueHandling = NullValueHandling.Ignore)]
        public string? SourceImg { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public GenerateParams? Params { get; set; }

    }

    public class GenerateParams
    {
        [JsonProperty("n")]
        public int N { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class ImageResponse
    {
        [JsonProperty("finished")]
        public int Finished { get; set; }
        [JsonProperty("processing")]
        public int Processing { get; set; }
        [JsonProperty("faulted")]
        public bool Faulted { get; set; }
        [JsonProperty("generations")]
        public List<Generation> Generations { get; set; }
    }

    public class Generation
    {
        [JsonProperty("img")]
        public string? Img { get; set; }
    }
    #endregion
}

public class Rootobject
{
    public string prompt { get; set; }
    public Params _params { get; set; }
    public bool nsfw { get; set; }
    public bool trusted_workers { get; set; }
    public bool censor_nsfw { get; set; }
    public string[] workers { get; set; }
    public string[] models { get; set; }
    public string source_image { get; set; }
    public string source_processing { get; set; }
    public string source_mask { get; set; }
    public bool r2 { get; set; }
    public bool shared { get; set; }
}

public class Params
{
    public string sampler_name { get; set; }
    public int[] toggles { get; set; }
    public int cfg_scale { get; set; }
    public float denoising_strength { get; set; }
    public string seed { get; set; }
    public int height { get; set; }
    public int width { get; set; }
    public int seed_variation { get; set; }
    public string[] post_processing { get; set; }
    public bool karras { get; set; }
    public int steps { get; set; }
    public int n { get; set; }
}
