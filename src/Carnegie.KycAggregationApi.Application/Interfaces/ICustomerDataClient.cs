using Carnegie.KycAggregationApi.Application.Models;

namespace Carnegie.KycAggregationApi.Application.Interfaces;

public interface ICustomerDataClient
{
    Task<PersonalDetails?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default);
    Task<ContactDetails?> GetContactDetailsAsync(string ssn, CancellationToken ct = default);
    Task<KycForm?> GetKycFormAsync(string ssn, DateOnly asOfDate, CancellationToken ct = default);
}
