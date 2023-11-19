using System;
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
        
        public LogServiceProvider(BackendApplication app) : base(app)
        {
            log = new InMemoryLog(
                maxRecordCount: 50,
                maxRecordSize: 5_000
            );
        }

        public override void Register()
        {
            App.Services.RegisterSingleton<ILog>(container => log);

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
            // === Get the static property to assign to ===
            
            PropertyInfo pi = typeof(Debug)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(p => p.Name == nameof(Debug.UnisaveAdapter));

            if (pi == null) // missing on the client, do nothing
                return;

            if (!pi.CanWrite)
                return;
            
            // === Create an adapter instance ===
            
            Type adapterType = typeof(UnityEngine.Debug)
                .GetNestedType(
                    nameof(UnityEngine.Debug.Adapter),
                    BindingFlags.NonPublic
                );

            ConstructorInfo ci = adapterType.GetConstructor(new Type[] { });

            if (ci == null)
                return;
            
            object adapterInstance = ci.Invoke(new object[] { });
            
            // === Populate the instance with log handlers ===

            FieldInfo fiInfo = adapterType.GetField(nameof(Debug.Adapter.info));
            fiInfo.SetValue(
                adapterInstance,
                new Action<string, object>(
                    (message, context) => log.Info(message, context)
                )
            );
            
            FieldInfo fiWarning = adapterType.GetField(nameof(Debug.Adapter.warning));
            fiWarning.SetValue(
                adapterInstance,
                new Action<string, object>(
                    (message, context) => log.Warning(message, context)
                )
            );
            
            FieldInfo fiError = adapterType.GetField(nameof(Debug.Adapter.error));
            fiError.SetValue(
                adapterInstance,
                new Action<string, object>(
                    (message, context) => log.Error(message, context)
                )
            );
            
            // === Set the instance into the static property ===

            pi.SetValue(null, adapterInstance, null);
        }

        public override void TearDown()
        {
            // TODO: extract logs by registering log catchers or something...
            // var specialValues = App.Services.Resolve<SpecialValues>();
            // specialValues.Add("logs", log.ExportLog());
        }
    }
}