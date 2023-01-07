using Basemix.Rats;

namespace Basemix.Tests.sdk;

public static class MemoryRepositoryExtensions
{
    public static async Task<Rat> Seed(this MemoryRatsRepository repository, Rat rat)
    {
        var id = await repository.AddRat(rat);
        return repository.Rats[id];
    }
}