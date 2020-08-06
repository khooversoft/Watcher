using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject);

        public static string? ToNullIfEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject) ? null : subject;

        public static string? WithParameters(this string? subject, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (subject.IsEmpty()) return subject;
            parameters.VerifyNotNull(nameof(parameters));

            return subject + " (" + string.Join(", ", parameters.Select(x => $"{x.Key}={x.Value}")) + ")";
        }
    }
}
