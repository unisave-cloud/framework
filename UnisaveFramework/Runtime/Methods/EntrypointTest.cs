using System;
using LightJson;
using Unisave.Exceptions;

namespace Unisave.Runtime.Methods
{
    /// <summary>
    /// Used for testing proper framework entrypoint behaviour 
    /// </summary>
    internal static class EntrypointTest
    {
        public static JsonValue Start(
            JsonObject methodParameters,
            SpecialValues specialValues
        )
        {
            switch (methodParameters["perform"].AsString)
            {
                case "return-null":
                    return JsonValue.Null;
                
                case "return-42":
                    return 42;
                
                case "return-foo-bar":
                    return new JsonObject()
                        .Add("foo", "bar");
                
                case "return-null-with-special-foo-bar":
                    specialValues.Add("foo", "bar");
                    return JsonValue.Null;
                
                case "throw-ue":
                    throw new UnisaveException();
                
                case "throw-ue-with-special-foo-bar":
                    specialValues.Add("foo", "bar");
                    throw new UnisaveException();
                
                case "throw-invalid-method-parameters-ex":
                    throw new InvalidMethodParametersException();
                
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}