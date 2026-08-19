using EprPackagingDataArchive.ComplianceSchemes.Models;
using EprPackagingDataArchive.ComplianceSchemes.Providers;

namespace EprPackagingDataArchive.Test.ComplianceSchemes.Providers;

/// <summary>
/// Unit tests for the compliance scheme adapter, constructed directly with no host.
///
/// Membership is the part of this domain most likely to shape the eventual database choice, because
/// a producer can join or leave part-way through an obligation year. The "as at" tests below are the
/// ones worth reading first.
/// </summary>
public class StubComplianceSchemeProviderTest
{
    private readonly IComplianceSchemeProvider _provider = new StubComplianceSchemeProvider();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Only_the_seeded_scheme_exists()
    {
        Assert.True(await _provider.ExistsAsync("CS-004", Token));
        Assert.False(await _provider.ExistsAsync("CS-999", Token));
    }

    [Fact]
    public async Task Returns_the_whole_membership_when_no_date_is_given()
    {
        var members = await _provider.GetMembersAsync("CS-004", new MemberQuery(), Token);

        Assert.Equal(3, members.Count);
        Assert.Contains(members, m => m.Status == MembershipStatuses.Left);
    }

    [Theory]
    [InlineData("2026-02-15", true)]  // still a member
    [InlineData("2026-03-31", true)]  // last day of membership, inclusive
    [InlineData("2026-04-01", false)] // day after leaving
    [InlineData("2024-12-31", false)] // before joining
    public async Task Membership_as_at_a_date_respects_both_ends_of_the_dates(string date, bool expected)
    {
        var members = await _provider.GetMembersAsync(
            "CS-004", new MemberQuery { AsAt = DateOnly.Parse(date) }, Token);

        Assert.Equal(expected, members.Any(m => m.OrganisationId == "100777"));
    }

    [Fact]
    public async Task A_producer_that_joined_mid_year_is_absent_before_it_joined()
    {
        // 100987 joined on 2026-04-01, part-way through the H1 window.
        var january = await _provider.GetMembersAsync(
            "CS-004", new MemberQuery { AsAt = new DateOnly(2026, 1, 15) }, Token);

        Assert.DoesNotContain(january, m => m.OrganisationId == "100987");
    }

    [Fact]
    public async Task Filters_membership_by_status()
    {
        var left = await _provider.GetMembersAsync(
            "CS-004", new MemberQuery { Status = MembershipStatuses.Left }, Token);

        var only = Assert.Single(left);
        Assert.Equal("100777", only.OrganisationId);
        Assert.NotNull(only.LeftOn);
    }

    [Fact]
    public async Task Reporting_status_counts_reconcile_with_the_member_list()
    {
        var status = await _provider.GetReportingStatusAsync("CS-004", "2026-H1", Token);

        Assert.Equal(status.Members.Count, status.Summary.Members);
        Assert.Equal(status.Members.Count(m => m.Reported), status.Summary.Reported);
        Assert.Equal(status.Members.Count(m => !m.Reported), status.Summary.NotReported);
        Assert.Equal(status.Summary.Members, status.Summary.Reported + status.Summary.NotReported);
    }

    [Fact]
    public async Task A_member_that_did_not_report_has_null_tonnage_not_zero()
    {
        var status = await _provider.GetReportingStatusAsync("CS-004", "2026-H1", Token);

        var silent = status.Members.Where(m => !m.Reported).ToList();
        Assert.NotEmpty(silent);
        // Null and zero mean different things: "we have heard nothing" versus "they reported nil".
        Assert.All(silent, m =>
        {
            Assert.Null(m.Tonnage);
            Assert.Null(m.ReportedAt);
            Assert.Null(m.SubmissionId);
        });
    }

    [Fact]
    public async Task A_member_that_reported_carries_its_submission_and_timestamp()
    {
        var status = await _provider.GetReportingStatusAsync("CS-004", "2026-H1", Token);

        var reported = Assert.Single(status.Members.Where(m => m.Reported));
        Assert.Equal("100456", reported.OrganisationId);
        Assert.Equal(133.80m, reported.Tonnage);
        Assert.NotNull(reported.SubmissionId);
        Assert.NotNull(reported.ReportedAt);
    }

    [Fact]
    public async Task Reporting_status_derives_the_obligation_year_from_the_period()
    {
        var status = await _provider.GetReportingStatusAsync("CS-004", "2026-H1", Token);

        Assert.Equal(2027, status.ObligationYear);
    }

    [Fact]
    public async Task A_period_nobody_reported_for_returns_every_member_as_not_reported()
    {
        var status = await _provider.GetReportingStatusAsync("CS-004", "2099-H1", Token);

        Assert.Equal(3, status.Summary.Members);
        Assert.Equal(0, status.Summary.Reported);
        Assert.All(status.Members, m => Assert.False(m.Reported));
    }
}
