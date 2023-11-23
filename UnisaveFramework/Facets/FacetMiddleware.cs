using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unisave.Foundation;

namespace Unisave.Facets
{
    /// <summary>
    /// Base class for any facet middleware class
    /// </summary>
    public abstract class FacetMiddleware
    {
        /// <summary>
        /// Handles a request, passes it to the next layer
        /// and returns the response
        /// </summary>
        public abstract Task<FacetResponse> Handle(
            FacetRequest request,
            Func<FacetRequest, Task<FacetResponse>> next,
            string[] parameters
        );

        /// <summary>
        /// Executes provided closure inside all middleware layers
        /// </summary>
        public static async Task<FacetResponse> ExecuteMiddlewareStack(
            IContainer services,
            IEnumerable<MiddlewareAttribute> globalMiddleware,
            FacetRequest request,
            Func<FacetRequest, Task<FacetResponse>> action
        )
        {
            List<Layer> layers = ObtainMiddlewareLayers(
                services,
                globalMiddleware,
                request
            );
            
            LayerIterator iterator = new LayerIterator(
                layers,
                action
            );
            
            return await iterator.Iterate(request);
        }

        /// <summary>
        /// Extracts and sorts middleware layers from a facet request
        /// </summary>
        private static List<Layer> ObtainMiddlewareLayers(
            IContainer services,
            IEnumerable<MiddlewareAttribute> globalMiddleware,
            FacetRequest request
        )
        {
            var globalLayers = globalMiddleware
                .Select(attr => new Layer(services, attr))
                .OrderBy(l => l.order);
            
            var classLayers = request.FacetType
                .GetCustomAttributes<MiddlewareAttribute>(inherit: true)
                .Select(attr => new Layer(services, attr))
                .OrderBy(l => l.order);
            
            var methodLayers = request.Method
                .GetCustomAttributes<MiddlewareAttribute>(inherit: true)
                .Select(attr => new Layer(services, attr))
                .OrderBy(l => l.order);

            return globalLayers
                .Concat(classLayers)
                .Concat(methodLayers)
                .ToList();
        }

        /// <summary>
        /// One middleware layer
        /// = middleware instance + parameters + order
        /// </summary>
        private class Layer
        {
            public readonly FacetMiddleware middleware;
            public readonly string[] parameters;
            public readonly int order;

            public Layer(IContainer services, MiddlewareAttribute attribute)
            {
                order = attribute.Order;
                parameters = attribute.Parameters;
                middleware = (FacetMiddleware) services.Resolve(
                    attribute.MiddlewareType
                );
            }
        }

        /// <summary>
        /// Executes the recursive iteration through middleware layers
        /// </summary>
        private class LayerIterator
        {
            private int currentLayerIndex;
            private readonly List<Layer> layers;
            private readonly Func<FacetRequest, Task<FacetResponse>> finalAction;

            public LayerIterator(
                List<Layer> layers,
                Func<FacetRequest, Task<FacetResponse>> finalAction
            )
            {
                this.layers = layers;
                this.finalAction = finalAction;
            }

            public async Task<FacetResponse> Iterate(FacetRequest request)
            {
                currentLayerIndex = 0;
                return await Next(request);
            }

            private async Task<FacetResponse> Next(FacetRequest request)
            {
                if (currentLayerIndex >= layers.Count)
                    return await finalAction.Invoke(request);
                
                var currentLayer = layers[currentLayerIndex];
                currentLayerIndex++;
                
                FacetResponse response = await currentLayer.middleware
                    .Handle(request, Next, currentLayer.parameters);
                
                // make sure null wasn't returned
                if (response == null)
                {
                    throw new NullReferenceException(
                        $"Middleware {currentLayer.middleware.GetType()} has "
                        + $"returned null when calling {request.FacetName}."
                        + $"{request.MethodName} Returning null from "
                        + "middleware is not allowed."
                    );
                }

                return response;
            }
        }
    }
}