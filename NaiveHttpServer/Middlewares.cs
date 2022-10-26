using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace NaiveHttpServer
{
    public delegate Task Middleware<in T>(T ctx, Func<Task> next);

    public static class Middlewares
    {
        public static Middleware<T> Empty<T>() => (_, next) => next();

        public static async Task Log(Context ctx, Func<Task> next)
        {
            ILogger logger = ctx.Logger;
            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;

            logger.Debug($"[In] {request.HttpMethod} {request.RawUrl}");
            await next();
            logger.Debug($"[Out] {request.HttpMethod} [{response.StatusCode}] {request.RawUrl}");
        }

        public static async Task Execute(Context ctx, Func<Task> next)
        {
            using HttpListenerResponse response = ctx.Response;
            try
            {
                await next();
            }
            catch (Exception e)
            {
                ctx.Logger.Warning("Unexpected exception occurred.", e);
                await response.Error(ErrorCodes.Unknown, e.Message);
                response.StatusCode = e switch
                {
                    FileNotFoundException => 404,
                    DirectoryNotFoundException => 404,
                    UnauthorizedAccessException => 403,
                    _ => 500,
                };
            }
        }

        public static Middleware<Context> NotFound(string documentUrl)
        {
            return async (ctx, _) =>
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.Error(
                    ErrorCodes.NotFoundApi,
                    $"Not found this api: '{ctx.Request.RawUrl}', and please read the API document: {documentUrl}.");
            };
        }

        public static Middleware<Context> StaticFile(string route, string rootDir)
        {
            return async (ctx, next) =>
            {
                // Don't use Request.RawUrl, because it contains url parameters. (e.g. '?a=1&b=2')
                string relativePath = ctx.Request.Url.AbsolutePath.TrimStart('/');
                bool handled = relativePath.StartsWith(route);
                if (!handled)
                {
                    await next();
                    return;
                }

                string requestPath = HttpUtility.UrlDecode(relativePath)
                    .Substring(route.Length)
                    .ToLowerInvariant()
                    .TrimStart('/', '\\');
                string filePath = Path.Combine(rootDir, requestPath);

                switch (ctx.Request.HttpMethod)
                {
                    case HttpMethods.Get:
                        await ReadLocalFile(filePath, ctx.Response, ctx.Logger);
                        break;
                    case HttpMethods.Put:
                        await WriteLocalFile(filePath, ctx.Request);
                        ctx.Response.StatusCode = 204;
                        break;
                }
            };
        }

        private static async Task ReadLocalFile(string filePath, HttpListenerResponse response, ILogger logger)
        {
            if (File.Exists(filePath))
            {
                await response.File(filePath);
            }
            else if (Directory.Exists(filePath))
            {
                string[] filePaths = Directory
                    .GetFileSystemEntries(filePath)
                    .Select(Path.GetFileName)
                    .ToArray();
                await response.Json(filePaths);
            }
            else
            {
                string message = $"Not found the file: '{filePath}'.";
                logger.Warning(message);
                await response.Error(ErrorCodes.NotFoundFile, message, 404);
            }
        }

        private static Task WriteLocalFile(string filePath, HttpListenerRequest request)
        {
            return FileHelper.WriteAsync(filePath, stream => request.InputStream.CopyToAsync(stream));
        }

        public static Middleware<T> Then<T>(this Middleware<T> middleware, Middleware<T> nextMiddleware)
        {
            return (ctx, next) => middleware(ctx, () => nextMiddleware(ctx, next));
        }

        public static Task Run<T>(this Middleware<T> middleware, T ctx)
        {
            return middleware(ctx, () =>
#if NET45
                Task.FromResult(0)
#else
                Task.CompletedTask
#endif
            );
        }
    }
}
