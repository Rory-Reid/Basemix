using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public class MemoryLittersRepository : ILittersRepository
{
    private readonly MemoryPersistenceBackplane backplane;

    public MemoryLittersRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
    }
    
    private NextId NextId => this.backplane.NextLitterId.Get;
    public Dictionary<LitterIdentity, Litter> Litters => this.backplane.Litters;
    
    public Task<Litter?> GetLitter(long id)
    {
        return this.Litters.TryGetValue(id, out var litter)
            ? Task.FromResult(CopyOf(litter, this.UpdateNames(litter.Offspring)).AsNullable())
            : Task.FromResult((Litter?)null);
    }

    public Task<List<LitterOverview>> GetAll() =>
        Task.FromResult(this.Litters.Values.Select(x => new LitterOverview(x.Id)
        {
            DateOfBirth = x.DateOfBirth,
            Dam = x.DamName,
            Sire = x.SireName,
            OffspringCount = x.Offspring.Count
        }).ToList());

    public Task<long> CreateLitter(RatIdentity? damId = null, RatIdentity? sireId = null)
    {
        var id = this.NextId();

        (RatIdentity Id, string? Name)? dam = null;
        (RatIdentity Id, string? Name)? sire = null;
        
        if (damId != null)
        {
            if (!this.backplane.Rats.TryGetValue(damId, out var damRat))
            {
                throw new Exception("Dam Id not found");
            }

            dam = new(damId, damRat.Name);
        }

        if (sireId != null)
        {
            if (!this.backplane.Rats.TryGetValue(sireId, out var sireRat))
            {
                throw new Exception("Sire Id not found");
            }

            sire = new(sireId, sireRat.Name);
        }
        
        this.Litters.Add(id, new Litter(id, dam: dam, sire: sire));
        return Task.FromResult(id);
    }

    public Task UpdateLitter(Litter litter)
    {
        if (this.Litters.ContainsKey(litter.Id))
        {
            this.Litters[litter.Id] = CopyOf(litter);
        }
        else
        {
            throw new Exception($"Litter {litter.Id} not found!");
        }

        return Task.CompletedTask;
    }

    public Task<AddOffspringResult> AddOffspring(LitterIdentity id, RatIdentity ratId)
    {
        if (this.backplane.Rats.TryGetValue(ratId, out var rat) &&
            this.Litters.TryGetValue(id, out var litter))
        {
            var offspring = litter.Offspring.ToList();
            offspring.Add(new Offspring(ratId, rat.Name));
            this.Litters[id] = CopyOf(litter, offspring);
            
            return Task.FromResult(AddOffspringResult.Success);
        }

        return Task.FromResult(AddOffspringResult.NonExistantRatOrLitter);
    }

    public Task<RemoveOffspringResult> RemoveOffspring(LitterIdentity id, RatIdentity ratId)
    {
        var litter = this.Litters[id];
        var rat = litter.Offspring.SingleOrDefault(x => x.Id == ratId);
        if (rat == null)
        {
            return Task.FromResult(RemoveOffspringResult.NothingToRemove);
        }
        
        var offspring = litter.Offspring.ToList();
        offspring.Remove(rat);
        this.Litters[id] = CopyOf(litter, offspring);
        return Task.FromResult(RemoveOffspringResult.Success);

    }

    public Task DeleteLitter(LitterIdentity id)
    {
        this.Litters.Remove(id);
        return Task.CompletedTask;
    }

    public void Seed(Litter litter) => this.backplane.Seed(litter);

    private List<Offspring> UpdateNames(IEnumerable<Offspring> offspring) =>
        offspring.Select(x => x with {Name = this.backplane.Rats[x.Id].Name}).ToList();
    
    private static Litter CopyOf(Litter litter, List<Offspring>? newOffspring = null) =>
        new(
            litter.Id,
            litter.DamId == null
                ? null
                : (litter.DamId, litter.DamName!),
            litter.SireId == null
                ? null
                : (litter.SireId, litter.SireName!),
            litter.DateOfBirth,
            newOffspring ?? litter.Offspring.ToList());
}