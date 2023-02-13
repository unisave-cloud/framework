using System;

namespace UnityEngine.Scripting
{
    /// <summary>
    /// Used by Unity engine to control managed code stripping
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequireDerivedAttribute : Attribute
    {
    }
}