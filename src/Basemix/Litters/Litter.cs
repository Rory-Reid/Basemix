using Basemix.Identity;
using Basemix.Litters.Persistence;
using Basemix.Rats;
using Basemix.Rats.Persistence;

namespace Basemix.Litters;

public class Litter
{
    private readonly List<Offspring> offspring;

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
    public DateOnly? DateOfBirth { get; set; }
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

    public async Task AddOffspring(ILittersRepository repository, Rat rat)
    {
        var result = await repository.AddOffspring(this.Id, rat.Id);
        if (result == AddOffspringResult.Success)
        {
            this.offspring.Add(new Offspring(rat.Id, rat.Name));
        }
    }

    public async Task RemoveOffspring(ILittersRepository repository, Rat rat)
    {
        var result = await repository.RemoveOffspring(this.Id, rat.Id);
        if (result == RemoveOffspringResult.Success)
        {
            this.offspring.RemoveAll(x => x.Id == rat.Id);
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
        var id = await repository.CreateLitter();
        return new Litter(id);
    }
}

public record Offspring(RatIdentity Id, string Name);

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