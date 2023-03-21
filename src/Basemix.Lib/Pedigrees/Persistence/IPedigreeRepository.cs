using Basemix.Lib.Rats;

namespace Basemix.Lib.Pedigrees.Persistence;

public interface IPedigreeRepository
{
    Task<Node?> GetPedigree(RatIdentity id);
}