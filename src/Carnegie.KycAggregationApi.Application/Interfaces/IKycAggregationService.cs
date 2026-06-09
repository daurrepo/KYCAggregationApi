using Carnegie.KycAggregationApi.Application.Models;
using Carnegie.KycAggregationApi.Domain;

namespace Carnegie.KycAggregationApi.Application.Interfaces;

public interface IKycAggregationService
{
    Task<AggregatedKycDataResponse?> GetAsync(string ssn, CancellationToken ct = default);
}
