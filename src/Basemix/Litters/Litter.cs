using System.Threading.Channels;
using Basemix.Identity;
using Basemix.Litters.Persistence;
using Basemix.Rats;

namespace Basemix.Litters;

public class Litter
{
    private List<Offspring> offspring;

    public Litter(LitterIdentity? identity = null,
        (RatIdentity Id, string Name)? dam = null,
        (RatIdentity Id, string Name)? sire = null,
        DateOnly? dateOfBirth = null,
        List<Offspring>? offspring = null)
    {
        this.Id = identity ?? LitterIdentity.Anonymous;
        if (dam != null)
        {
            this.DamId = dam.Value.Id;
            this.DamName = dam.Value.Name;
        }

        if (sire != null)
        {
            this.SireId = sire.Value.Id;
            this.SireName = sire.Value.Name;
        }

        this.DateOfBirth = dateOfBirth;
        this.offspring = offspring ?? new();
    }

    public LitterIdentity Id { get; }
    public RatIdentity? DamId { get; private set; }
    public RatIdentity? SireId { get; private set; }
    public string? DamName { get; private set; }
    public string? SireName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public IReadOnlyList<Offspring> Offspring => this.offspring;

    public async Task<LitterAddResult> SetDam(LittersRepository litterRepository, Rat rat)
    {
        if (rat.Sex != Sex.Doe)
        {
            return LitterAddResult.WrongSex;
        }

        // Save
        return LitterAddResult.Success;
    }

    public async Task<LitterAddResult> SetSire(LittersRepository litterRepository, Rat rat)
    {
        if (rat.Sex != Sex.Buck)
        {
            return LitterAddResult.WrongSex;
        }

        // Save
        return LitterAddResult.Success;
    }
}

public record Offspring(RatIdentity Id, string Name);

public enum LitterAddResult
{
    Error = default,
    Success,
    WrongSex
}

public class LitterIdentity
{
    private LitterIdentity() {}
    public LitterIdentity(long id)
    {
        if (id <= 0)
        {
            throw new InvalidIdentityException($"Id must be a positive number, not negative or zero. {id} is invalid");
        }
        
        this.idValue = id;
    }
        
    private readonly long? idValue = null;

    public long Value
    {
        get
        {
            if (this.idValue == null)
            {
                throw new NoIdentityException("Anonymous ID cannot be retrieved");
            }

            return this.idValue.Value;
        }
    }

    public static implicit operator long(LitterIdentity id) => id.Value;
    public static implicit operator LitterIdentity(long id) => new(id);

    public static LitterIdentity Anonymous => new();
}