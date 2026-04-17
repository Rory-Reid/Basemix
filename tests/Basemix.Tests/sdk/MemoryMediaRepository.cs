using Basemix.Lib.Media;
using Basemix.Lib.Media.Persistence;

namespace Basemix.Tests.sdk;

public class MemoryMediaRepository : IMediaRepository
{
    public Dictionary<string, RatPhoto> Photos { get; } = new();

    public Task SavePhoto(string id, Stream imageStream, string originalFilename, int maxResolution, bool compress)
    {
        using var ms = new MemoryStream();
        imageStream.CopyTo(ms);
        this.Photos[id] = new RatPhoto
        {
            Id = id,
            ImageFormat = "jpeg",
            StoredWidth = maxResolution,
            StoredHeight = maxResolution,
            Data = ms.ToArray()
        };
        return Task.CompletedTask;
    }

    public Task<RatPhoto?> GetPhoto(string id)
    {
        this.Photos.TryGetValue(id, out var photo);
        return Task.FromResult(photo);
    }

    public Task DeletePhoto(string id)
    {
        this.Photos.Remove(id);
        return Task.CompletedTask;
    }
}
