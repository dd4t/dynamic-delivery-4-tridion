using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel.Logging;
using log4net.Core;
using log4net;

namespace Sample.Web.Mvc.Logging
{
    public class Log4NetLogger : ILogWrapper
    {

        public void Critical(string message, LoggingCategory category, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(category.ToString());
            //Any idea what to do with the category?
            log.FatalFormat(message, parameters);
        }

        public void Critical(string message, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(typeof(Log4NetLogger));
            log.FatalFormat(message, parameters);
        }

        public void Debug(string message, LoggingCategory category, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(category.ToString());
            if (log.IsDebugEnabled)
                log.DebugFormat(message, parameters);
        }

        public void Debug(string message, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(typeof(Log4NetLogger));
            if (log.IsDebugEnabled)
                log.DebugFormat(message, parameters);
        }

        public void Error(string message, LoggingCategory category, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(category.ToString());
            log.ErrorFormat(message, parameters);
        }

        public void Error(string message, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(typeof(Log4NetLogger));
            log.ErrorFormat(message, parameters);
        }

        public void Information(string message, LoggingCategory category, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(category.ToString());
            log.InfoFormat(message, parameters);
        }

        public void Information(string message, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(typeof(Log4NetLogger));
            log.InfoFormat(message, parameters);
        }

        public void Warning(string message, LoggingCategory category, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(category.ToString());
            log.WarnFormat(message, parameters);
        }

        public void Warning(string message, params object[] parameters)
        {
            ILog log = LogManager.GetLogger(typeof(Log4NetLogger));
            log.WarnFormat(message, parameters);
        }
    }
}
