using EprPackagingDataArchive.PackagingData.Providers;

namespace EprPackagingDataArchive.Test.PackagingData.Providers;

/// <summary>
/// Unit tests for the nested Get Packaging Data report, the shape the ticket describes.
/// </summary>
public class StubPackagingDataReportTest
{
    private readonly IPackagingDataProvider _provider = new StubPackagingDataProvider();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Returns_null_for_an_unknown_organisation()
    {
        Assert.Null(await _provider.GetReportAsync("000000", new ReportQuery(), Token));
    }

    [Fact]
    public async Task An_organisation_with_no_data_gets_an_empty_report_not_null()
    {
        var report = await _provider.GetReportAsync("100999", new ReportQuery(), Token);

        Assert.NotNull(report);
        Assert.Equal("100999", report.Organisation.OrganisationId);
        Assert.Empty(report.Submissions);
    }

    [Fact]
    public async Task Nests_rows_under_their_own_submission()
    {
        var report = await _provider.GetReportAsync("100123", new ReportQuery(), Token);

        Assert.NotNull(report);
        Assert.Equal(3, report.Submissions.Count);
        // Every row belongs to exactly one submission: totals across the nesting equal the estate.
        Assert.Equal(9, report.Submissions.Sum(s => s.PackagingData.Count));
        Assert.All(report.Submissions, s => Assert.NotEmpty(s.PackagingData));
    }

    [Fact]
    public async Task Year_filters_on_the_submission_period_year()
    {
        var report = await _provider.GetReportAsync("100123", new ReportQuery { Year = 2025 }, Token);

        Assert.NotNull(report);
        // 2025-H2 accepted and 2025-H1 rejected; the 2026-H1 submission is excluded.
        Assert.Equal(2, report.Submissions.Count);
        Assert.All(report.Submissions, s => Assert.StartsWith("2025-", s.SubmissionPeriod));
    }

    [Fact]
    public async Task Status_rejected_returns_only_rejected_submissions()
    {
        var report = await _provider.GetReportAsync("100123", new ReportQuery { Status = "rejected" }, Token);

        Assert.NotNull(report);
        var only = Assert.Single(report.Submissions);
        Assert.Equal("RejectedByRegulator", only.Status);
        Assert.Equal("2025-H1", only.SubmissionPeriod);
    }

    [Fact]
    public async Task Rows_carry_the_ticket_fields()
    {
        var report = await _provider.GetReportAsync(
            "100123", new ReportQuery { Year = 2025, Status = "rejected" }, Token);

        Assert.NotNull(report);
        var rows = Assert.Single(report.Submissions).PackagingData;

        var subsidiaryRow = Assert.Single(rows, r => r.SubsidiaryId is not null);
        Assert.Equal("100123-S01", subsidiaryRow.SubsidiaryId);
        Assert.Equal(1200, subsidiaryRow.TransitionalPackagingUnits);

        var parentRow = Assert.Single(rows, r => r.SubsidiaryId is null);
        Assert.Equal("PVC", parentRow.PackagingMaterialSubtype);
        Assert.Equal(120.00m, parentRow.PackagingMaterialWeight);
        Assert.Equal("GB-ENG", parentRow.FromCountry);
    }
}
