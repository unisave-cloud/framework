using System;
using System.Reflection;
using TinyIoC;

namespace Unisave.Foundation
{
    public static class TinyIoCExtensions
    {
        /// <summary>
        /// Provides access to the private ConstructType method of the TinyIoC
        /// container. The method constructs a new instance by choosing a
        /// constructor, resolving its arguments and calling it.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationType">
        /// The type that you actually want to construct.
        /// </param>
        /// <param name="requestedType">
        /// This is only used for the construction of generic types.
        /// I believe it's the type with type arguments specified but I
        /// haven't checked. For non-generic types, leave this at null.
        /// </param>
        /// <param name="constructor">
        /// If you want to use a specific constructor, you can pass it in.
        /// </param>
        /// <param name="parameters">
        /// Should named parameters be resolved differently and how
        /// </param>
        /// <param name="options">
        /// How strict should the argument resolution be.
        /// </param>
        /// <returns>
        /// The newly constructed instance.
        /// </returns>
        public static object ConstructType(
            this TinyIoCContainer container,
            Type implementationType,
            Type requestedType = null,
            ConstructorInfo constructor = null,
            NamedParameterOverloads parameters = null,
            ResolveOptions options = null
        )
        {
            MethodInfo method = typeof(TinyIoCContainer).GetMethod(
                name: "ConstructType", 
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                callConvention: CallingConventions.Any,
                types: new Type[] {
                    typeof(Type), typeof(Type), typeof(ConstructorInfo),
                    typeof(NamedParameterOverloads), typeof(ResolveOptions)
                },
                modifiers: null
            );

            if (method == null)
                throw new Exception(
                    "TinyIoC no longer contains the ConstructType method."
                );
            
            return method.Invoke(container, new object[] {
                requestedType, implementationType, // they are indeed in this order
                constructor,
                parameters ?? NamedParameterOverloads.Default,
                options ?? ResolveOptions.Default
            });
        }
    }
}