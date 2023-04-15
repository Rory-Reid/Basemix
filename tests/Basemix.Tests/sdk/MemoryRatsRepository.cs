using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;

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
        var rat = new Rat(this.NextId()) { Owned = true };
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

    public Task DeleteRat(long id)
    {
        this.Rats.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<RatSearchResult>> SearchRat(string? nameSearchTerm = null, bool? deceased = null,
        bool? owned = null, Sex? sex = null)
    {
        var results = this.Rats.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(nameSearchTerm))
        {
            results = results.Where(r => (r.Name ?? string.Empty).ToLower().Contains(nameSearchTerm.ToLower()));
        }

        results = deceased switch
        {
            true => results.Where(r => r.DateOfDeath.HasValue),
            false => results.Where(r => !r.DateOfDeath.HasValue),
            _ => results
        };

        results = owned switch
        {
            true => results.Where(r => r.Owned),
            false => results.Where(r => !r.Owned),
            _ => results
        };

        results = sex switch
        {
            Sex.Doe => results.Where(r => r.Sex == Sex.Doe),
            Sex.Buck => results.Where(r => r.Sex == Sex.Buck),
            _ => results
        };

        return Task.FromResult(results.Select(r => r.ToSearchResult()).ToList());
    }

    private static Rat CopyOf(Rat rat, RatIdentity id) =>
        new(id, rat.Name, rat.Sex, rat.Variety, rat.DateOfBirth)
        {
            Notes = rat.Notes
        };
}