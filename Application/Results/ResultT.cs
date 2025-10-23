using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Results
{
    public class Result <T> : Result
    {
        public T? Data { get; }
        private Result(bool success, T? value, Error? err):base(success,err)
        {
            this.Data = value;
        }

        //public static Result<T> success() => new(true, default(T), null);
        public static new Result<T> Fail(Error err) => new(false, default(T), err);
        public static Result<T> success(T value) => new(true, value, null);
        public static implicit operator Result<T>(T value) => success(value);
        public static implicit operator Result<T>(Error err) => Fail(err);
    }
}
