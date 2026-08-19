using System.ComponentModel.DataAnnotations;

namespace EprPackagingDataArchive.Example.Models;

public sealed class UpdateExampleRequest
{
    [Required]
    [MinLength(1)]
    public string Value { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Counter { get; init; }
}