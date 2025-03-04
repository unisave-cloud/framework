using System.Threading;
using Unisave.Bootstrapping;

namespace Unisave.HttpClient
{
    /// <summary>
    /// Registers the .NET HTTP client and the Unisave HTTP client
    /// into the IoC container
    /// </summary>
    public class HttpClientBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override void Main()
        {
            // register .NET HTTP client instance
            Services.RegisterSingleton<System.Net.Http.HttpClient>(
                container => {
                    var dotNetClient = new System.Net.Http.HttpClient();
                    
                    // Timeout is handled by the PendingRequest class,
                    // disable the default .NET 100 second timeout:
                    dotNetClient.Timeout = Timeout.InfiniteTimeSpan;
                    
                    return dotNetClient;
                }
            );
            
            // register Unisave PendingRequest factory
            Services.RegisterSingleton<Factory>(
                container => new Factory(
                    container.Resolve<System.Net.Http.HttpClient>()
                )
            );
            
            // register Unisave HTTP client service (IHttp)
            // (this service is behind the "HTTP" facade)
            Services.RegisterSingleton<IHttp>(
                container => new Client(
                    container.Resolve<Factory>()
                )
            );
        }
    }
}