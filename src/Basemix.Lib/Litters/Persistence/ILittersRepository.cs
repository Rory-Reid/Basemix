using Basemix.Lib.Rats;

namespace Basemix.Lib.Litters.Persistence;

public interface ILittersRepository
{
    Task<Litter?> GetLitter(long id);
    Task<List<LitterOverview>> SearchLitters(bool? bredByMe = null);
    Task<long> CreateLitter(RatIdentity? damId = null, RatIdentity? sireId = null);
    Task UpdateLitter(Litter litter);
    Task<AddOffspringResult> AddOffspring(LitterIdentity id, RatIdentity ratId);
    Task<RemoveOffspringResult> RemoveOffspring(LitterIdentity id, RatIdentity ratId);
    Task DeleteLitter(LitterIdentity id);
}