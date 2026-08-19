using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace EprPackagingDataArchive.Utils.Http;

[ExcludeFromCodeCoverage]
public class ProxyHttpMessageHandler : HttpClientHandler
{
    [ExcludeFromCodeCoverage]
    public ProxyHttpMessageHandler(ILogger<ProxyHttpMessageHandler> logger)
    {
        var proxyAddress = Environment.GetEnvironmentVariable("HTTP_PROXY");
        var proxy = new WebProxy { BypassProxyOnLocal = true };
        if (proxyAddress != null)
        {
            logger.LogDebug("Creating proxy http client");
            ConfigureProxy(proxy, proxyAddress);
        }
        else
        {
            logger.LogWarning("HTTP_PROXY is NOT set, proxy client will be disabled");
        }

        Proxy = proxy;
        UseProxy = proxyAddress != null;
    }

    // The CDP proxy requires authentication. WebProxy does not pick up the user:password
    // embedded in the proxy URL automatically, so the credentials must be set explicitly.
    private static void ConfigureProxy(WebProxy proxy, string proxyAddress)
    {
        var proxyUri = new UriBuilder(proxyAddress).Uri;
        proxy.Address = new Uri($"{proxyUri.Scheme}://{proxyUri.Host}:{proxyUri.Port}");

        if (string.IsNullOrEmpty(proxyUri.UserInfo)) return;

        var credentials = proxyUri.UserInfo.Split(':', 2);
        proxy.Credentials = new NetworkCredential(
            Uri.UnescapeDataString(credentials[0]),
            credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty);
    }
}