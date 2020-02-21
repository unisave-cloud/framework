using System;
using Unisave.Contracts;
using Unisave.Facets;
using Unisave.Foundation;

namespace Unisave.Sessions
{
    /// <summary>
    /// Loads and stores session data
    /// </summary>
    public class SessionFacetMiddleware : FacetMiddleware
    {
        public SessionFacetMiddleware(Application app) : base(app) { }
        
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
            ISession session = App.Resolve<ISession>();
            
            session.LoadSession(sessionId);
            
            try
            {
                return next.Invoke(request);
            }
            finally
            {
                session.StoreSession(sessionId);
            }
        }
    }
}