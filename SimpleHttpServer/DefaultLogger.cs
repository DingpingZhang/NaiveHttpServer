using System;
using System.Threading;

namespace SimpleHttpServer
{
    internal class DefaultLogger : ILogger
    {
        public void Error(string message, Exception? exception = null)
        {
            Write(nameof(Error), message, exception);
        }

        public void Warning(string message, Exception? exception = null)
        {
            Write(nameof(Warning), message, exception);
        }

        public void Info(string message, Exception? exception = null)
        {
            Write(nameof(Info), message, exception);
        }

        public void Debug(string message, Exception? exception = null)
        {
            Write(nameof(Debug), message, exception);
        }

        private static void Write(string level, string message, Exception? exception)
        {
            Thread thread = Thread.CurrentThread;
            string threadName = string.IsNullOrEmpty(thread.Name) ? thread.ManagedThreadId.ToString() : thread.Name!;

            string exceptionString = exception is null ? string.Empty : $"{Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}";

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {level.ToUpperInvariant()} [{threadName}] {message}{exceptionString}");
        }
    }
}
