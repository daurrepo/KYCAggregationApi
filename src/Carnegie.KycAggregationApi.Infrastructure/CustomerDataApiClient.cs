using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Application.Models;
using Carnegie.KycAggregationApi.Infrastructure.ApiModels;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Carnegie.KycAggregationApi.Infrastructure;

public class CustomerDataApiClient : ICustomerDataClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CustomerDataApiClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions _jsonOptionsSnake = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public CustomerDataApiClient(HttpClient http, ILogger<CustomerDataApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<PersonalDetails?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"personal-details/{Uri.EscapeDataString(ssn)}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Personal details not found for SSN.");
            return null;
        }

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        var personalDetailsApiModel = await JsonSerializer.DeserializeAsync<PersonalDetailsApiModel>(stream, _jsonOptions, ct);
        return personalDetailsApiModel is null ? null : new PersonalDetails(personalDetailsApiModel.FirstName, personalDetailsApiModel.SurName);
    }

    public async Task<ContactDetails?> GetContactDetailsAsync(string ssn, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"contact-details/{Uri.EscapeDataString(ssn)}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Contact details not found for SSN.");
            return null;
        }

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        var contactDetailsApiModel = await JsonSerializer.DeserializeAsync<ContactDetailsApiModel>(stream, _jsonOptions, ct);

        var contactDetails = contactDetailsApiModel is null ? null : new ContactDetails(
            contactDetailsApiModel.Addresses?.Select(a => new AddressInfo(a.Street, a.City, a.PostalCode, a.Country)).ToList() ?? new List<AddressInfo>(),
            contactDetailsApiModel.Emails?.Select(e => new EmailItem(e.Preferred, e.EmailAddress)).ToList() ?? new List<EmailItem>(),
            contactDetailsApiModel.PhoneNumbers?.Select(p => new PhoneItem(p.Preferred, p.Number)).ToList() ?? new List<PhoneItem>()
        );
        return contactDetails;
    }

    public async Task<KycForm?> GetKycFormAsync(string ssn, DateOnly asOfDate, CancellationToken ct = default)
    {
        var dateStr = asOfDate.ToString("yyyy-MM-dd");
        var response = await _http.GetAsync($"kyc-form/{Uri.EscapeDataString(ssn)}/{dateStr}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("KYC form not found for SSN.");
            return null;
        }

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<KycForm>(stream, _jsonOptionsSnake, ct);
    }
}
