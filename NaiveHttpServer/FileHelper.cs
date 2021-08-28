using System;
using System.IO;
using System.Threading.Tasks;

namespace NaiveHttpServer
{
    public static class FileHelper
    {
        public static ILogger? Logger { get; set; }

        public static async Task WriteAsync(string path, Func<Stream, Task> writer, bool isBackup = true)
        {
            string tempFilePath = $"{path}.writing";
            using (var stream = new FileStream(
                tempFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                0x10000,
                FileOptions.SequentialScan)
            )
            {
                await writer(stream);
            }

            if (File.Exists(path))
            {
                await SpinRetry(() => File.Replace(tempFilePath, path, isBackup ? BackupPath(path) : null, true));
            }
            else
            {
                await SpinRetry(() => File.Move(tempFilePath, path));
            }
        }

        public static async Task ReadAsync(string path, Func<Stream, Task> reader)
        {
            try
            {
                await CriticalReadAsync(path, reader);
            }
            catch (Exception e)
            {
                string backupPath = BackupPath(path);
                if (!File.Exists(backupPath))
                {
                    throw;
                }

                Logger?.Warning($"Can not read {path}, turn back to backup.", e);
                await CriticalReadAsync(backupPath, reader);
            }
        }

        private static async Task CriticalReadAsync(string path, Func<Stream, Task> reader)
        {
            using FileStream stream = new(
                path,
                FileMode.OpenOrCreate,
                FileAccess.Read,
                FileShare.ReadWrite,
                0x10000,
                FileOptions.SequentialScan);
            await reader(stream);
        }

        private static string BackupPath(string path) => $"{path}.backup";

        private static async Task SpinRetry(Action action, int retryCount = 10)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    action();
                    await Task.Delay(100);
                    break;
                }
                catch
                {
                    if (i == retryCount - 1)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
