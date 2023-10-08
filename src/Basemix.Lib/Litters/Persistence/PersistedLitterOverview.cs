using Basemix.Lib.Persistence;

namespace Basemix.Lib.Litters.Persistence;

public class PersistedLitterOverview
{
    public long Id { get; init; }
    public long? DateOfBirth { get; init; }
    public string? Name { get; init; }
    public string? Dam { get; init; }
    public string? Sire { get; init; }
    public int OffspringCount { get; init; }

    public LitterOverview ToModelledOverview() =>
        new(this.Id)
        {
            DateOfBirth = this.DateOfBirth?.ToDateOnly(),
            Name = this.Name,
            Dam = this.Dam,
            Sire = this.Sire,
            OffspringCount = this.OffspringCount
        };
}