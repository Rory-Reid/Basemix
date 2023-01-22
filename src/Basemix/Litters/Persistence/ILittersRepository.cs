using Basemix.Rats;

namespace Basemix.Litters.Persistence;

public interface ILittersRepository
{
    Task<Litter?> GetLitter(long id);
    Task<List<LitterOverview>> GetAll();
    Task<long> CreateLitter(RatIdentity? damId = null, RatIdentity? sireId = null);
    Task UpdateLitter(Litter litter);
    Task<AddOffspringResult> AddOffspring(LitterIdentity id, RatIdentity ratId);
    Task<RemoveOffspringResult> RemoveOffspring(LitterIdentity id, RatIdentity ratId);
    Task DeleteLitter(LitterIdentity id);
}