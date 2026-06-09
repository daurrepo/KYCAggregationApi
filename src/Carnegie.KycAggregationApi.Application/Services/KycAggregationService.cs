using Carnegie.KycAggregationApi.Application.Exceptions;
using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Application.Models;
using Carnegie.KycAggregationApi.Domain;
using Microsoft.Extensions.Logging;

namespace Carnegie.KycAggregationApi.Application.Services;

public class KycAggregationService : IKycAggregationService
{
    private readonly ICustomerDataClient _client;
    private readonly IKycRepository _repository;
    private readonly ILogger<KycAggregationService> _logger;

    public KycAggregationService(ICustomerDataClient client, IKycRepository repository, ILogger<KycAggregationService> logger)
    {
        _client = client;
        _repository = repository;
        _logger = logger;
    }

    public async Task<AggregatedKycDataResponse?> GetAsync(string ssn, CancellationToken ct = default)
    {
        var cached = await _repository.GetBySsnAsync(ssn, ct);

        if (cached is not null)
        {
            var responseFromCache = new AggregatedKycDataResponse
            {
                Ssn = cached.Ssn,
                FirstName = cached.FirstName,
                LastName = cached.LastName,
                Address = cached.Address,
                PhoneNumber = cached.PhoneNumber,
                Email = cached.Email,
                TaxCountry = cached.TaxCountry,
                Income = cached.Income
            };

            if (string.IsNullOrEmpty(responseFromCache.TaxCountry))
            {
                throw new TaxCountryNotPresentException();
            }
            return responseFromCache;
        }

        var personal = await _client.GetPersonalDetailsAsync(ssn, ct);
        if (personal is null)
        {
            throw new CustomerNotFoundException();
        }

        var contact = await _client.GetContactDetailsAsync(ssn, ct);
        var kycForm = await _client.GetKycFormAsync(ssn, DateOnly.FromDateTime(DateTime.UtcNow), ct);

        var aggregated = Map(ssn, personal, contact, kycForm);
        await _repository.SaveAsync(aggregated, ct);
        if (string.IsNullOrEmpty(aggregated.TaxCountry))
        {
            _logger.LogWarning("Tax country is missing for SSN: {Ssn}", ssn);
        }

        var response = MapToResponse(ssn, personal, contact, kycForm);
        if (string.IsNullOrEmpty(response.TaxCountry))
        {
            throw new TaxCountryNotPresentException();
        }
        return response;
    }

    private static AggregatedKycData Map(string ssn, PersonalDetails personal, ContactDetails? contact, KycForm? kycForm)
    {
        var preferredAddress = contact?.Addresses.FirstOrDefault();
        var address = preferredAddress is not null
            ? $"{preferredAddress.Street}, {preferredAddress.PostalCode} {preferredAddress.City}"
            : "";

        var email = GetPreferredEmail(contact);
        var phoneNumber = GetPreferredPhone(contact);
        var taxCountry = GetKycValue(kycForm, "tax_country");

        int? income = null;
        var incomeStr = GetKycValue(kycForm, "annual_income");

        if (int.TryParse(incomeStr, out var parsed))
        {
            income = parsed;
        }

        return new AggregatedKycData
        {
            Ssn = ssn,
            FirstName = personal.FirstName,
            LastName = personal.LastName,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            TaxCountry = taxCountry,
            Income = income
        };
    }

    private static AggregatedKycDataResponse MapToResponse(string ssn, PersonalDetails personal, ContactDetails? contact, KycForm? kycForm)
    {
        var data = Map(ssn, personal, contact, kycForm);

        return new AggregatedKycDataResponse
        {
            Ssn = data.Ssn,
            FirstName = data.FirstName,
            LastName = data.LastName,
            Address = data.Address,
            PhoneNumber = data.PhoneNumber,
            Email = data.Email,
            TaxCountry = data.TaxCountry,
            Income = data.Income
        };
    }

    private static string GetPreferredEmail(ContactDetails? contact)
    {
        return contact?.Emails.FirstOrDefault(e => e.Preferred)?.EmailAddress ?? "";
    }

    private static string GetPreferredPhone(ContactDetails? contact)
    {
        return contact?.PhoneNumbers.FirstOrDefault(p => p.Preferred)?.Number ?? "";
    }

    private static string GetKycValue(KycForm? kycForm, string key)
    {
        return kycForm?.Items.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase))?.Value ?? "";
    }
}
