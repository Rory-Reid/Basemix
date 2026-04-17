namespace Basemix.Platforms.Android;

public class AndroidPhotoPicker : IPhotoPicker
{
    public async Task<PhotoPickResult?> PickPhotoAsync()
    {
#pragma warning disable CS0618 // PickPhotosAsync requires different handling for single pick
        var result = await MediaPicker.Default.PickPhotoAsync();
#pragma warning restore CS0618
        if (result == null)
        {
            return null;
        }

        var stream = await result.OpenReadAsync();
        return new PhotoPickResult
        {
            Stream = stream,
            FileName = result.FileName
        };
    }
}
