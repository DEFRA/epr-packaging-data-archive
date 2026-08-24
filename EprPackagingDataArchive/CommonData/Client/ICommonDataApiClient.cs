namespace EprPackagingDataArchive.CommonData.Client;

/// <summary>
/// Calls the Azure Common Data API directly, passing responses through unmapped.
///
/// Deliberately not one of the domain providers. Mapping upstream rows onto the domain model is a
/// later decision, and this exists to see the raw shape before making it.
/// </summary>
public interface ICommonDataApiClient
{
    /// <summary>
    /// The smoke test. Takes no parameters and returns a timestamp, so a 200 here means the whole
    /// chain works: egress, firewall, TLS and auth. Start with this one.
    /// </summary>
    Task<UpstreamResult> GetLastSyncTimeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The only organisation-filterable endpoint upstream. Built as the regulator's caseworker grid,
    /// so it returns submission summaries rather than packaging lines.
    /// </summary>
    Task<UpstreamResult> GetPomSummaryAsync(string organisationReference, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Samples the POM stream. That endpoint returns NDJSON for every producer across a whole year
    /// with no organisation filter, so this reads only the first <paramref name="take"/> rows and
    /// then stops, which also releases the upstream rate limit slot.
    /// </summary>
    Task<UpstreamResult> GetPomSampleAsync(int relativeYear, int take, CancellationToken cancellationToken);
}
