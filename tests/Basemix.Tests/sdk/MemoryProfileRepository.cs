using Basemix.Lib.Settings;
using Basemix.Lib.Settings.Persistence;

namespace Basemix.Tests.sdk;

public class MemoryProfileRepository : IProfileRepository
{
    private readonly MemoryPersistenceBackplane backplane;

    public MemoryProfileRepository(MemoryPersistenceBackplane? backplane = null)
    {
        this.backplane = backplane ?? new();
        var defaultProfile = new Profile {Id = 1, Name = "Default"};
        SetProfileDefaults(defaultProfile);
        this.backplane.Profiles.TryAdd(1, defaultProfile);
    }
    
    public Task<Profile> GetDefaultProfile() =>
        Task.FromResult(CopyOf(this.backplane.Profiles[1]));

    public Task ResetProfileDefaults(long profileId)
    {
        SetProfileDefaults(this.backplane.Profiles[profileId]);
        return Task.CompletedTask;
    }

    public Task UpdateProfileSettings(Profile profile)
    {
        this.backplane.Profiles[profile.Id] = CopyOf(profile);
        return Task.CompletedTask;
    }

    private static void SetProfileDefaults(Profile profile)
    {
        profile.Pedigree = new()
        {
            ShowSex = true,
            Pdf = new()
            {
                PageMargin = 25,
                Font = "Carlito",
                FontSize = 10,
                FooterFontSize = 10,
                HeaderFontSize = 36,
                SubheaderFontSize = 26
            }
        };
        profile.LitterEstimation = new()
        {
            MinBirthDaysAfterPairing = 21,
            MaxBirthDaysAfterPairing = 23,
            MinWeaningDaysAfterBirth = 21,
            MinSeparationDaysAfterBirth = 28,
            MinRehomeDaysAfterBirth = 42
        };
    }

    private static Profile CopyOf(Profile profile) =>
        new()
        {
            Id = profile.Id,
            Name = profile.Name,
            RatteryName = profile.RatteryName,
            Pedigree = new()
            {
                Footer = profile.Pedigree.Footer,
                ShowSex = profile.Pedigree.ShowSex,
                Pdf = new()
                {
                    PageMargin = profile.Pedigree.Pdf.PageMargin,
                    Font = profile.Pedigree.Pdf.Font,
                    FontSize = profile.Pedigree.Pdf.FontSize,
                    FooterFontSize = profile.Pedigree.Pdf.FooterFontSize,
                    HeaderFontSize = profile.Pedigree.Pdf.HeaderFontSize,
                    SubheaderFontSize = profile.Pedigree.Pdf.SubheaderFontSize
                }
            },
            LitterEstimation = new()
            {
                MinBirthDaysAfterPairing = profile.LitterEstimation.MinBirthDaysAfterPairing,
                MaxBirthDaysAfterPairing = profile.LitterEstimation.MaxBirthDaysAfterPairing,
                MinWeaningDaysAfterBirth = profile.LitterEstimation.MinWeaningDaysAfterBirth,
                MinSeparationDaysAfterBirth = profile.LitterEstimation.MinSeparationDaysAfterBirth,
                MinRehomeDaysAfterBirth = profile.LitterEstimation.MinRehomeDaysAfterBirth
            }
        };
}