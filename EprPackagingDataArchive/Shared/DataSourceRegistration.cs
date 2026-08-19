using System.Diagnostics.CodeAnalysis;
using EprPackagingDataArchive.ComplianceSchemes.Providers;
using EprPackagingDataArchive.Config;
using EprPackagingDataArchive.Organisations.Providers;
using EprPackagingDataArchive.PackagingData.Providers;
using Microsoft.Extensions.Options;

namespace EprPackagingDataArchive.Shared;

/// <summary>
/// The one place that decides which adapters back the API.
///
/// Endpoints depend only on the provider interfaces, so moving from fixtures to the Common Data API
/// and later to a local projection is a change here and nowhere else.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DataSourceRegistration
{
    public static IServiceCollection AddPackagingDataProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DataSourceOptions>()
            .Bind(configuration.GetSection(DataSourceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var mode = configuration
            .GetSection(DataSourceOptions.SectionName)
            .Get<DataSourceOptions>()?.Mode ?? DataSourceMode.Stub;

        return mode switch
        {
            DataSourceMode.Stub => services.AddStubProviders(),

            // Phase two. The adapters call the Azure Common Data API through the CDP egress proxy,
            // so they must be registered with AddHttpClientWithTracingAndProxy rather than
            // AddHttpClient, or they lose both request correlation and the proxy credentials.
            DataSourceMode.CommonDataApi => throw new NotSupportedException(
                "DataSource:Mode=CommonDataApi is not implemented yet. It arrives in phase two, "
                + "which also needs a Squid egress allow-list entry and the Azure resource firewall "
                + "opened to CDP's egress IPs. Use Stub until then."),

            // Phase three, once the team has chosen between MongoDB and Aurora PostgreSQL.
            DataSourceMode.Projection => throw new NotSupportedException(
                "DataSource:Mode=Projection is not implemented yet. It arrives in phase three, "
                + "once a persistent store is chosen. Use Stub until then."),

            _ => throw new ArgumentOutOfRangeException(
                nameof(configuration), mode, "Unrecognised data source mode")
        };
    }

    private static IServiceCollection AddStubProviders(this IServiceCollection services)
    {
        // Singletons because the fixtures are immutable and hold no connection or per-request state.
        services.AddSingleton<IDataSourceDescriptor, StubDataSourceDescriptor>();
        services.AddSingleton<IOrganisationProvider, StubOrganisationProvider>();
        services.AddSingleton<IPackagingDataProvider, StubPackagingDataProvider>();
        services.AddSingleton<IComplianceSchemeProvider, StubComplianceSchemeProvider>();

        return services;
    }
}
