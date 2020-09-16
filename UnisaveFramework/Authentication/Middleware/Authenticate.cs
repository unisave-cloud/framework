using System;
using Unisave.Facets;
using Unisave.Foundation;

namespace Unisave.Authentication.Middleware
{
    /// <summary>
    /// Rejects unauthenticated requests by throwing AuthException
    /// </summary>
    public class Authenticate : FacetMiddleware
    {
        private readonly AuthenticationManager auth;
        
        public Authenticate(Application app) : base(app)
        {
            auth = app.Resolve<AuthenticationManager>();
        }

        public override FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        )
        {
            if (!auth.Check())
                throw new AuthException("Unauthenticated");
            
            return next.Invoke(request);
        }
    }
}