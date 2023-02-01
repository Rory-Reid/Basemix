using Basemix.Rats;

namespace Basemix.Pedigrees.Persistence;

public interface IPedigreeRepository
{
    Task<Node?> GetPedigree(RatIdentity id);
}