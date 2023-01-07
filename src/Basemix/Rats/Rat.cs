using Basemix.Identity;

namespace Basemix.Rats;

public class Rat
{
    public Rat(string name, Sex sex, DateOnly dateOfBirth, RatIdentity? id = null)
    {
        this.Name = name;
        this.Sex = sex;
        this.DateOfBirth = dateOfBirth;
        this.Id = id ?? RatIdentity.Anonymous;
    }
    
    public RatIdentity Id { get; init; }
    public string Name { get; set; }
    public Sex Sex { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? Notes { get; set; }
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