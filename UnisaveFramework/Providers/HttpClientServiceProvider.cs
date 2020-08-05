using System.Net.Http;
using Unisave.Foundation;
using Unisave.Http.Client;

namespace Unisave.Providers
{
    public class HttpClientServiceProvider : ServiceProvider
    {
        public HttpClientServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            // register .NET HTTP client instance
            App.Singleton<HttpClient>(
                app => new HttpClient()
            );
            
            // register Unisave HTTP client instance
            App.Singleton<Factory>(
                app => new Factory(app.Resolve<HttpClient>())
            );
        }
    }
}