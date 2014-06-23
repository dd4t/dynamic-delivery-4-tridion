using DD4T.ContentModel.Logging;
using System;

namespace DD4T.Utils
{
    public static class LoggerService
    {
        private static ILogWrapper _logger = null;
        private static bool HasLogger 
        {
            get
            {
                return Logger != null;
            }
        }
        public static ILogWrapper Logger
        {
            get
            {
                if (_logger == null)
                {
                    if (! string.IsNullOrEmpty(ConfigurationHelper.LoggerClass))
                    {
                        Type type = Type.GetType(ConfigurationHelper.LoggerClass);
                        _logger = Activator.CreateInstance(type) as ILogWrapper;
                    }
                }
                return _logger;
            }
        }

        public static void Debug(string message, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Debug(message, parameters);
        }

        public static void Debug(string message, LoggingCategory category, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Debug(message, category, parameters);
        }

        public static void Information(string message, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Information(message, parameters);
        }

        public static void Information(string message, LoggingCategory category, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Information(message, category, parameters);
        }

        public static void Warning(string message, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Warning(message, parameters);
        }

        public static void Warning(string message, LoggingCategory category, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Warning(message, category, parameters);
        }

        public static void Error(string message, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Error(message, parameters);
        }

        public static void Error(string message, LoggingCategory category, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Error(message, category, parameters);
        }

        public static void Critical(string message, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Critical(message, parameters);
        }

        public static void Critical(string message, LoggingCategory category, params object[] parameters)
        {
            if (!HasLogger) return;
            Logger.Critical(message, category, parameters);
        }
    }
}