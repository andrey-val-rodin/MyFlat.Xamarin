using System.Net;
using System.Net.Http;
using System.Reflection;

namespace MobileFlat.Common
{
    /// <summary>
    /// This class is intended to return cookie container after request has been sent.
    /// The usual way to work with cookies is as follows:
    /// 
    ///     var container = new CookieContainer();
    ///     var uri = new Uri("https://lk.globusenergo.ru/");
    ///     using var httpClientHandler = new HttpClientHandler { CookieContainer = container };
    ///     using var httpClient = new HttpClient(httpClientHandler);
    ///     await httpClient.GetAsync(uri);
    ///     var cookies = container.GetCookies(uri).Cast<Cookie>().ToList();
    /// 
    /// But then we have to create our own HTTP handler instance. Xamarin app itself creates
    /// the appropriate handler. For example, for Android it automatically creates AndroidHttpHandler.
    /// HttpClientWithCookies uses Reflection to obtain CookieContainer from private members,
    /// so we always use the auto-instantiated handler.
    /// /// </summary>
    public class HttpClientWithCookies : HttpClient
    {
        public CookieContainer CookieContainer
        {
            get
            {
                var field = GetType().BaseType.BaseType.GetField("_handler",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var handler = field?.GetValue(this) as HttpClientHandler;
                return handler?.CookieContainer;
            }
        }
    }
}
