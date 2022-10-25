using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NaiveHttpServer
{
    public static class Extensions
    {
        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static async Task Json(this HttpListenerResponse response, object value)
        {
            string jsonText = value.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(jsonText);

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "application/json";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task File(this HttpListenerResponse response, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return;
            }

            await FileHelper.ReadAsync(filePath, async stream =>
            {
                await stream.CopyToAsync(response.OutputStream);
                response.ContentType = MimeTypes.GetMimeType(filePath);
                response.ContentLength64 = stream.Length;
            });
        }

        public static async Task Error(this HttpListenerResponse response, string errorCode, string message, int statusCode = 500)
        {
            response.StatusCode = statusCode;
            await response.Json(new
            {
                errorCode,
                message,
            });
        }

        public static async Task<T?> JsonFromBody<T>(this HttpListenerRequest request)
        {
            string jsonText = await request.TextFromBody();
            return jsonText.ToObject<T>();
        }

        public static async Task<string> TextFromBody(this HttpListenerRequest request)
        {
            using StreamReader reader = new(request.InputStream);
            return await reader.ReadToEndAsync();
        }

        public static string ToJson(this object value, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(value, formatting, JsonSettings);
        }

        public static T? ToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }
    }
}
