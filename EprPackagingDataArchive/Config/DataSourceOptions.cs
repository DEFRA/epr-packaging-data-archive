namespace EprPackagingDataArchive.Config;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Selects which set of provider adapters is registered.
///
/// This is the switch that makes each later phase a registration change rather than a rewrite. On
/// CDP it arrives as the environment variable <c>DataSource__Mode</c>, since ASPNETCORE_ENVIRONMENT
/// is pinned to Production there and appsettings.Development.json never applies.
/// </summary>
public class DataSourceOptions
{
    public const string SectionName = "DataSource";

    [Required]
    [EnumDataType(typeof(DataSourceMode))]
    public DataSourceMode Mode { get; init; } = DataSourceMode.Stub;
}

public enum DataSourceMode
{
    /// <summary>In-memory fixtures. No network, no database. Phase one.</summary>
    Stub = 0,

    /// <summary>Reads the Azure Common Data API through the CDP egress proxy. Phase two.</summary>
    CommonDataApi = 1,

    /// <summary>Reads a locally held projection. Phase three, once the store is chosen.</summary>
    Projection = 2
}
