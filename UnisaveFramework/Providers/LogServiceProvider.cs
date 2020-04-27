using System.Linq;
using System.Reflection;
using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Logging;
using Unisave.Runtime;
using UnityEngine;

namespace Unisave.Providers
{
    public class LogServiceProvider : ServiceProvider
    {
        private readonly InMemoryLog log;
        
        public LogServiceProvider(Application app) : base(app)
        {
            log = new InMemoryLog(
                maxRecordCount: 50,
                maxRecordSize: 5_000
            );
        }

        public override void Register()
        {
            App.Singleton<ILog>(app => log);

            // if there's a property called "unisave adapter" on the Debug class
            // then we're definitely running in a sandbox and we do the binding
            if (typeof(Debug)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                .Any(p => p.Name == nameof(Debug.UnisaveAdapter))
            )
            {
                HookIntoUnityEngineDebug();
            }
        }

        private void HookIntoUnityEngineDebug()
        {
            Debug.UnisaveAdapter = new Debug.Adapter {
                info = (message, context) => log.Info(message, context),
                warning = (message, context) => log.Warning(message, context),
                error = (message, context) => log.Error(message, context)
            };
        }

        public override void TearDown()
        {
            var specialValues = App.Resolve<SpecialValues>();
            specialValues.Add("logs", log.ExportLog());
        }
    }
}