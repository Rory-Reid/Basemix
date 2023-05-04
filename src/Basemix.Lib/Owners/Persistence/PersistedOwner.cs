namespace Basemix.Lib.Owners.Persistence;

public record PersistedOwner
{
    public PersistedOwner() {}

    public PersistedOwner(Owner owner)
    {
        this.Id = owner.Id.Value;
        this.Name = owner.Name;
        this.Email = owner.Email;
        this.Phone = owner.Phone;
        this.Notes = owner.Notes;
    }
    
    public long? Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }

    public Owner ToModelledOwner() =>
        new(this.Id)
        {
            Name = this.Name,
            Email = this.Email,
            Phone = this.Phone,
            Notes = this.Notes
        };
}

public record PersistedOwnerSearchResult
{
    public long Id { get; set; }
    public string? Name { get; set; }

    public OwnerSearchResult ToResult() =>
        new(this.Id, this.Name);
}