using Basemix.Litters;
using Basemix.Rats;

namespace Basemix.Tests.sdk;

public class MemoryPersistenceBackplane
{
    public Sequence NextRatId { get; } = new();
    public Dictionary<RatIdentity, Rat> Rats { get; } = new();
    public Sequence NextLitterId { get; } = new();
    public Dictionary<LitterIdentity, Litter> Litters { get; } = new();
}