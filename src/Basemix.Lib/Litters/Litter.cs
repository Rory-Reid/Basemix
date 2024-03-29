using Basemix.Lib.Identity;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;

namespace Basemix.Lib.Litters;

public class Litter
{
    private readonly List<Offspring> offspring;

    public Litter(LitterIdentity? identity = null,
        (RatIdentity Id, string? Name)? dam = null,
        (RatIdentity Id, string? Name)? sire = null,
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
    public string? Name { get; set; }
    public bool BredByMe { get; set; }
    public RatIdentity? DamId { get; private set; }
    public RatIdentity? SireId { get; private set; }
    public string? DamName { get; private set; }
    public string? SireName { get; private set; }
    public DateOnly? DateOfPairing { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<Offspring> Offspring => this.offspring;

    public async Task<LitterAddResult> SetDam(ILittersRepository repository, Rat rat)
    {
        if (rat.Sex != Sex.Doe)
        {
            return LitterAddResult.WrongSex;
        }

        this.DamId = rat.Id;
        this.DamName = rat.Name;
        await repository.UpdateLitter(this);
        return LitterAddResult.Success;
    }

    public async Task<LitterAddResult> SetDam(ILittersRepository repository, RatSearchResult rat)
    {
        if (rat.Sex != Sex.Doe)
        {
            return LitterAddResult.WrongSex;
        }

        this.DamId = rat.Id;
        this.DamName = rat.Name;
        await repository.UpdateLitter(this);
        return LitterAddResult.Success;
    }

    public Task RemoveDam(ILittersRepository repository)
    {
        this.DamId = null;
        this.DamName = null;
        return repository.UpdateLitter(this);
    }

    public async Task<LitterAddResult> SetSire(ILittersRepository repository, Rat rat)
    {
        if (rat.Sex != Sex.Buck)
        {
            return LitterAddResult.WrongSex;
        }

        this.SireId = rat.Id;
        this.SireName = rat.Name;
        await repository.UpdateLitter(this);
        return LitterAddResult.Success;
    }
    
    public async Task<LitterAddResult> SetSire(ILittersRepository repository, RatSearchResult rat)
    {
        if (rat.Sex != Sex.Buck)
        {
            return LitterAddResult.WrongSex;
        }

        this.SireId = rat.Id;
        this.SireName = rat.Name;
        await repository.UpdateLitter(this);
        return LitterAddResult.Success;
    }

    public Task RemoveSire(ILittersRepository repository)
    {
        this.SireId = null;
        this.SireName = null;
        return repository.UpdateLitter(this);
    }

    public async Task AddOffspring(ILittersRepository repository, Rat rat)
    {
        var result = await repository.AddOffspring(this.Id, rat.Id);
        if (result == AddOffspringResult.Success)
        {
            this.offspring.Add(new Offspring(rat.Id, rat.Name));
        }
    }
    
    public async Task AddOffspring(ILittersRepository repository, RatSearchResult rat)
    {
        var result = await repository.AddOffspring(this.Id, rat.Id);
        if (result == AddOffspringResult.Success)
        {
            this.offspring.Add(new Offspring(rat.Id, rat.Name));
        }
    }

    public async Task RemoveOffspring(ILittersRepository repository, RatIdentity ratId)
    {
        var result = await repository.RemoveOffspring(this.Id, ratId);
        if (result == RemoveOffspringResult.Success)
        {
            this.offspring.RemoveAll(x => x.Id == ratId);
        }
    }

    public async Task<CreateMultipleResult> CreateMultipleOffspring(ILittersRepository littersRepository, IRatsRepository ratsRepository,
        int amount)
    {
        // TODO - write tests once rats are fixed
        if (this.DamId == null)
        {
            return CreateMultipleResult.NoDam;
        }

        if (this.SireId == null)
        {
            return CreateMultipleResult.NoSire;
        }

        if (this.DateOfBirth == null)
        {
            return CreateMultipleResult.NoDateOfBirth;
        }
        
        var count = this.Offspring.Count;
        for (var i = 0; i < amount; i++)
        {
            // Todo - should be transactional
            var rat = await Rat.Create(ratsRepository);
            rat.Name = $"{this.DamName} & {this.SireName}'s offspring ({count + i})";
            rat.DateOfBirth = this.DateOfBirth;
            await rat.Save(ratsRepository);

            await this.AddOffspring(littersRepository, rat);
        }

        return CreateMultipleResult.Success;
    }

    public Task Save(ILittersRepository repository) => repository.UpdateLitter(this);

    public static async Task<Litter> Create(ILittersRepository repository)
    {
        // TODO: Don't like how this is a double-source-of-truth. Consider other patterns
        // If this returns a litter in a state that doesn't match the database defaults then it could
        // introduce subtle errors. I don't like having no defaults in the db but I also don't like the
        // concept of creating and reading then returning from db. Maybe I'll have to suck it up and do that.
        // Consider:
        //  - Test which validates consistency of `item.Create` and result from `repository.Get` for litter/rat/owner
        //  - Expand repository.create method to take parameters for some fields. This method can then create with
        //    defaults and return them and it'll at least be guaranteed consistent at creation via this.
        //  - Decide if 'create-and-read' really is all that bad
        var id = await repository.CreateLitter();
        return new Litter(id)
        {
            BredByMe = true
        };
    }
}

public record Offspring(RatIdentity Id, string? Name, string? OwnerName = null);

public enum LitterAddResult
{
    Error = default,
    Success,
    WrongSex
}

public enum CreateMultipleResult
{
    Error = default,
    Success,
    NoDam,
    NoSire,
    NoDateOfBirth
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

    public override bool Equals(object? obj)
    {
        if (obj is LitterIdentity id)
        {
            return this.Value == id.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.idValue.GetHashCode();
    }

    public static implicit operator long(LitterIdentity id) => id.Value;
    public static implicit operator LitterIdentity(long id) => new(id);
    public static implicit operator long?(LitterIdentity? id) => id?.Value;
    public static implicit operator LitterIdentity?(long? id) => id == null ? null : new(id.Value);
    public static bool operator ==(LitterIdentity? lhs, LitterIdentity? rhs) => 
        (lhs is null || rhs is null)
            ? lhs is null && rhs is null
            : lhs.Equals(rhs);
    
    public static bool operator !=(LitterIdentity? lhs, LitterIdentity? rhs) => 
        (lhs is null || rhs is null)
            ? !(lhs is null && rhs is null)
            : !lhs.Equals(rhs);

    public static LitterIdentity Anonymous => new();
}