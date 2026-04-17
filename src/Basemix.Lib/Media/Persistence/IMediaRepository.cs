namespace Basemix.Lib.Media.Persistence;

public interface IMediaRepository
{
    Task SavePhoto(string id, Stream imageStream, string originalFilename, int maxResolution, bool compress);
    Task<RatPhoto?> GetPhoto(string id);
    Task DeletePhoto(string id);
}
