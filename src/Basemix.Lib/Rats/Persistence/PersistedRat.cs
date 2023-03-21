using Basemix.Lib.Persistence;

namespace Basemix.Lib.Rats.Persistence;

public record PersistedRat
{
    public PersistedRat() {}
    public PersistedRat(Rat rat)
    {
        this.Id = rat.Id.IsAnonymous ? null : rat.Id;
        this.Name = rat.Name;
        this.Sex = rat.Sex?.ToString();
        this.Variety = rat.Variety;
        this.DateOfBirth = rat.DateOfBirth?.ToPersistedDateTime();
        this.Notes = rat.Notes;
    }
    
    public long? Id { get; init; }
    public string? Name { get; init; }
    public string? Sex { get; init; }
    public long? DateOfBirth { get; init; }
    public string? Genotype { get; init; }
    public string? Variety { get; init; }
    public string? Notes { get; init; }
    public string? BirthNotes { get; init; }
    public string? TypeScore { get; init; }
    public string? TemperamentScore { get; init; }
    public long? DateOfDeath { get; init; }
    public string? DeathReason { get; init; }

    public Rat ToModelledRat(List<PersistedRatLitter>? litters = null)
    {
        var id = this.Id.HasValue ? new RatIdentity(this.Id.Value) : RatIdentity.Anonymous;
        Sex? sex = !string.IsNullOrEmpty(this.Sex) ? Enum.Parse<Sex>(this.Sex) : null;
        var dateOfBirth = this.DateOfBirth?.ToDateOnly();
        return new Rat(id, this.Name, sex, this.Variety, dateOfBirth,
            litters?.Select(x =>
                new RatLitter(
                    x.Id,
                    x.DateOfBirth?.ToDateOnly(),
                    sex == Lib.Rats.Sex.Buck ? x.DamName : x.SireName,
                    x.OffspringCount)).ToList())
        {
            Notes = this.Notes
        };
    }
}