using Carnegie.KycAggregationApi.Application.Interfaces;
using Carnegie.KycAggregationApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace Carnegie.KycAggregationApi.Infrastructure;

public class KycRepository : IKycRepository
{
    private readonly KycDbContext _context;

    public KycRepository(KycDbContext context)
    {
        _context = context;
    }

    public async Task<AggregatedKycData?> GetBySsnAsync(string ssn, CancellationToken ct = default)
    {
        return await _context.KycData.AsNoTracking().FirstOrDefaultAsync(x => x.Ssn == ssn, ct);
    }


    public async Task SaveAsync(AggregatedKycData data, CancellationToken ct = default)
    {
        var existing = await _context.KycData.FindAsync([data.Ssn], ct);
        if (existing is null)
        {
            _context.KycData.Add(data);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(data);
        }

        await _context.SaveChangesAsync(ct);
    }
}
