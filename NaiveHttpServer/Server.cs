using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NaiveHttpServer
{
    public class Server
    {
        private ILogger _logger = new DefaultLogger();
        private HttpListener? _listener;
        private Middleware<Context> _middleware = Middlewares.Empty<Context>();

        public bool IsRunning => _listener is { IsListening: true };

        public string HostUrl { get; }

        public Server(string host, int port)
        {
            HostUrl = $"http://{host}:{port}/";
        }

        public Server Use(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public Server Use(Middleware<Context> middleware)
        {
            _middleware = _middleware.Then(middleware);
            return this;
        }

        public bool Start()
        {
            try
            {
                StartHttpListener();
                return true;
            }
            catch (HttpListenerException e)
            {
                _logger.Warning("Failed to start HttpListener.", e);
                if (e.ErrorCode == 5)
                {
                    NetAclChecker.AddAddress(HostUrl);
                    StartHttpListener();
                    return true;
                }

                return false;
            }
        }

        public void Stop()
        {
            if (_listener is { IsListening: true })
            {
                _listener.Stop();
                _logger.Info("Http server has been stopped.");
            }
        }

        private void StartHttpListener()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(HostUrl);
            _listener.Start();

            AsyncProcessRequest();

            _logger.Info($"Http server has started listening: {HostUrl}...");
        }

        private void AsyncProcessRequest()
        {
            Task.Run(async () =>
            {
                while (_listener!.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = await _listener.GetContextAsync();
                        Context ctx = new(context.Request, context.Response, _logger);

#pragma warning disable 4014
                        _middleware.Run(ctx);
#pragma warning restore 4014
                    }
                    catch (IOException e)
                    {
                        _logger.Warning(nameof(IOException), e);
                    }
                    catch (HttpListenerException e)
                    {
                        const int errorOperationAborted = 995;
                        if (e.ErrorCode == errorOperationAborted)
                        {
                            // The IO operation has been aborted because of either a thread exit or an application request.
                            break;
                        }

                        _logger.Warning(nameof(HttpListenerException), e);
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.Warning(nameof(InvalidOperationException), e);
                    }
                }
            });
        }
    }
}
