namespace Basemix.Platforms.MacCatalyst;

public class ApplePhotoPicker : IPhotoPicker
{
    public async Task<PhotoPickResult?> PickPhotoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a photo",
            FileTypes = FilePickerFileType.Images
        });
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
