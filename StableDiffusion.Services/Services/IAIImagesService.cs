using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Services.Services
{
    public interface IAIImagesService
    {
        public Task<GenerateImageResult> GenerateImageAsync(GenerateImageParams parameters);
        public Task<GetImageResult> GetImageAsync(string id);
    }

    public class GenerateImageParams
    {
        public string Prompt { get; set; }
        public string? SourceImg { get; set; }
    }

    public class GenerateImageResult
    {
        public string? Id { get; set; }
    }

    public class GetImageResult
    {
       public byte[] Img { get; set; }
       public GetImageStatus Status { get; set; }
    }

    public enum GetImageStatus
    {
        Unknown = 0,
        Ready = 1,
        Processing = 2,
        Failed = 3
    }
}
