using EprPackagingDataArchive.Organisations.Models;
using EprPackagingDataArchive.PackagingData.Models;
using EprPackagingDataArchive.Shared.Stubs;

namespace EprPackagingDataArchive.PackagingData.Providers;

/// <summary>
/// Serves packaging data from <see cref="StubDataSet"/>.
///
/// Summaries are computed from the same line fixtures the detail endpoint returns rather than being
/// separate hardcoded totals. That is not just tidiness: it means a caller who fetches the lines and
/// adds them up gets the summary figure, which is a guarantee a real implementation must also make
/// and which hardcoded fixtures would quietly break.
/// </summary>
public sealed class StubPackagingDataProvider : IPackagingDataProvider
{
    public Task<IReadOnlyCollection<PackagingDataLine>> GetLinesAsync(
        string organisationId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<PackagingDataLine> results = LinesFor(organisationId, query)
            .OrderBy(l => l.SubmissionPeriod)
            .ThenBy(l => l.Material, StringComparer.Ordinal)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<PackagingDataSummary> GetSummaryAsync(
        string organisationId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default)
    {
        var lines = LinesFor(organisationId, query).ToList();

        var summary = new PackagingDataSummary
        {
            OrganisationId = organisationId,
            SubmissionPeriod = query.SubmissionPeriod ?? "all",
            ObligationYear = query.ObligationYear ?? lines.Select(l => l.ObligationYear).DefaultIfEmpty(0).Max(),
            Totals = TotalsOf(lines),
            ByMaterial = GroupBy(lines, l => l.Material),
            ByActivity = GroupBy(lines, l => l.Activity),
            ByNation = GroupBy(lines, l => l.FromNation)
        };

        return Task.FromResult(summary);
    }

    public Task<SchemePackagingDataSummary> GetSchemeSummaryAsync(
        string schemeId,
        PackagingDataQuery query,
        CancellationToken cancellationToken = default)
    {
        var memberIds = StubDataSet.SchemeMembers.Select(m => m.OrganisationId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var lines = StubDataSet.PackagingLines
            .Where(l => memberIds.Contains(l.OrganisationId))
            .Where(l => Matches(l, query))
            .ToList();

        var summary = new SchemePackagingDataSummary
        {
            SchemeId = schemeId,
            SubmissionPeriod = query.SubmissionPeriod ?? "all",
            ObligationYear = query.ObligationYear ?? lines.Select(l => l.ObligationYear).DefaultIfEmpty(0).Max(),
            MemberCount = memberIds.Count,
            ReportingMemberCount = lines.Select(l => l.OrganisationId)
                .Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            Totals = TotalsOf(lines),
            ByMaterial = GroupBy(lines, l => l.Material),
            ByNation = GroupBy(lines, l => l.FromNation)
        };

        return Task.FromResult(summary);
    }

    private static IEnumerable<PackagingDataLine> LinesFor(string organisationId, PackagingDataQuery query) =>
        StubDataSet.PackagingLines
            .Where(l => string.Equals(l.OrganisationId, organisationId, StringComparison.OrdinalIgnoreCase))
            .Where(l => Matches(l, query));

    private static bool Matches(PackagingDataLine line, PackagingDataQuery query)
    {
        if (query.SubmissionPeriod is not null
            && !string.Equals(line.SubmissionPeriod, query.SubmissionPeriod, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.ObligationYear is not null && line.ObligationYear != query.ObligationYear) return false;

        if (query.Material is not null
            && !string.Equals(line.Material, query.Material, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (query.SubmittedBy is null) return true;

        var wantsScheme = string.Equals(query.SubmittedBy, "scheme", StringComparison.OrdinalIgnoreCase);
        var isScheme = line.SubmittedBy.Role == SubmitterRoles.ComplianceScheme;

        return wantsScheme == isScheme;
    }

    private static SummaryTotals TotalsOf(IReadOnlyCollection<PackagingDataLine> lines) =>
        new()
        {
            Tonnage = lines.Sum(l => l.Tonnage),
            LineCount = lines.Count
        };

    private static IReadOnlyCollection<Breakdown> GroupBy(
        IEnumerable<PackagingDataLine> lines,
        Func<PackagingDataLine, string> key) =>
        lines.GroupBy(key)
            .Select(g => new Breakdown
            {
                Key = g.Key,
                Tonnage = g.Sum(l => l.Tonnage),
                LineCount = g.Count()
            })
            .OrderByDescending(b => b.Tonnage)
            .ToList();
}
