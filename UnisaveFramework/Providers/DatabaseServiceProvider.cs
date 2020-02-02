using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Services;

namespace Unisave.Providers
{
    public class DatabaseServiceProvider : ServiceProvider
    {
        public DatabaseServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<IDatabase>(
                app => new SandboxDatabaseApi()
            );
        }
    }
}