namespace Basemix.Lib.Media;

public class RatPhoto
{
    public string Id { get; set; } = null!;
    public string ImageFormat { get; set; } = null!;
    public int StoredWidth { get; set; }
    public int StoredHeight { get; set; }
    public byte[] Data { get; set; } = null!;

    public string ToDataUri() =>
        $"data:image/{this.ImageFormat};base64,{Convert.ToBase64String(this.Data)}";
}
