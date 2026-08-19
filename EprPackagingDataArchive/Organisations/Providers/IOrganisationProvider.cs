using EprPackagingDataArchive.Organisations.Models;

namespace EprPackagingDataArchive.Organisations.Providers;

/// <summary>
/// The port for organisation data.
///
/// This interface speaks domain language and knows nothing about where the data comes from. That is
/// the whole point: phase two swaps in an adapter that calls the Common Data API, phase three one
/// that reads a local projection, and neither touches an endpoint. If this interface ever grows a
/// method shaped like its source (a SQL string, an upstream DTO, a "summary" endpoint's payload),
/// swapping the implementation stops being a registration change and becomes a rewrite.
/// </summary>
public interface IOrganisationProvider
{
    Task<OrganisationResponse?> GetOrganisationAsync(
        string organisationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string organisationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SubmissionResponse>> GetSubmissionsAsync(
        string organisationId,
        SubmissionQuery query,
        CancellationToken cancellationToken = default);

    Task<SubmissionDetailResponse?> GetSubmissionAsync(
        string organisationId,
        string submissionId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Filters carried as a single object so that adding one later does not change the interface.
/// </summary>
public sealed record SubmissionQuery
{
    public string? SubmissionPeriod { get; init; }

    public int? ObligationYear { get; init; }

    public string? Type { get; init; }
}
