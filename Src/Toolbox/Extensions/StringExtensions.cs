using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject);

        public static string? ToNullIfEmpty(this string? subject) => string.IsNullOrWhiteSpace(subject) ? null : subject;
    }
}
