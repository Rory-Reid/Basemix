using Basemix.Lib.Identity;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats;

namespace Basemix.Lib.Owners;

public class Owner
{
    public Owner(OwnerIdentity? identity = null)
    {
        this.Id = identity ?? OwnerIdentity.Anonymous;
    }
    
    public OwnerIdentity Id { get; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    public Task Save(IOwnersRepository repository) => repository.UpdateOwner(this);

    public static async Task<Owner> Create(IOwnersRepository repository)
    {
        var id = await repository.CreateOwner();
        return new Owner(id);
    }
}

public class OwnerIdentity
{
    private OwnerIdentity() {}
    public OwnerIdentity(long id)
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
        if (obj is OwnerIdentity id)
        {
            return this.Value == id.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.idValue.GetHashCode();
    }

    public static implicit operator long(OwnerIdentity id) => id.Value;
    public static implicit operator OwnerIdentity(long id) => new(id);
    public static implicit operator long?(OwnerIdentity? id) => id?.Value;
    public static implicit operator OwnerIdentity?(long? id) => id == null ? null : new(id.Value);
    public static bool operator ==(OwnerIdentity? lhs, OwnerIdentity? rhs) => 
        (lhs is null || rhs is null)
            ? lhs is null && rhs is null
            : lhs.Equals(rhs);
    
    public static bool operator !=(OwnerIdentity? lhs, OwnerIdentity? rhs) => 
        (lhs is null || rhs is null)
            ? !(lhs is null && rhs is null)
            : !lhs.Equals(rhs);

    public static OwnerIdentity Anonymous => new();
}