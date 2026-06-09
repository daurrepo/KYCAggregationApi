using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Carnegie.KycAggregationApi.Application.Models;

public class AggregatedKycDataResponse
{
    [Required]
    [StringLength(13)]
    [JsonPropertyName("ssn")]

    public string Ssn { get; init; } = "";

    [Required]
    [StringLength(100)]
    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = "";


    [Required]
    [StringLength(100)]
    [JsonPropertyName("last_name")]
    public string LastName { get; init; } = "";

    [Required]
    [StringLength(256)]
    [JsonPropertyName("address")]
    public string Address { get; init; } = "";

    [Phone]
    [StringLength(30)]
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; init; }

    [EmailAddress]
    [StringLength(254)]
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [Required]
    [StringLength(2, MinimumLength = 2)]
    [JsonPropertyName("tax_country")]
    public string TaxCountry { get; init; } = "";

    [Range(0, int.MaxValue)]
    [JsonPropertyName("income")]
    public int? Income { get; init; }
}