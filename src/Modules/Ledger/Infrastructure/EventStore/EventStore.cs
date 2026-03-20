using Finitech.Modules.Ledger.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Finitech.Modules.Ledger.Infrastructure.EventStore;

/// <summary>
/// Event store for the Ledger module — append-only, immutable.
/// Every financial operation is stored as an event, making the ledger
/// a true audit trail that can be replayed to reconstruct state.
/// </summary>
public class LedgerEventStore
{
    private readonly LedgerEventDbContext _db;

    public LedgerEventStore(LedgerEventDbContext db) => _db = db;

    public async Task AppendAsync(LedgerEvent @event)
    {
        @event.Id = Guid.NewGuid();
        @event.Timestamp = DateTime.UtcNow;
        @event.Version = await GetNextVersionAsync(@event.AggregateId);

        _db.Events.Add(@event);
        await _db.SaveChangesAsync();
    }

    public async Task AppendBatchAsync(IEnumerable<LedgerEvent> events)
    {
        foreach (var @event in events)
        {
            @event.Id = Guid.NewGuid();
            @event.Timestamp = DateTime.UtcNow;
            @event.Version = await GetNextVersionAsync(@event.AggregateId);
            _db.Events.Add(@event);
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<LedgerEvent>> GetEventsAsync(Guid aggregateId, long? fromVersion = null)
    {
        var query = _db.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version);

        if (fromVersion.HasValue)
            return await query.Where(e => e.Version >= fromVersion.Value).ToListAsync();

        return await query.ToListAsync();
    }

    public async Task<List<LedgerEvent>> GetEventsByTypeAsync(string eventType, DateTime? since = null)
    {
        var query = _db.Events.Where(e => e.EventType == eventType);
        if (since.HasValue)
            query = query.Where(e => e.Timestamp >= since.Value);

        return await query.OrderBy(e => e.Timestamp).ToListAsync();
    }

    private async Task<long> GetNextVersionAsync(Guid aggregateId)
    {
        var maxVersion = await _db.Events
            .Where(e => e.AggregateId == aggregateId)
            .MaxAsync(e => (long?)e.Version) ?? 0;
        return maxVersion + 1;
    }
}

public class LedgerEventDbContext : DbContext
{
    public LedgerEventDbContext(DbContextOptions<LedgerEventDbContext> options) : base(options) { }

    public DbSet<LedgerEvent> Events => Set<LedgerEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerEvent>(entity =>
        {
            entity.ToTable("ledger_events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AggregateId, e.Version }).IsUnique();
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Data).HasColumnType("jsonb");
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
        });
    }
}
