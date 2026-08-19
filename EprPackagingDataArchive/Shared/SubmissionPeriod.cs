using System.Diagnostics.CodeAnalysis;

namespace EprPackagingDataArchive.Shared;

/// <summary>
/// A reporting window expressed the way the domain expresses it, for example <c>2026-H1</c>.
///
/// Large producers report half-yearly (H1 covers Jan to Jun, H2 covers Jul to Dec); small producers
/// report annually (P0). Callers pass the token, never a date range: arbitrary from/to dates invite
/// consumers to compute the wrong window, which is a mistake the estate has already made upstream.
/// </summary>
public readonly record struct SubmissionPeriod
{
    private SubmissionPeriod(int year, string half)
    {
        Year = year;
        Half = half;
    }

    public int Year { get; }

    /// <summary>H1, H2 or P0.</summary>
    public string Half { get; }

    /// <summary>
    /// The obligation year is the year AFTER the submission period year. Getting this wrong is one
    /// of the easiest mistakes to make in this domain, so it is derived here rather than passed in.
    /// </summary>
    public int ObligationYear => Year + 1;

    public static bool TryParse(string? value, [NotNullWhen(true)] out SubmissionPeriod? period)
    {
        period = null;

        if (string.IsNullOrWhiteSpace(value)) return false;

        var parts = value.Split('-');
        if (parts.Length != 2) return false;

        if (!int.TryParse(parts[0], out var year) || year is < 2020 or > 2100) return false;

        var half = parts[1].ToUpperInvariant();
        if (half is not ("H1" or "H2" or "P0")) return false;

        period = new SubmissionPeriod(year, half);
        return true;
    }

    public override string ToString() => $"{Year}-{Half}";
}
