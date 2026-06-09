using Carnegie.KycAggregationApi.Domain;

namespace Carnegie.KycAggregationApi.Application.Interfaces;

public interface IKycRepository
{
    Task<AggregatedKycData?> GetBySsnAsync(string ssn, CancellationToken ct = default);
    Task SaveAsync(AggregatedKycData data, CancellationToken ct = default);
}
