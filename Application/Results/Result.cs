using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public Error? error { get; }
        protected Result(bool success, Error? err)
        {
            if (success & err != null || !success & err == null) throw new Exception("Invalid Result");
            this.IsSuccess = success;
            this.error = err;
        }

        public static Result success() => new(true, null);
        public static Result Fail(Error err) => new(false, err);
        public static implicit operator Result(Error error) => Fail(error);
    }
}
