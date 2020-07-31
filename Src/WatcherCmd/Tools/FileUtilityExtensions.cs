using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Toolbox.Services;
using Toolbox.Tools;

namespace WatcherCmd.Tools
{
    public static class FileUtilityExtensions
    {
        public static T ReadAndDeserialize<T>(this string file, IJson json)
        {
            file.VerifyNotEmpty(nameof(file));
            json.VerifyNotNull(nameof(json));

            return json.Deserialize<T>(File.ReadAllText(file));
        }
    }
}
