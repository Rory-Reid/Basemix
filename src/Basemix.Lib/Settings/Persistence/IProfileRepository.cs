namespace Basemix.Lib.Settings.Persistence;

public interface IProfileRepository
{
    public Task<Profile> GetDefaultProfile();
    public Task ResetProfileDefaults(long profileId);
    public Task UpdateProfileSettings(Profile profile);
}