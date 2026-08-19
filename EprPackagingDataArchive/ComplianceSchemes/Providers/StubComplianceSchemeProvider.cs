using EprPackagingDataArchive.ComplianceSchemes.Models;
using EprPackagingDataArchive.Shared.Stubs;

namespace EprPackagingDataArchive.ComplianceSchemes.Providers;

/// <summary>
/// Serves compliance scheme data from <see cref="StubDataSet"/>.
///
/// Reporting status is derived by joining members against packaging lines rather than being stored,
/// which is the same join a real implementation has to do and keeps the two endpoints consistent.
/// </summary>
public sealed class StubComplianceSchemeProvider : IComplianceSchemeProvider
{
    public Task<bool> ExistsAsync(string schemeId, CancellationToken cancellationToken = default) =>
        Task.FromResult(string.Equals(schemeId, StubDataSet.SchemeId, StringComparison.OrdinalIgnoreCase));

    public Task<IReadOnlyCollection<SchemeMember>> GetMembersAsync(
        string schemeId,
        MemberQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<SchemeMember> results = StubDataSet.SchemeMembers
            .Where(m => query.AsAt is null || WasMemberOn(m, query.AsAt.Value))
            .Where(m => query.Status is null
                        || string.Equals(m.Status, query.Status, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<SchemeReportingStatus> GetReportingStatusAsync(
        string schemeId,
        string submissionPeriod,
        CancellationToken cancellationToken = default)
    {
        var linesByOrganisation = StubDataSet.PackagingLines
            .Where(l => string.Equals(l.SubmissionPeriod, submissionPeriod, StringComparison.OrdinalIgnoreCase))
            .GroupBy(l => l.OrganisationId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var submissionsById = StubDataSet.Submissions
            .ToDictionary(s => s.SubmissionId, s => s, StringComparer.OrdinalIgnoreCase);

        var members = StubDataSet.SchemeMembers
            .Select(member =>
            {
                var reported = linesByOrganisation.TryGetValue(member.OrganisationId, out var lines);
                var submissionId = reported ? lines![0].SubmissionId : null;

                return new MemberReportingStatus
                {
                    OrganisationId = member.OrganisationId,
                    Name = member.Name,
                    Reported = reported,
                    // Null rather than zero when nothing was reported, so "reported nil" and
                    // "did not report" stay distinguishable to a caller.
                    Tonnage = reported ? lines!.Sum(l => l.Tonnage) : null,
                    SubmissionId = submissionId,
                    ReportedAt = submissionId is not null && submissionsById.TryGetValue(submissionId, out var s)
                        ? s.SubmittedAt
                        : null
                };
            })
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();

        var status = new SchemeReportingStatus
        {
            SchemeId = schemeId,
            SubmissionPeriod = submissionPeriod,
            ObligationYear = ObligationYearOf(submissionPeriod),
            Summary = new ReportingSummary
            {
                Members = members.Count,
                Reported = members.Count(m => m.Reported),
                NotReported = members.Count(m => !m.Reported)
            },
            Members = members
        };

        return Task.FromResult(status);
    }

    private static bool WasMemberOn(SchemeMember member, DateOnly date) =>
        member.JoinedOn <= date && (member.LeftOn is null || member.LeftOn >= date);

    private static int ObligationYearOf(string submissionPeriod) =>
        Shared.SubmissionPeriod.TryParse(submissionPeriod, out var period) ? period.Value.ObligationYear : 0;
}
