namespace Basemix.Litters;

public class LitterOverview
{
    public LitterIdentity Id { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Dam { get; init; }
    public string? Sire { get; init; }
    public int OffspringCount { get; init; }
}