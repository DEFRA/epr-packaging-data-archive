using EprPackagingDataArchive.Shared;

namespace EprPackagingDataArchive.Test.Shared;

public class SubmissionPeriodTest
{
    [Theory]
    [InlineData("2026-H1", 2026, "H1")]
    [InlineData("2026-H2", 2026, "H2")]
    [InlineData("2026-P0", 2026, "P0")]
    [InlineData("2026-h1", 2026, "H1")]
    public void Parses_valid_periods(string value, int expectedYear, string expectedHalf)
    {
        Assert.True(SubmissionPeriod.TryParse(value, out var period));
        Assert.Equal(expectedYear, period.Value.Year);
        Assert.Equal(expectedHalf, period.Value.Half);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("2026")]
    [InlineData("Q1-2026")]
    [InlineData("2026-H3")]
    [InlineData("2026-01")]
    [InlineData("1999-H1")]
    [InlineData("2026-H1-extra")]
    public void Rejects_anything_else(string? value)
    {
        Assert.False(SubmissionPeriod.TryParse(value, out var period));
        Assert.Null(period);
    }

    [Fact]
    public void Obligation_year_is_the_year_after_the_submission_period()
    {
        Assert.True(SubmissionPeriod.TryParse("2026-H1", out var period));

        // The single easiest mistake to make in this domain, so it is derived rather than passed in.
        Assert.Equal(2027, period.Value.ObligationYear);
    }

    [Fact]
    public void Round_trips_through_its_string_form()
    {
        Assert.True(SubmissionPeriod.TryParse("2026-H2", out var period));
        Assert.Equal("2026-H2", period.Value.ToString());
    }
}

public class NationTest
{
    [Theory]
    [InlineData("EN", Nation.England)]
    [InlineData("NI", Nation.NorthernIreland)]
    [InlineData("SC", Nation.Scotland)]
    [InlineData("WS", Nation.Wales)]
    public void Converts_warehouse_codes_to_the_published_form(string warehouse, string expected) =>
        Assert.Equal(expected, Nation.FromWarehouseCode(warehouse));

    [Fact]
    public void Throws_on_an_unrecognised_warehouse_code() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Nation.FromWarehouseCode("XX"));
}
