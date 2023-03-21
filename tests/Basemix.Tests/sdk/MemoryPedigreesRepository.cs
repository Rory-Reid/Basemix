using Basemix.Lib.Pedigrees;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public class MemoryPedigreesRepository : IPedigreeRepository
{
    private readonly MemoryPersistenceBackplane backplane;

    public MemoryPedigreesRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
    }
        
    public Task<Node?> GetPedigree(RatIdentity id)
    {
        return Task.FromResult((Node?) null);
    }
}