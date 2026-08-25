using EprPackagingDataArchive.Organisations.Models;
using EprPackagingDataArchive.PackagingData.Models;
using EprPackagingDataArchive.PackagingData.Providers;

namespace EprPackagingDataArchive.Test.PackagingData.Providers;

/// <summary>
/// Unit tests for the packaging data adapter, constructed directly with no host.
///
/// The important ones here are the invariants rather than the fixture values: a summary must equal
/// its own lines, and a scheme rollup must not reach outside its membership. Those hold for any
/// implementation, so this class doubles as the contract a phase two or three adapter must satisfy.
/// </summary>
public class StubPackagingDataProviderTest
{
    private readonly IPackagingDataProvider _provider = new StubPackagingDataProvider();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static PackagingDataQuery H1 => new() { SubmissionPeriod = "2026-H1" };

    [Fact]
    public async Task Summary_totals_equal_the_sum_of_the_lines_it_summarises()
    {
        var lines = await _provider.GetLinesAsync("100123", H1, Token);
        var summary = await _provider.GetSummaryAsync("100123", H1, Token);

        // The invariant. Hardcoded fixture totals would let these drift apart silently.
        Assert.Equal(lines.Sum(l => l.Tonnage), summary.Totals.Tonnage);
        Assert.Equal(lines.Count, summary.Totals.LineCount);
    }

    [Fact]
    public async Task Every_breakdown_sums_back_to_the_same_total()
    {
        var summary = await _provider.GetSummaryAsync("100123", H1, Token);

        Assert.Equal(summary.Totals.Tonnage, summary.ByMaterial.Sum(b => b.Tonnage));
        Assert.Equal(summary.Totals.Tonnage, summary.ByActivity.Sum(b => b.Tonnage));
        Assert.Equal(summary.Totals.Tonnage, summary.ByNation.Sum(b => b.Tonnage));
    }

    [Fact]
    public async Task Breakdowns_are_ordered_by_tonnage_descending()
    {
        var summary = await _provider.GetSummaryAsync("100123", H1, Token);

        var tonnages = summary.ByMaterial.Select(b => b.Tonnage).ToList();
        Assert.Equal(tonnages.OrderByDescending(t => t), tonnages);
        Assert.Equal(PackagingMaterials.Plastic, summary.ByMaterial.First().Key);
    }

    [Fact]
    public async Task Filters_lines_by_material()
    {
        var lines = await _provider.GetLinesAsync(
            "100123", new PackagingDataQuery { Material = PackagingMaterials.Glass, SubmissionPeriod = "2026-H1" }, Token);

        var only = Assert.Single(lines);
        Assert.Equal(152.04m, only.Tonnage);
    }

    [Fact]
    public async Task Filters_lines_by_who_submitted_them()
    {
        var bySelf = await _provider.GetLinesAsync(
            "100123", new PackagingDataQuery { SubmittedBy = "self" }, Token);
        var byScheme = await _provider.GetLinesAsync(
            "100123", new PackagingDataQuery { SubmittedBy = "scheme" }, Token);

        Assert.NotEmpty(bySelf);
        Assert.All(bySelf, l => Assert.Equal(SubmitterRoles.Self, l.SubmittedBy.Role));
        // 100123 is a direct producer, so nothing was ever filed on its behalf.
        Assert.Empty(byScheme);
    }

    [Fact]
    public async Task Returns_nothing_for_an_organisation_that_has_reported_nothing()
    {
        var lines = await _provider.GetLinesAsync("100999", new PackagingDataQuery(), Token);
        var summary = await _provider.GetSummaryAsync("100999", new PackagingDataQuery(), Token);

        Assert.Empty(lines);
        // Zero rather than a crash on an empty aggregate.
        Assert.Equal(0m, summary.Totals.Tonnage);
        Assert.Equal(0, summary.Totals.LineCount);
        Assert.Empty(summary.ByMaterial);
    }

    [Fact]
    public async Task A_scheme_rollup_covers_only_its_members()
    {
        var scheme = await _provider.GetSchemeSummaryAsync("CS-004", H1, Token);
        var directProducer = await _provider.GetSummaryAsync("100123", H1, Token);

        // 100123 is not a member and reports far more tonnage. It must not leak into the rollup.
        Assert.Equal(133.80m, scheme.Totals.Tonnage);
        Assert.True(directProducer.Totals.Tonnage > scheme.Totals.Tonnage);
    }

    [Fact]
    public async Task A_scheme_rollup_counts_members_and_reporters_separately()
    {
        var scheme = await _provider.GetSchemeSummaryAsync("CS-004", H1, Token);

        // Three members exist, one reported. Conflating these would hide non-reporting.
        Assert.Equal(3, scheme.MemberCount);
        Assert.Equal(1, scheme.ReportingMemberCount);
    }

    [Fact]
    public async Task Period_filtering_separates_reporting_years()
    {
        var h1 = await _provider.GetSummaryAsync("100123", H1, Token);
        var h2 = await _provider.GetSummaryAsync(
            "100123", new PackagingDataQuery { SubmissionPeriod = "2025-H2" }, Token);

        Assert.Equal(842.19m, h1.Totals.Tonnage);
        Assert.Equal(529.65m, h2.Totals.Tonnage);
        Assert.Equal(2027, h1.ObligationYear);
        Assert.Equal(2026, h2.ObligationYear);
    }
}
