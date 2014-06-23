namespace DD4T.ContentModel.Logging
{
    public enum LoggingCategory { General, Controller, View, Model, System, Integration, Performance }
    public interface ILogWrapper
    {
        void Debug(string message, params object[] parameters);
        void Debug(string message, LoggingCategory category, params object[] parameters);
        void Information(string message, params object[] parameters);
        void Information(string message, LoggingCategory category, params object[] parameters);
        void Warning(string message, params object[] parameters);
        void Warning(string message, LoggingCategory category, params object[] parameters);
        void Error(string message, params object[] parameters);
        void Error(string message, LoggingCategory category, params object[] parameters);
        void Critical(string message, params object[] parameters);
        void Critical(string message, LoggingCategory category, params object[] parameters);
    }
}
