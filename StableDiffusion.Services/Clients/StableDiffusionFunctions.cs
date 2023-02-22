using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Services.Clients
{
    public static class StableDiffusionFunctions
    {
        public static string GenerateAsync => "/v2/generate/async";
        public static string Status => "/v2/generate/status/%ID%";


        public static string GetFunctionWithReplacedId(string func, string id) => func.Replace("%ID%", id);
    }
}
