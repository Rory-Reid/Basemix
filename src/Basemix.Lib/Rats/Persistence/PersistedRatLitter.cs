namespace Basemix.Lib.Rats.Persistence;

public class PersistedRatLitter
{
    public long Id { get; set; }
    public long? DateOfBirth { get; set; }
    public string? DamName { get; set; }
    public string? SireName { get; set; }
    public int OffspringCount { get; set; }
}