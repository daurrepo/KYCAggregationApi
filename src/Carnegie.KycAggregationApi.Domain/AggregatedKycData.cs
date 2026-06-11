using System.ComponentModel.DataAnnotations;

namespace Carnegie.KycAggregationApi.Domain;

public record AggregatedKycData
{
    [Required]
    [StringLength(13)]

    public string Ssn { get; init; } = "";

    [Required]
    [StringLength(100)]
    public string FirstName { get; init; } = "";


    [Required]
    [StringLength(100)]
    public string LastName { get; init; } = "";

    [Required]
    [StringLength(256)]
    public string Address { get; init; } = "";

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; init; }

    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; init; }

    [StringLength(2)]
    public string TaxCountry { get; init; } = "";

    [Range(0, int.MaxValue)]
    public int? Income { get; init; }
}