using System.Net;

namespace SimpleHttpServer
{
    public delegate bool ParameterProvider(string key, out string value);

    public class Context
    {
        public HttpListenerRequest Request { get; }

        public HttpListenerResponse Response { get; }

        public ILogger Logger { get; }

        public ParameterProvider TryGetParameter { get; set; }

        public Context(HttpListenerRequest request, HttpListenerResponse response, ILogger logger)
        {
            Request = request;
            Response = response;
            Logger = logger;
            TryGetParameter = (string _, out string value) =>
            {
                value = null!;
                return false;
            };
        }
    }
}
