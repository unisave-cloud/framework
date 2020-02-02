using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Utils;

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
        public abstract FacetResponse Handle(
            FacetRequest request,
            Func<FacetRequest, FacetResponse> next,
            string[] parameters
        );

        /// <summary>
        /// Executes provided closure inside all middleware layers
        /// </summary>
        public static FacetResponse ExecuteMiddlewareStack(
            IEnumerable<MiddlewareAttribute> globalMiddleware,
            FacetRequest request,
            Func<FacetRequest, FacetResponse> action
        )
        {
            List<Layer> layers = ObtainMiddlewareLayers(
                globalMiddleware,
                request
            );
            
            LayerIterator iterator = new LayerIterator(
                layers,
                action
            );
            
            return iterator.Iterate(request);
        }

        /// <summary>
        /// Extracts and sorts middleware layers from a facet request
        /// </summary>
        private static List<Layer> ObtainMiddlewareLayers(
            IEnumerable<MiddlewareAttribute> globalMiddleware,
            FacetRequest request
        )
        {
            var globalLayers = globalMiddleware
                .Select(attr => new Layer(attr))
                .OrderBy(l => l.order);
            
            var classLayers = request.FacetType
                .GetCustomAttributes<MiddlewareAttribute>()
                .Select(attr => new Layer(attr))
                .OrderBy(l => l.order);
            
            var methodLayers = request.Method
                .GetCustomAttributes<MiddlewareAttribute>()
                .Select(attr => new Layer(attr))
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

            public Layer(MiddlewareAttribute attribute)
            {
                order = attribute.Order;
                parameters = attribute.Parameters;
                middleware = ExecutionHelper
                    .Instantiate<FacetMiddleware>(attribute.MiddlewareType);
            }
        }

        /// <summary>
        /// Executes the recursive iteration through middleware layers
        /// </summary>
        private class LayerIterator
        {
            private int currentLayerIndex;
            private readonly List<Layer> layers;
            private readonly Func<FacetRequest, FacetResponse> finalAction;

            public LayerIterator(
                List<Layer> layers,
                Func<FacetRequest, FacetResponse> finalAction
            )
            {
                this.layers = layers;
                this.finalAction = finalAction;
            }

            public FacetResponse Iterate(FacetRequest request)
            {
                currentLayerIndex = 0;
                return Next(request);
            }

            private FacetResponse Next(FacetRequest request)
            {
                if (currentLayerIndex >= layers.Count)
                    return finalAction.Invoke(request);
                
                var currentLayer = layers[currentLayerIndex];
                currentLayerIndex++;
                
                var response = currentLayer.middleware
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