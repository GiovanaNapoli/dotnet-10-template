using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities.Common
{
    public class ResponseBase
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; private init; }

        [JsonPropertyName("errors")]
        public IReadOnlyList<string> Errors { get; private init; } = [];

        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages { get; private init; } = [];

        [JsonPropertyName("warnings")]
        public IReadOnlyList<string> Warnings { get; private init; } = [];

        private ResponseBase() { }

        public static ResponseBase Success(IEnumerable<string>? messages = null) => new()
        {
            IsSuccess = true,
            Messages = messages?.ToList() ?? []
        };

        public static ResponseBase Failure(string error) => new()
        {
            IsSuccess = false,
            Errors = [error]
        };

        public static ResponseBase Failure(IEnumerable<string> errors)
        {
            var errorList = errors.ToList();

            if (errorList.Count == 0)
                throw new ArgumentException("Ao menos um erro deve ser informado.", nameof(errors));

            return new() { IsSuccess = false, Errors = errorList };
        }

        public static ResponseBase Warning(IEnumerable<string> warnings) => new()
        {
            IsSuccess = true,
            Warnings = warnings.ToList()
        };
    }

    public class ResponseBase<T> where T : class
    {
        private readonly ResponseBase _base;

        [JsonPropertyName("isSuccess")]
        public bool IsSuccess => _base.IsSuccess;

        [JsonPropertyName("errors")]
        public IReadOnlyList<string> Errors => _base.Errors;

        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages => _base.Messages;

        [JsonPropertyName("warnings")]
        public IReadOnlyList<string> Warnings => _base.Warnings;

        [JsonPropertyName("data")]
        public T? Data { get; private init; }

        private ResponseBase(ResponseBase baseResponse, T? data = null)
        {
            _base = baseResponse;
            Data = data;
        }

        public static ResponseBase<T> Success(T data, IEnumerable<string>? messages = null)
            => new(ResponseBase.Success(messages), data);

        public static ResponseBase<T> Failure(string error)
            => new(ResponseBase.Failure(error));

        public static ResponseBase<T> Failure(IEnumerable<string> errors)
            => new(ResponseBase.Failure(errors));

        public static ResponseBase<T> Warning(IEnumerable<string> warnings, T? data = null)
            => new(ResponseBase.Warning(warnings), data);
    }
}