using Basemix.Lib.Settings.Persistence;

namespace Basemix.Tests.sdk;

public class MemoryOptionsRepository : IOptionsRepository
{
    private readonly MemoryPersistenceBackplane backplane;
    
    public MemoryOptionsRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
    }

    public Task<List<DeathReason>> GetDeathReasons()
    {
        return Task.FromResult(this.backplane.DeathReasons.Values.ToList());
    }
}