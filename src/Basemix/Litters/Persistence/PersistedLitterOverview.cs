using Basemix.Persistence;

namespace Basemix.Litters.Persistence;

public class PersistedLitterOverview
{
    public long Id { get; init; }
    public long? DateOfBirth { get; init; }
    public string? Dam { get; init; }
    public string? Sire { get; init; }
    public int OffspringCount { get; init; }

    public LitterOverview ToModelledOverview() =>
        new(this.Id)
        {
            DateOfBirth = this.DateOfBirth?.ToDateOnly(),
            Dam = this.Dam,
            Sire = this.Sire,
            OffspringCount = this.OffspringCount
        };
}