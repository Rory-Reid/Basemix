using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;

namespace Basemix.Tests.sdk;

public class MemoryOwnersRepository : IOwnersRepository
{
    private readonly MemoryPersistenceBackplane backplane;
    public MemoryOwnersRepository(MemoryPersistenceBackplane? backplane = null) =>
        this.backplane = backplane ?? new();

    private NextId NextId => this.backplane.NextOwnerId.Get;
    public Dictionary<OwnerIdentity, Owner> Owners => this.backplane.Owners;

    public Task<Owner?> GetOwner(OwnerIdentity id)
    {
        return this.Owners.TryGetValue(id, out var owner)
            ? Task.FromResult(CopyOf(owner).AsNullable())
            : Task.FromResult((Owner?)null);
    }

    public Task<OwnerIdentity> CreateOwner()
    {
        var id = this.NextId();
        this.Owners.Add(id, new(id));
        return Task.FromResult(new OwnerIdentity(id));
    }

    public Task UpdateOwner(Owner owner)
    {
        if (this.Owners.ContainsKey(owner.Id))
        {
            this.Owners[owner.Id] = CopyOf(owner);
        }
        else
        {
            throw new Exception($":Owner {owner.Id} not found!");
        }

        return Task.CompletedTask;
    }

    public Task DeleteOwner(OwnerIdentity id)
    {
        this.Owners.Remove(id);
        return Task.CompletedTask;
    }

    public Task<List<OwnerSearchResult>> SearchOwner(string? nameSearchTerm = null)
    {
        var results = this.Owners.Values.AsEnumerable();
        
        if (!string.IsNullOrEmpty(nameSearchTerm))
        {
            results = results.Where(r => (r.Name ?? string.Empty).ToLower().Contains(nameSearchTerm.ToLower()));
        }

        return Task.FromResult(results.Select(o => new OwnerSearchResult(o.Id, o.Name)).ToList());
    }
    
    public void Seed(Owner owner) => this.Owners.Add(owner.Id, owner);
    
    private static Owner CopyOf(Owner owner) => new (owner.Id)
        {
            Name = owner.Name,
            Email = owner.Email,
            Phone = owner.Phone,
            Notes = owner.Notes
        };
}