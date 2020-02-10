using System;
using Unisave.Contracts;
using Unisave.Facets;
using Unisave.Foundation;

namespace Unisave.Authentication
{
    public class Authenticate : FacetMiddleware
    {
        public const string SessionKey = "authenticatedPlayerId";
        
        private ISession session;
        private AuthenticationManager auth;
        
        public Authenticate(Application app) : base(app)
        {
            session = app.Resolve<ISession>();
            auth = app.Resolve<AuthenticationManager>();
        }

        public override FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        )
        {
            // recall, who was authenticated
            string id = session.Get<string>(SessionKey);

            if (id == null)
            {
                auth.SetPlayer(null);
            }
            else
            {
                // TODO: update this code once you implement Entities properly
                // 1) load entity from the database by ID, as a JsonObject
                // 2) convert it to a concrete instance of an entity
                // 3) auth.SetPlayer(entity)
            }

            try
            {
                return next.Invoke(request);
            }
            finally
            {
                // remember, who is authenticated
                session.Set(SessionKey, auth.Id());
            }
        }
    }
}