using Basemix.Rats;
using Basemix.Rats.Persistence;

namespace Basemix.Tests.sdk;

public class MemoryRatsRepository : IRatsRepository
{
    private readonly MemoryPersistenceBackplane backplane;
    
    public MemoryRatsRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
    }

    private NextId NextId => this.backplane.NextRatId.Get;
    public Dictionary<RatIdentity, Rat> Rats => this.backplane.Rats;

    public Task<long> CreateRat()
    {
        var rat = new Rat(this.NextId());
        this.Rats.Add(rat.Id, rat);
        return Task.FromResult(rat.Id.Value);
    }

    public Task<long> AddRat(Rat rat)
    {
        var id = this.NextId();
        this.Rats.Add(id, CopyOf(rat, id));
        return Task.FromResult(id);
    }

    public Task UpdateRat(Rat rat)
    {
        this.Rats[rat.Id] = CopyOf(rat, rat.Id);
        return Task.CompletedTask;
    }

    public Task<Rat?> GetRat(long id)
    {
        this.Rats.TryGetValue(id, out var rat);
        return Task.FromResult(rat);
    }

    public Task<List<Rat>> GetAll() =>
        Task.FromResult(this.Rats.Values.ToList());

    private static Rat CopyOf(Rat rat, RatIdentity id) =>
        new(id, rat.Name, rat.Sex, rat.DateOfBirth)
        {
            Notes = rat.Notes
        };
}