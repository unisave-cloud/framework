using System;

namespace Unisave
{
    /*
        Here are defined all the custom attributes of unisave
     */

    /// <summary>
    /// Cross attribute - used to quickly mark certain properties
    /// (currently only for marking entity data)
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class XAttribute : System.Attribute
    {
        public XAttribute()
        {
        }
    }
}
