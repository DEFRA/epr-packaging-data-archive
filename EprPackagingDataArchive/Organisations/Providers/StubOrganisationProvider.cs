using EprPackagingDataArchive.Organisations.Models;
using EprPackagingDataArchive.Shared.Stubs;

namespace EprPackagingDataArchive.Organisations.Providers;

/// <summary>
/// Serves organisation data from <see cref="StubDataSet"/>.
///
/// Filtering is applied here rather than in the endpoint on purpose. A real adapter will push these
/// filters down to its source, so keeping them behind the interface means the endpoint never learns
/// which filters happen to be cheap and which are expensive.
/// </summary>
public sealed class StubOrganisationProvider : IOrganisationProvider
{
    public Task<OrganisationResponse?> GetOrganisationAsync(
        string organisationId,
        CancellationToken cancellationToken = default)
    {
        var match = StubDataSet.Organisations
            .FirstOrDefault(o => string.Equals(o.OrganisationId, organisationId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match);
    }

    public Task<bool> ExistsAsync(string organisationId, CancellationToken cancellationToken = default) =>
        Task.FromResult(StubDataSet.Organisations
            .Any(o => string.Equals(o.OrganisationId, organisationId, StringComparison.OrdinalIgnoreCase)));

    public Task<IReadOnlyCollection<SubmissionResponse>> GetSubmissionsAsync(
        string organisationId,
        SubmissionQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SubmissionResponse> results = SubmissionsFor(organisationId)
            .Where(s => query.SubmissionPeriod is null
                        || string.Equals(s.SubmissionPeriod, query.SubmissionPeriod, StringComparison.OrdinalIgnoreCase))
            .Where(s => query.ObligationYear is null || s.ObligationYear == query.ObligationYear)
            .Where(s => query.Type is null
                        || string.Equals(s.Type, query.Type, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.SubmittedAt)
            .Cast<SubmissionResponse>()
            .ToList();

        return Task.FromResult(results);
    }

    public Task<SubmissionDetailResponse?> GetSubmissionAsync(
        string organisationId,
        string submissionId,
        CancellationToken cancellationToken = default)
    {
        var match = SubmissionsFor(organisationId)
            .FirstOrDefault(s => string.Equals(s.SubmissionId, submissionId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match);
    }

    /// <summary>
    /// Submissions are selected by the organisation the data is ABOUT, not the one that filed it,
    /// so a scheme member sees the submission its scheme made on its behalf.
    /// </summary>
    private static IEnumerable<SubmissionDetailResponse> SubmissionsFor(string organisationId) =>
        StubDataSet.Submissions.Where(s =>
            StubDataSet.SubmissionSubject.TryGetValue(s.SubmissionId, out var subject)
            && string.Equals(subject, organisationId, StringComparison.OrdinalIgnoreCase));
}
