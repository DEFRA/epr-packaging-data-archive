using EprPackagingDataArchive.Organisations.Models;
using EprPackagingDataArchive.Organisations.Providers;

namespace EprPackagingDataArchive.Test.Organisations.Providers;

/// <summary>
/// Unit tests for the organisation adapter. No host, no HTTP: the class is constructed directly.
///
/// These pin the behaviour that any future adapter has to reproduce. When the Common Data API
/// adapter arrives in phase two, it should be able to pass this same set of assertions.
/// </summary>
public class StubOrganisationProviderTest
{
    private readonly IOrganisationProvider _provider = new StubOrganisationProvider();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Returns_null_for_an_unknown_organisation()
    {
        var result = await _provider.GetOrganisationAsync("000000", Token);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("100123")]
    [InlineData("100456")]
    [InlineData("CS-004")]
    public async Task Finds_every_seeded_organisation(string organisationId)
    {
        Assert.True(await _provider.ExistsAsync(organisationId, Token));
    }

    [Fact]
    public async Task Matches_organisation_ids_case_insensitively()
    {
        // Reference numbers are handled as opaque strings, so casing must not decide identity.
        Assert.True(await _provider.ExistsAsync("cs-004", Token));
    }

    [Fact]
    public async Task A_scheme_operator_has_no_producer_size_of_its_own()
    {
        var scheme = await _provider.GetOrganisationAsync("CS-004", Token);

        Assert.NotNull(scheme);
        Assert.Equal(OrganisationTypes.ComplianceSchemeOperator, scheme.Type);
        // It carries no obligation itself, so Large or Small would be meaningless.
        Assert.Null(scheme.ProducerSize);
        Assert.Null(scheme.Registration);
    }

    [Fact]
    public async Task Submissions_are_selected_by_the_subject_not_the_submitter()
    {
        // 100456's data was filed by CS-004. The member must still see it.
        var memberSubmissions = await _provider.GetSubmissionsAsync("100456", new SubmissionQuery(), Token);

        var submission = Assert.Single(memberSubmissions);
        Assert.Equal("CS-004", submission.SubmittedBy.OrganisationId);
        Assert.Equal(SubmitterRoles.ComplianceScheme, submission.SubmittedBy.Role);
    }

    [Fact]
    public async Task The_submitting_scheme_does_not_inherit_its_members_submissions()
    {
        // The mirror of the test above: filing on someone's behalf does not make it your data.
        var schemeSubmissions = await _provider.GetSubmissionsAsync("CS-004", new SubmissionQuery(), Token);

        Assert.Empty(schemeSubmissions);
    }

    [Fact]
    public async Task Submissions_are_ordered_newest_first()
    {
        var submissions = await _provider.GetSubmissionsAsync("100123", new SubmissionQuery(), Token);

        Assert.Equal(3, submissions.Count);
        Assert.True(submissions.First().SubmittedAt > submissions.Last().SubmittedAt);
    }

    [Fact]
    public async Task Filters_submissions_by_obligation_year()
    {
        var submissions = await _provider.GetSubmissionsAsync(
            "100123", new SubmissionQuery { ObligationYear = 2026 }, Token);

        // Both 2025 periods carry obligation year 2026: the accepted H2 and the rejected H1.
        Assert.Equal(2, submissions.Count);
        Assert.All(submissions, s => Assert.StartsWith("2025-", s.SubmissionPeriod));
    }

    [Fact]
    public async Task Returns_an_empty_collection_rather_than_null_when_nothing_matches()
    {
        var submissions = await _provider.GetSubmissionsAsync(
            "100123", new SubmissionQuery { SubmissionPeriod = "2099-H1" }, Token);

        Assert.NotNull(submissions);
        Assert.Empty(submissions);
    }

    [Fact]
    public async Task A_submission_cannot_be_read_through_the_wrong_organisation()
    {
        // Real submission id, wrong owner. Guessing an id must not disclose another org's data.
        var result = await _provider.GetSubmissionAsync(
            "100123", "4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024", Token);

        Assert.Null(result);
    }

    [Fact]
    public async Task Submission_detail_distinguishes_submitted_from_reached_the_warehouse()
    {
        var result = await _provider.GetSubmissionAsync(
            "100456", "4e0a91c3-77bf-4d18-b6a2-5f13c8e9d024", Token);

        Assert.NotNull(result);
        Assert.False(result.AvailableInWarehouse);
        Assert.True(result.IsResubmission);
        Assert.Equal(1, result.Validation.WarningCount);
    }
}
