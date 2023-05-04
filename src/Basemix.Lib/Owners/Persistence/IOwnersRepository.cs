namespace Basemix.Lib.Owners.Persistence;

public interface IOwnersRepository
{
    public Task<Owner?> GetOwner(OwnerIdentity id);
    public Task<OwnerIdentity> CreateOwner();
    public Task UpdateOwner(Owner owner);
    public Task DeleteOwner(OwnerIdentity id);
    Task<List<OwnerSearchResult>> SearchOwner(string? nameSearchTerm = null);
}
