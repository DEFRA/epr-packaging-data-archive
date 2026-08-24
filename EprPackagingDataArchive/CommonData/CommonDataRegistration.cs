using System.Diagnostics.CodeAnalysis;
using EprPackagingDataArchive.CommonData.Client;
using EprPackagingDataArchive.Config;
using EprPackagingDataArchive.Utils.Http;
using Microsoft.Extensions.Options;

namespace EprPackagingDataArchive.CommonData;

[ExcludeFromCodeCoverage]
public static class CommonDataRegistration
{
    /// <summary>
    /// Registers the Common Data API client.
    ///
    /// The client is registered unconditionally and the feature switch gates only route mapping.
    /// An earlier version read configuration twice, once here to decide whether to register and
    /// once at mapping time to decide whether to map. When those two reads disagreed, which they
    /// can if a configuration source is layered in later, the routes existed but their dependency
    /// did not, and every call returned 500 instead of the intended response. Registering always
    /// removes that class of failure: a mapped route can never be missing its client.
    ///
    /// Registration is cheap and opens no connection. Nothing reaches the network until a /cd route
    /// is called, and those are only mapped when the feature is enabled.
    /// </summary>
    public static IServiceCollection AddCommonDataPoc(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<CommonDataApiOptions>()
            .Bind(configuration.GetSection(CommonDataApiOptions.SectionName))
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.BaseUrl),
                "CommonDataApi:BaseUrl is required when CommonDataApi:Enabled is true.")
            .ValidateOnStart();

        // Must go through AddHttpClientWithTracingAndProxy. A plain AddHttpClient would lose the
        // authenticated CDP egress proxy, so every outbound call would fail once deployed, and would
        // also drop x-cdp-request-id correlation.
        services.AddHttpClientWithTracingAndProxy<ICommonDataApiClient, CommonDataApiClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CommonDataApiOptions>>().Value;

                // Tolerates an unconfigured base URL so that resolving the client can never throw.
                // The routes that use it are not mapped unless a base URL has been supplied.
                if (!string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                }

                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        return services;
    }

    /// <summary>Whether the exploratory /cd routes should be mapped.</summary>
    public static bool IsEnabled(IConfiguration configuration) =>
        configuration.GetSection(CommonDataApiOptions.SectionName).Get<CommonDataApiOptions>()?.Enabled ?? false;
}
