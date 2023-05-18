namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public record FamilyRow(
    string? LitterId,
    string? ShowOrFullName,
    string? PetName,
    string? Breeder,
    string? Variety,
    bool? Resp,
    bool? MammaryOrFattyLumps,
    bool? OtherTumours,
    bool? Neuro,
    bool? AbcessInf,
    bool? Heart,
    bool? Kidney,
    bool? Hld,
    string? Aod,
    string? CauseOfDeath1,
    string? CauseOfDeath2,
    string? CauseOfDeathOther,
    string? S1, string? D1,
    string? Ss2, string? Sd2, string? Ds2, string? Dd2,
    string? Sss3, string? Ssd3, string? Sds3, string? Sdd3, string? Dss3, string? Dsd3, string? Dds3, string? Ddd3,
    string? Ssss4, string? Sssd4, string? Ssds4, string? Ssdd4, string? Sdss4, string? Sdsd4, string? Sdds4, string? Sddd4,
    string? Dsss4, string? Dssd4, string? Dsds4, string? Dsdd4, string? Ddss4, string? Ddsd4, string? Ddds4, string? Dddd4,
    string? Sssss5, string? Ssssd5, string? Sssds5, string? Sssdd5, string? Ssdss5, string? Ssdsd5, string? Ssdds5, string? Ssddd5, string? Sdsss5, string? Sdssd5, string? Sdsds5, string? Sdsdd5, string? Sddss5, string? Sddsd5, string? Sddds5, string? Sdddd5,
    string? Dssss5, string? Dsssd5, string? Dssds5, string? Dssdd5, string? Dsdss5, string? Dsdsd5, string? Dsdds5, string? Dsddd5, string? Ddsss5, string? Ddssd5, string? Ddsds5, string? Ddsdd5, string? Dddss5, string? Dddsd5, string? Dddds5, string? Ddddd5)
{
    public bool IsNullFamilyData =>
        this.LitterId is null && this.ShowOrFullName is null && this.PetName is null;
}