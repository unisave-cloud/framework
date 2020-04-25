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
        
        public static void Log(string message)
            => UnisaveAdapter?.info?.Invoke(message, null);

        public static void LogFormat(string format, params object[] args) 
            => Log(string.Format(format, args));

        public static void LogWarning(string message)
            => UnisaveAdapter?.warning?.Invoke(message, null);
        
        public static void LogWarningFormat(string format, params object[] args)
            => LogWarning(string.Format(format, args));

        public static void LogError(string message)
            => UnisaveAdapter?.error?.Invoke(message, null);

        public static void LogErrorFormat(string format, params object[] args) 
            => LogError(string.Format(format, args));

        public static void LogException(Exception exception)
            => LogError(exception.ToString());
    }
}
