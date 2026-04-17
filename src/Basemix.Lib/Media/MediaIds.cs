namespace Basemix.Lib.Media;

/// <summary>
/// Deterministic, composite media ids. Using composite ids (rather than auto-incremented
/// integers) lets the rat reference its photo *before* the blob has been written, so that any
/// failure during photo save leaves a dangling reference (visible to the user as a missing
/// photo placeholder, recoverable by re-uploading) rather than an orphaned blob.
/// </summary>
public static class MediaIds
{
    public static string RatProfilePhoto(long ratId) => $"rat:{ratId}:profile";
}
