using Basemix.Lib.Identity;
using Basemix.Lib.Litters;
using Basemix.Lib.Owners;
using Basemix.Lib.Rats.Persistence;

namespace Basemix.Lib.Rats;

public class Rat
{
    public Rat(RatIdentity? id = null,
        string? name = null,
        Sex? sex = null,
        string? variety = null,
        DateOnly? dateOfBirth = null,
        List<RatLitter>? litters = null,
        OwnerIdentity? ownerId = null,
        string? ownerName = null)
    {
        this.Id = id ?? RatIdentity.Anonymous;
        this.Name = name;
        this.Sex = sex;
        this.Variety = variety;
        this.DateOfBirth = dateOfBirth;
        this.Litters = litters ?? new();
        this.OwnerId = ownerId;
        this.OwnerName = ownerName;
    }
    
    public RatIdentity Id { get; }
    public bool Owned { get; set; }
    public OwnerIdentity? OwnerId { get; private set; }
    public string? OwnerName { get; private set; }
    public string? Name { get; set; }
    public Sex? Sex { get; set; }
    public string? Variety { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? DateOfDeath { get; set; }
    public string? Notes { get; set; }
    
    public List<RatLitter> Litters { get; }

    public async Task<OwnerAddResult> SetOwner(IRatsRepository repository, IOwnerDetails owner)
    {
        if (this.Owned)
        {
            return OwnerAddResult.OwnedByUser;
        }

        this.OwnerId = owner.Id;
        this.OwnerName = owner.Name;
        await repository.UpdateRat(this);
        return OwnerAddResult.Success;
    }

    public Task RemoveOwner(IRatsRepository repository)
    {
        this.OwnerId = null;
        this.OwnerName = null;
        return repository.UpdateRat(this);
    }
    
    public TimeSpan? Age(NowDateOnly now)
    {
        if (this.DateOfBirth == null)
        {
            return null;
        }

        var maxAge = this.DateOfDeath ?? now();
        return new TimeSpan(maxAge.DayNumber - this.DateOfBirth.Value.DayNumber, 0, 0, 0);
    }
    
    public Task Save(IRatsRepository repository) => repository.UpdateRat(this);
    
    public static async Task<Rat> Create(IRatsRepository repository)
    {
        var id = await repository.CreateRat();
        return new Rat(id)
        {
            Owned = true
        };
    }

    public RatSearchResult ToSearchResult()
    {
        return new RatSearchResult(this.Id, this.Name, this.Sex, this.DateOfBirth);
    }
}

public record RatLitter(LitterIdentity Id, DateOnly? DateOfBirth, string? PairedWith, int OffspringCount);

public enum OwnerAddResult
{
    Error = default,
    Success,
    OwnedByUser
}

public class RatIdentity
{
    private RatIdentity() {}
    public RatIdentity(long id)
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

    public bool IsAnonymous => this.idValue == null;

    public override bool Equals(object? obj)
    {
        if (obj is RatIdentity id)
        {
            return this.Value == id.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.idValue.GetHashCode();
    }

    public static implicit operator long(RatIdentity id) => id.Value;
    public static implicit operator RatIdentity(long id) => new(id);
    public static implicit operator long?(RatIdentity? id) => id?.Value;
    public static implicit operator RatIdentity?(long? id) => id == null ? null : new(id.Value);
    public static bool operator ==(RatIdentity? lhs, RatIdentity? rhs) => 
        (lhs is null || rhs is null)
        ? lhs is null && rhs is null
        : lhs.Equals(rhs);
    
    public static bool operator !=(RatIdentity? lhs, RatIdentity? rhs) => 
        (lhs is null || rhs is null)
            ? !(lhs is null && rhs is null)
            : !lhs.Equals(rhs);

    public static RatIdentity Anonymous => new();
}