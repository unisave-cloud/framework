using System;
using Unisave.Contracts;
using Unisave.Facets;
using Unisave.Foundation;

namespace Unisave.Sessions.Middleware
{
    /// <summary>
    /// Loads and stores session data
    /// </summary>
    public class StartSession : FacetMiddleware
    {
        public StartSession(Application app) : base(app) { }
        
        public override FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        )
        {
            if (parameters.Length == 0)
                throw new ArgumentException(
                    "Middleware expects one parameter as the used session id."
                );
            
            string sessionId = parameters[0];
            
            ISession sessionOnStartup = App.Resolve<ISession>();
            sessionOnStartup.LoadSession(sessionId);
            
            try
            {
                return next.Invoke(request);
            }
            finally
            {
                // someone might have tampered with the session service
                // (which happens in full stack tests when clearing the database)
                ISession sessionOnTeardown = App.Resolve<ISession>();
                sessionOnTeardown.StoreSession(sessionId);
            }
        }
    }
}