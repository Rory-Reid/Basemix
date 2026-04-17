namespace Basemix.Tests.sdk;

public class NullPhotoPicker : IPhotoPicker
{
    public PhotoPickResult? NextResult { get; set; }

    public Task<PhotoPickResult?> PickPhotoAsync()
    {
        return Task.FromResult(this.NextResult);
    }
}
