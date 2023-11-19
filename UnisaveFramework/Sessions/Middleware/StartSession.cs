using System;
using Microsoft.Owin;
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
        /// <summary>
        /// Request-scoped services
        /// </summary>
        private readonly IContainer services;

        /// <summary>
        /// OWIN request and response objects
        /// </summary>
        private readonly IOwinContext owinContext;
        
        /// <summary>
        /// Repository that stores the current request's session ID
        /// </summary>
        private readonly ServerSessionIdRepository sessionIdRepository;

        private readonly int sessionLifetimeSeconds;

        public StartSession(
            IContainer services,
            IOwinContext owinContext,
            ServerSessionIdRepository sessionIdRepository,
            SessionBootstrapper bootstrapper
        )
        {
            this.services = services;
            this.owinContext = owinContext;
            this.sessionIdRepository = sessionIdRepository;
            
            sessionLifetimeSeconds = bootstrapper.SessionLifetimeSeconds;
        }

        public override FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        )
        {
            string sessionId = TrackSessionId();
            
            LoadSessionFrontend(sessionId);
            
            try
            {
                return next.Invoke(request);
            }
            finally
            {
                StoreSessionFrontend(sessionId);
            }
        }

        private string TrackSessionId()
        {
            // read or generate the session ID
            string sessionId = owinContext.Request.Cookies["unisave_session_id"]
                ?? ServerSessionIdRepository.GenerateSessionId();

            // store the session ID in the repository
            sessionIdRepository.SessionId = sessionId;
            
            // write the session ID to response cookies
            owinContext.Response.Cookies.Append(
                "unisave_session_id",
                sessionId,
                new CookieOptions() {
                    HttpOnly = true,
                    Path = "/",
                    Expires = DateTime.UtcNow.Add(
                        TimeSpan.FromSeconds(sessionLifetimeSeconds)
                    )
                }
            );
            
            // return the current request's session ID
            return sessionId;
        }

        private void LoadSessionFrontend(string sessionId)
        {
            ISession session = services.Resolve<ISession>();
            session.LoadSession(sessionId);
        }

        private void StoreSessionFrontend(string sessionId)
        {
            /*
             * It's important that we re-resolve the session frontend here,
             * because it might have been replaced during request execution.
             * 
             * (which happens in full stack tests when clearing the database)
             */
            
            ISession session = services.Resolve<ISession>();
            session.StoreSession(sessionId);
        }
    }
}