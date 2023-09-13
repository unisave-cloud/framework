using Unisave.Foundation;
using Unisave.HttpClient;

namespace Unisave.Providers
{
    public class HttpClientServiceProvider : ServiceProvider
    {
        public HttpClientServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            // register .NET HTTP client instance
            App.Services.RegisterSingleton<System.Net.Http.HttpClient>(
                container => new System.Net.Http.HttpClient()
            );
            
            // register Unisave HTTP client instance
            App.Services.RegisterSingleton<Factory>(
                container => new Factory(container.Resolve<System.Net.Http.HttpClient>())
            );
        }
    }
}