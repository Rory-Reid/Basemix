using Basemix.Persistence;
using Basemix.Rats;

namespace Basemix.Litters.Persistence;

public class PersistedLitter
{
    public PersistedLitter() {}

    public PersistedLitter(Litter litter)
    {
        this.Id = litter.Id.Value;
        this.DamId = litter.DamId?.Value;
        this.SireId = litter.SireId?.Value;
        this.DateOfBirth = litter.DateOfBirth?.ToPersistedDateTime();
    }
    
    public long? Id { get; init; }
    public long? DamId { get; init; }
    public long? SireId { get; init; }
    public long? DateOfBirth { get; init; }
}

public class LitterReadModel
{
    public long Id { get; init; }
    public long? DamId { get; init; }
    public string? DamName { get; init; }
    public long? SireId { get; init; }
    public string? SireName { get; init; }
    public long? DateOfBirth { get; init; }

    public Litter ToModelledLitter(IEnumerable<LitterOffspringReadModel> offspring)
    {
        (RatIdentity Id, string Name)?dam = null;
        (RatIdentity Id, string Name)?sire = null;

        if (this.DamId != null)
        {
            dam = new(this.DamId!, this.DamName!);
        }

        if (this.SireId != null)
        {
            sire = new(this.SireId!, this.SireName!);
        }

        return new Litter(this.Id, dam: dam, sire: sire, this.DateOfBirth?.ToDateOnly(),
            offspring.Select(x => x.ToModelledOffspring()).ToList());
    }
}

public class LitterOffspringReadModel
{
    public long Id { get; set; }
    public string? Name { get; set; }
    
    public Offspring ToModelledOffspring() => new Offspring(this.Id, this.Name);
}
