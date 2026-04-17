namespace Basemix;

public interface IPhotoPicker
{
    Task<PhotoPickResult?> PickPhotoAsync();
}

public class PhotoPickResult : IDisposable
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }

    public void Dispose() => this.Stream.Dispose();
}
