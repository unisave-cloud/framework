using System;

namespace UnityEngine.Scripting
{
    /// <summary>
    /// Used by Unity engine to control managed code stripping
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly
        | AttributeTargets.Class
        | AttributeTargets.Constructor
        | AttributeTargets.Delegate
        | AttributeTargets.Enum
        | AttributeTargets.Event
        | AttributeTargets.Field
        | AttributeTargets.Interface
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Struct,
        Inherited = false
    )]
    public class PreserveAttribute : Attribute
    {
    }
}