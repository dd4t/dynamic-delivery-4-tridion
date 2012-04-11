using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntLib = Microsoft.Practices.EnterpriseLibrary.Logging;
using DD4T.ContentModel.Logging;
using System.Diagnostics;

namespace DD4T.LogWrappers.Entlib
{
    public class EntLibLogger : ILogWrapper
    {

        public void Debug(string message, params object[] parameters)
        {
            Log(message, LoggingCategory.General, TraceEventType.Verbose, parameters);
        }

        public void Debug(string message, LoggingCategory category, params object[] parameters)
        {
            Log(message, category, TraceEventType.Verbose, parameters);
        }

        public void Information(string message, params object[] parameters)
        {
            Log(message, LoggingCategory.General, TraceEventType.Information, parameters);
        }

        public void Information(string message, LoggingCategory category, params object[] parameters)
        {
            Log(message, category, TraceEventType.Information, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            Log(message, LoggingCategory.General, TraceEventType.Warning, parameters);
        }

        public void Warning(string message, LoggingCategory category, params object[] parameters)
        {
            Log(message, category, TraceEventType.Warning, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            Log(message, LoggingCategory.General, TraceEventType.Error, parameters);
        }

        public void Error(string message, LoggingCategory category, params object[] parameters)
        {
            Log(message, category, TraceEventType.Error, parameters);
        }

        public void Critical(string message, params object[] parameters)
        {
            Log(message, LoggingCategory.General, TraceEventType.Critical, parameters);
        }

        public void Critical(string message, LoggingCategory category, params object[] parameters)
        {
            Log(message, category, TraceEventType.Critical, parameters);
        }

        private void Log(string message, LoggingCategory category, TraceEventType severity,
                                params object[] parameters)
        {
            var logEntry = new EntLib.LogEntry();
            logEntry.Categories.Add(category.ToString());
            logEntry.Severity = severity;
            if (EntLib.Logger.ShouldLog(logEntry))
            {
                if (parameters.Length > 0)
                {
                    logEntry.Message = string.Format(message, parameters);
                }
                else
                {
                    logEntry.Message = message;
                }
            }
            EntLib.Logger.Write(logEntry);
        }
    }
}
