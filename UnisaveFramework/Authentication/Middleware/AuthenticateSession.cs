using System;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Facets;
using Unisave.Foundation;

namespace Unisave.Authentication.Middleware
{
    /// <summary>
    /// Middleware that before a request pulls the authenticated
    /// player from the session and after the request stores
    /// the authenticated player to the session
    /// </summary>
    public class AuthenticateSession : FacetMiddleware
    {
        public const string SessionKey = "authenticatedPlayerId";
        
        private readonly ISession session;
        private readonly AuthenticationManager auth;
        private readonly EntityManager entityManager;
        
        public AuthenticateSession(Application app) : base(app)
        {
            session = app.Resolve<ISession>();
            auth = app.Resolve<AuthenticationManager>();
            entityManager = app.Resolve<EntityManager>();
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
                Entity player = Entity.FromJson(
                    entityManager.Find(id),
                    App.GameAssemblyTypes
                );
                auth.SetPlayer(player);
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