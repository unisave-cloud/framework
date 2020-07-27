using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using LightJson;
using Unisave.Http.Client;

namespace Unisave.Facades
{
    /*
     * Why create custom synchronous HTTP client,
     * when .NET gives you "HttpClient" class?
     *
     * - Unisave backend should be single-threaded and synchronous,
     *     which plays nicely with the request-based structure of the system
     * - We can easily fake requests, which is useful for testing
     * - HttpClient from .NET lacks proper JSON integration
     * - Fluent API (inspired by Laravel)
     */
    
    /// <summary>
    /// Facade for making HTTP requests
    /// </summary>
    public static partial class Http
    {
        // TODO: Factory + PendingRequest API here
    }
}