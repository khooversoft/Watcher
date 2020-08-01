using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestRestApiServer.Application
{
    internal class Option : IOption
    {
        public bool Help { get; set; }
        public string? ConfigFile { get; set; }
        public int Port { get; set; } = 5001;
    }
}
