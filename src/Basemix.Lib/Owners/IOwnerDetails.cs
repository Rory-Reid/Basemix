namespace Basemix.Lib.Owners;

public interface IOwnerDetails
{
    public OwnerIdentity Id { get; }
    public string? Name { get; }
}