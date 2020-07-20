using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Toolbox.Services
{
    /// <summary>
    /// Provides json services using .net core JSON
    /// </summary>
    public class Json : IJson
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static Json Default { get; } = new Json();

        public string Serialize<T>(T subject) => JsonSerializer.Serialize(subject, _options);

        public T Deserialize<T>(string subject) => JsonSerializer.Deserialize<T>(subject, _options);
    }
}
