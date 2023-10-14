using Basemix.Lib.Litters;
using Basemix.Lib.Owners;
using Basemix.Lib.Pedigrees;
using Basemix.Lib.Rats;
using Basemix.Lib.Settings;
using Bogus;

namespace Basemix.Tests.sdk;

public class MemoryPersistenceBackplane
{
    private Faker faker = new();
    
    public Sequence NextRatId { get; } = new();
    public Dictionary<RatIdentity, Rat> Rats { get; } = new();
    public Sequence NextLitterId { get; } = new();
    public Dictionary<LitterIdentity, Litter> Litters { get; } = new();
    public Sequence NextOwnerId { get; } = new();
    public Dictionary<OwnerIdentity, Owner> Owners { get; } = new();
    
    public Dictionary<RatIdentity, Node> Pedigrees { get; } = new();
    public Dictionary<long, Profile> Profiles { get; } = new();

    public void Seed(Litter litter)
    {
        if (litter.DamId != null && !this.Rats.ContainsKey(litter.DamId))
        {
            this.Seed(this.faker.Rat(id: litter.DamId, name: litter.DamName));
        }

        if (litter.SireId != null && !this.Rats.ContainsKey(litter.SireId))
        {
            this.Seed(this.faker.Rat(id: litter.SireId, name: litter.SireName));
        }

        foreach (var offspring in litter.Offspring)
        {
            this.Seed(this.faker.Rat(offspring.Id, name: offspring.Name));
        }
        
        this.Litters.Add(litter.Id, litter);
    }

    public void Seed(Rat rat) => this.Rats.Add(rat.Id, rat);
    
    public void Seed(Owner owner) => this.Owners.Add(owner.Id, owner);
}