using Basemix.Lib.Pedigrees;
using Basemix.Lib.Pedigrees.Persistence;
using Basemix.Lib.Rats;

namespace Basemix.Tests.sdk;

public class MemoryPedigreesRepository : IPedigreeRepository
{
    private readonly MemoryPersistenceBackplane backplane;
    
    public Dictionary<RatIdentity, Node> Pedigrees => this.backplane.Pedigrees;

    public MemoryPedigreesRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
    }
        
    public Task<Node?> GetPedigree(RatIdentity id)
    {
        this.Pedigrees.TryGetValue(id, out var pedigree);
        return Task.FromResult(pedigree);
    }
}