using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Foundation;
using UnityEngine;

namespace Unisave.Logging
{
    public class LoggingBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        private readonly IAppBuilder owinAppBuilder;

        public LoggingBootstrapper(IAppBuilder owinAppBuilder)
        {
            this.owinAppBuilder = owinAppBuilder;
        }

        public override void Main()
        {
            // register Debug.Log implementation
            HookIntoUnityEngineDebug();
            
            // register the logging middleware
            owinAppBuilder.Use(HandleRequest);
        }

        private async Task HandleRequest(IOwinContext ctx, Func<Task> next)
        {
            var requestServices = ctx.Get<IContainer>("unisave.RequestServices");
            
            // register ILog interface for the request
            requestServices.RegisterInstance<ILog>(
                new InMemoryLog(
                    maxRecordCount: 50,
                    maxRecordSize: 5_000
                )
            );
            
            // run the request
            await next.Invoke();
        }
        
        /// <summary>
        /// Connects the static "UnityEngine.Debug.Log" interface
        /// to the Log facade, so that all future requests can use the API
        /// </summary>
        public static void HookIntoUnityEngineDebug()
        {
            // Only hook if there is a "UnisaveAdapter" property on the Debug class.
            // If there is, we are running with fake UnityEngine dll and so we
            // should hook.
            if (typeof(Debug)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                .All(p => p.Name != nameof(Debug.UnisaveAdapter))
               )
            {
                return;
            }
            
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
                new Action<string, object>(Log.Info)
            );
            
            FieldInfo fiWarning = adapterType.GetField(nameof(Debug.Adapter.warning));
            fiWarning.SetValue(
                adapterInstance,
                new Action<string, object>(Log.Warning)
            );
            
            FieldInfo fiError = adapterType.GetField(nameof(Debug.Adapter.error));
            fiError.SetValue(
                adapterInstance,
                new Action<string, object>(Log.Error)
            );
            
            // === Set the instance into the static property ===

            pi.SetValue(null, adapterInstance, null);
        }
    }
}