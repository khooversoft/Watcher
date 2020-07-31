using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WatcherCmd.Test.Application
{
    public class TestLoggerBuilder
    {
        public ILoggerFactory Build()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            });

            return loggerFactory;
        }
    }
}
