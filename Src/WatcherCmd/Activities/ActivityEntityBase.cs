using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Services;
using WatcherCmd.Application;
using WatcherSdk.Records;
using WatcherSdk.Repository;
using Toolbox.Tools;
using System.Threading.Tasks;
using System.Threading;
using WatcherCmd.Tools;

namespace WatcherCmd.Activities
{
    internal abstract class ActivityEntityBase<T> where T : class, IRecord
    {
        protected readonly IOption _option;
        protected readonly IRecordContainer<T> _recordContainer;
        protected readonly IJson _json;
        protected readonly ILogger _logger;
        protected readonly string _entityName;

        protected ActivityEntityBase(IOption option, IRecordContainer<T> recordContainer, IJson json, ILogger logger, string entityName)
        {
            option.VerifyNotNull(nameof(option));
            recordContainer.VerifyNotNull(nameof(recordContainer));
            json.VerifyNotNull(nameof(json));
            logger.VerifyNotNull(nameof(logger));
            entityName.VerifyNotEmpty(nameof(entityName));

            _option = option;
            _recordContainer = recordContainer;
            _json = json;
            _logger = logger;
            _entityName = entityName;
        }

        public async Task Create(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Create)}: Reading file {_option.File} and writing to store");
            T record = _option.File!.ReadAndDeserialize<T>(_json);

            await _recordContainer.Set(record, token);
        }

        public async Task List(CancellationToken token)
        {
            IReadOnlyList<T> list = await _recordContainer.ListAll(token);

            _logger.LogInformation($"Listing all {_entityName} records");
            foreach (var item in list)
            {
                _logger.LogInformation($"Record={item}");
            }
        }

        public async Task Delete(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Create)}: Deleting ID={_option.Id}");
            await _recordContainer.Delete(_option.Id!, token: token);
        }

        public async Task Clear(CancellationToken token)
        {
            _logger.LogInformation($"{nameof(Create)}: Deleting ID={_option.Id}");
            IReadOnlyList<T> list = await _recordContainer.ListAll(token);

            foreach (var item in list)
            {
                await _recordContainer.Delete(item.Id);
            }
        }

        public abstract Task CreateTemplate(CancellationToken token);
    }
}
