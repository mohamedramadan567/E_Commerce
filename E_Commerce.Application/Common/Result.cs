using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Application.Common
{
    public class Result
    {
        public bool IsSucces { get; }
        public IReadOnlyList<Error> Errors { get; }
        protected Result(bool isSucces, IReadOnlyList<Error> errors)
        {
            IsSucces = isSucces;
            Errors = errors;
        }

        public static Result Ok() => new(true, Array.Empty<Error>());
        public static Result Fail(Error error) => new(false, new[] { error });
        public static Result Fail(IReadOnlyList<Error> errors) => new(false, errors);

    }

    public class Result<TValue> : Result
    {
        private readonly TValue _value;

        public TValue Data => IsSucces ? _value : throw new InvalidOperationException("Can not Access the value of falied result");

        public Result(TValue value) : base(true, Array.Empty<Error>())
        {
            _value = value;
        }

        public Result(Error error): base(false, new[] {error})
        {
            _value = default!;
        }
        public Result(IReadOnlyList<Error> errors) : base(false, errors)
        {
            _value = default!;
        }

        public static Result<TValue> Ok(TValue value) => new Result<TValue>(value);
        public static Result<TValue> Fail(Error error) => new Result<TValue>(error);
        public static Result<TValue> Fail(IReadOnlyList<Error> errors) => new Result<TValue>(errors);

        public static implicit operator Result<TValue>(TValue value) => Ok(value);
        public static implicit operator Result<TValue>(Error error) => Fail(error);
    }


}
