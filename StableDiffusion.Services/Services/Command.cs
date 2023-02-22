using StableDiffusion.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StableDiffusion.Services.Services
{
    public abstract class Command
    {
        private readonly HashSet<Error> _errors = new HashSet<Error>();

        public void AddError(Error error)
        {
            if (!_errors.Contains(error))
            {
                _errors.Add(error);
            }
        }

        public void AddError<T>(T enumValue) where T : Enum
        {
            var code = Convert.ToInt32(enumValue);
            AddError(new Error(code, enumValue.GetDescription()));
        }

        public IEnumerable<Error> Errors => _errors;
        public bool IsValid => !_errors.Any();
    }

    public class Error
    {
        public Error(int code, string description)
        {
            Code = code;
            Description = description;
        }

        public int Code { get; }
        public string Description { get; }
    }
}
