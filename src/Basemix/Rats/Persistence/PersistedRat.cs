using Basemix.Persistence;

namespace Basemix.Rats.Persistence;

public record PersistedRat
{
    public PersistedRat() {}
    public PersistedRat(Rat rat)
    {
        this.Name = rat.Name;
        this.Sex = rat.Sex.ToString();
        this.DateOfBirth = rat.DateOfBirth.ToPersistedDateTime();
        this.Notes = rat.Notes;
    }
    
    public long? Id { get; init; }
    public string Name { get; init; } = null!;
    public string Sex { get; init; } = null!;
    public long DateOfBirth { get; init; }
    public string? Genotype { get; init; }
    public string? Variety { get; init; }
    public string? Notes { get; init; }
    public string? BirthNotes { get; init; }
    public string? TypeScore { get; init; }
    public string? TemperamentScore { get; init; }
    public long? DateOfDeath { get; init; }
    public string? DeathReason { get; init; }

    public Rat ToModelledRat()
    {
        var id = this.Id.HasValue ? new RatIdentity(this.Id.Value) : RatIdentity.Anonymous;
        return new Rat(this.Name, Enum.Parse<Sex>(this.Sex), this.DateOfBirth.ToDateOnly(), id)
        {
            Notes = this.Notes
        };
    }
}