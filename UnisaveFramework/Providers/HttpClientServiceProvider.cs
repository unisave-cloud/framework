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
            App.Singleton<System.Net.Http.HttpClient>(
                app => new System.Net.Http.HttpClient()
            );
            
            // register Unisave HTTP client instance
            App.Singleton<Factory>(
                app => new Factory(app.Resolve<System.Net.Http.HttpClient>())
            );
        }
    }
}