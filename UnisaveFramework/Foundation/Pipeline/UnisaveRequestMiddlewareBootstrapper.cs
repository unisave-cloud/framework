using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Logging;

namespace Unisave.Foundation.Pipeline
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class UnisaveRequestMiddlewareBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override Type[] RunAfter => new Type[] {
            typeof(LoggingBootstrapper)
        };
        
        private readonly IAppBuilder owinAppBuilder;

        private readonly Dictionary<string, IAppBuilder> branchBuilders =
            new Dictionary<string, IAppBuilder>();
        
        public UnisaveRequestMiddlewareBootstrapper(IAppBuilder owinAppBuilder)
        {
            this.owinAppBuilder = owinAppBuilder;
        }

        public IAppBuilder DefineBranch(string unisaveRequestKind)
        {
            var branchBuilder = owinAppBuilder.New();
            branchBuilders[unisaveRequestKind] = branchBuilder;
            return branchBuilder;
        }

        private Dictionary<string, AppFunc> CompileBranches()
        {
            Dictionary<string, AppFunc> branches
                = new Dictionary<string, AppFunc>();
            
            foreach (string key in branchBuilders.Keys)
            {
                branches[key] = (AppFunc) branchBuilders[key].Build(
                    typeof(AppFunc)
                );
            }

            return branches;
        }
        
        public override void Main()
        {
            Dictionary<string, AppFunc> branches = CompileBranches();

            owinAppBuilder.Use<UnisaveRequestMiddleware>(branches);
        }
    }
}