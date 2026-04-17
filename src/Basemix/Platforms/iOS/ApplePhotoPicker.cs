namespace Basemix.Platforms.iOS;

public class ApplePhotoPicker : IPhotoPicker
{
    public async Task<PhotoPickResult?> PickPhotoAsync()
    {
        var results = await MediaPicker.Default.PickPhotosAsync();
        var result = results?.FirstOrDefault();
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
