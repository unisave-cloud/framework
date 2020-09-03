using System;

namespace UnityEngine
{
    public static class Debug
    {
        internal class Adapter
        {
            public Action<string, object> info;
            public Action<string, object> warning;
            public Action<string, object> error;
        }
        
        internal static Adapter UnisaveAdapter { get; set; }
        
        /*
         * NOTE: Context variants are not present because the context object
         * is a UnityEngine.Object, not System.Object which is not available
         * on the server.
         */
        
        public static void Log(object message)
            => UnisaveAdapter?.info?.Invoke(message?.ToString(), null);

        public static void LogFormat(string format, params object[] args) 
            => Log(string.Format(format, args));

        public static void LogWarning(object message)
            => UnisaveAdapter?.warning?.Invoke(message?.ToString(), null);
        
        public static void LogWarningFormat(string format, params object[] args)
            => LogWarning(string.Format(format, args));

        public static void LogError(object message)
            => UnisaveAdapter?.error?.Invoke(message?.ToString(), null);

        public static void LogErrorFormat(string format, params object[] args) 
            => LogError(string.Format(format, args));

        public static void LogException(Exception exception)
            => LogError(exception.ToString());
    }
}
