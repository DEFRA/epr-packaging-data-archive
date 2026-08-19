using EprPackagingDataArchive.PackagingData.Models;

namespace EprPackagingDataArchive.PackagingData.Providers;

/// <summary>
/// The port for reported packaging data. See <see cref="Organisations.Providers.IOrganisationProvider"/>
/// for why these interfaces are shaped around the domain rather than around any particular source.
/// </summary>
public interface IPackagingDataProvider
{
    Task<IReadOnlyCollection<PackagingDataLine>> GetLinesAsync(
        string organisationId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default);

    Task<PackagingDataSummary> GetSummaryAsync(
        string organisationId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default);

    Task<SchemePackagingDataSummary> GetSchemeSummaryAsync(
        string schemeId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record PackagingDataQuery
{
    public string? SubmissionPeriod { get; init; }

    public int? ObligationYear { get; init; }

    public string? Material { get; init; }

    /// <summary>Self or Scheme. Filters on who filed the data, not whose data it is.</summary>
    public string? SubmittedBy { get; init; }
}
