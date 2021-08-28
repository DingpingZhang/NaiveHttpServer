using System;

namespace NaiveHttpServer
{
    public interface ILogger
    {
        void Error(string message, Exception? exception = null);

        void Warning(string message, Exception? exception = null);

        void Info(string message, Exception? exception = null);

        void Debug(string message, Exception? exception = null);
    }
}
