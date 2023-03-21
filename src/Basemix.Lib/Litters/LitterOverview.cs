namespace Basemix.Lib.Litters;

public class LitterOverview
{
    public LitterOverview(LitterIdentity id) => this.Id = id;
    
    public LitterIdentity Id { get; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Dam { get; init; }
    public string? Sire { get; init; }
    public int OffspringCount { get; init; }
}