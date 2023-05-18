using ExcelDataReader;

namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public static class ExcelDataReaderExtensions
{
    public static RatRow ReadRatDetailsRow(this IExcelDataReader reader)
    {
        var litterIdentifier = reader.GetValue(0) as string;
        var ratName = reader.GetValue(1) as string;
        var petName = reader.GetValue(2) as string;
        var variety = reader.GetValue(3) as string;
        var sex = (reader.GetValue(4) as string)?.ToLower() switch
        {
            "d" => RatRow.SexType.Doe,
            "b" => RatRow.SexType.Buck,
            _ => (RatRow.SexType?)null
        };
        var ear = (reader.GetValue(5) as string)?.ToLower() switch
        {
            "d" => RatRow.EarType.Dumbo,
            "t" => RatRow.EarType.TopEared,
            _ => (RatRow.EarType?)null
        };
        var eye = (reader.GetValue(6) as string)?.ToLower() switch
        {
            "b" => RatRow.EyeType.Black,
            "r" => RatRow.EyeType.RedOrRuby,
            "p" => RatRow.EyeType.Pink,
            _ => (RatRow.EyeType?)null
        };
        var coat = (reader.GetValue(7) as string)?.ToLower() switch
        {
            "s" => RatRow.CoatType.StandardSmooth,
            "dr" => RatRow.CoatType.DoubleRex,
            "h" => RatRow.CoatType.Hairless,
            "r" => RatRow.CoatType.Rex,
            "sa" => RatRow.CoatType.Satin,
            "v" => RatRow.CoatType.Velvet,
            _ => (RatRow.CoatType?)null
        };
        var marking = (reader.GetValue(8) as string)?.ToLower() switch
        {
            "s" => RatRow.MarkingType.Self,
            "b" => RatRow.MarkingType.Berkshire,
            "ba" => RatRow.MarkingType.Badger,
            "bb" => RatRow.MarkingType.Bareback,
            "c" => RatRow.MarkingType.Capped,
            "ce" => RatRow.MarkingType.CappedEssex,
            "ch" => RatRow.MarkingType.Chinchilla,
            "d" => RatRow.MarkingType.Downunder,
            "e" => RatRow.MarkingType.Essex,
            "ho" => RatRow.MarkingType.Hooded,
            "hod" => RatRow.MarkingType.HoodedDownunder,
            "i" => RatRow.MarkingType.Irish,
            "me" => RatRow.MarkingType.Merle,
            "r" => RatRow.MarkingType.Roan,
            "spd" => RatRow.MarkingType.SpottedDownunder,
            "sq" => RatRow.MarkingType.Squirrel,
            "sr" => RatRow.MarkingType.StripedRoan,
            "t" => RatRow.MarkingType.Turpin,
            "v" => RatRow.MarkingType.Variegated,
            _ => (RatRow.MarkingType?)null
        };
        var shading = (reader.GetValue(9) as string)?.ToLower() switch
        {
            "a" => RatRow.ShadingType.ArgenteCreme,
            "b" => RatRow.ShadingType.Burmese,
            "gh" => RatRow.ShadingType.GoldenHimalayan,
            "gs" => RatRow.ShadingType.GoldenSiamese,
            "h" => RatRow.ShadingType.Himalayan,
            "m" => RatRow.ShadingType.Marten,
            "s" => RatRow.ShadingType.Siamese,
            "sa" => RatRow.ShadingType.SilverAgouti,
            "sb" => RatRow.ShadingType.SableBurmese,
            "wb" => RatRow.ShadingType.WheatenBurmese,
            _ => (RatRow.ShadingType?)null
        };
        var colour = (reader.GetValue(10) as string)?.ToLower() switch
        {
            // TODO
            _ => (RatRow.ColourType?)null
        };
        var tailKink = (reader.GetValue(11) as string)?.ToLower() switch
        {
            "y" => true,
            "n" => false,
            _ => (bool?)null
        };
        var owner = reader.GetValue(12) as string;
        var ownerContactDetails = reader.GetValue(13) as string;
        var dateOfBirth = reader.GetValue(14) as DateTime?;
        var dateOfDeath = reader.GetValue(15) as DateTime?;
        var litterMum = reader.GetValue(16) as string;
        var litterDad = reader.GetValue(17) as string;
        var knownGenetics = reader.GetValue(18) as string;
        var numberOfMatings = reader.GetValue(19) as int?;
        var litterDetails = reader.GetValue(20) as string;
        var deadOrAlive = (reader.GetValue(21) as string)?.ToLower() switch
        {
            "d" => RatRow.LifeIndicator.Dead,
            "a" => RatRow.LifeIndicator.Alive,
            _ => (RatRow.LifeIndicator?)null
        };
        var calculatedAge = reader.GetValue(22) as double?;
        // TODO more data
        
        return new RatRow(
            litterIdentifier,
            ratName,
            petName,
            variety,
            sex,
            ear,
            eye,
            coat,
            marking,
            shading,
            colour,
            tailKink,
            owner,
            ownerContactDetails,
            dateOfBirth is null ? null : DateOnly.FromDateTime(dateOfBirth.Value),
            dateOfDeath is null ? null : DateOnly.FromDateTime(dateOfDeath.Value),
            litterMum,
            litterDad,
            knownGenetics,
            numberOfMatings,
            litterDetails,
            deadOrAlive,
            calculatedAge);
    }

    public static LitterRow ReadLitterDetailsRow(this IExcelDataReader reader)
    {
        var litterIdentifier = reader.GetValue(0) as string;
        var matingDate = reader.GetValue(1) as DateTime?;
        var dayOfBirthCalculated = reader.GetValue(2) as int?;
        var timeOfBirth = reader.GetValue(3) as string;
        var dateOfBirth = reader.GetValue(4) as DateTime?;
        var currentAgeMonths = reader.GetValue(5) as double?;
        var numberOfDoes = reader.GetValue(6) as int?;
        var numberOfBucks = reader.GetValue(7) as int?;
        var totalInLitterCalculated = reader.GetValue(8) as int?;
        var totalInLitterIncludingStillbornCalculated = reader.GetValue(9) as int?;
        var numberOfStillborn = reader.GetValue(10) as int?;
        var numberOfInfantDeaths = reader.GetValue(11) as int?;
        var currentlyAlive = reader.GetValue(12) as int?;
        var passedOn = reader.GetValue(13) as int?;
        var percentageLivingCalculated = reader.GetValue(14) as double?;
        var meanAverageAgeCalculated = reader.GetValue(15) as double?;
        var minimumAge = reader.GetValue(16) as double?;
        var maximumAge = reader.GetValue(17) as double?;
        // TODO More
        
        return new LitterRow(
            litterIdentifier,
            matingDate is null ? null : DateOnly.FromDateTime(matingDate.Value),
            dayOfBirthCalculated,
            timeOfBirth,
            dateOfBirth is null ? null : DateOnly.FromDateTime(dateOfBirth.Value),
            currentAgeMonths,
            numberOfDoes,
            numberOfBucks,
            totalInLitterCalculated,
            totalInLitterIncludingStillbornCalculated,
            numberOfStillborn,
            numberOfInfantDeaths,
            currentlyAlive,
            passedOn,
            percentageLivingCalculated,
            meanAverageAgeCalculated,
            minimumAge,
            maximumAge);
    }

    public static FamilyRow ReadFamilyTreeDataRow(this IExcelDataReader reader)
    {
        var litterId = reader.GetValue(0) as string;
        var showFullName = reader.GetValue(1) as string;
        var petName = reader.GetValue(2) as string;
        var breeder = reader.GetValue(3) as string;
        var variety = reader.GetValue(4) as string;
        // TODO Conditions, cause of death, etc
        var s1 = reader.GetValue(17) as string;
        var d1 = reader.GetValue(18) as string;
        var ss2 = reader.GetValue(19) as string;
        var sd2 = reader.GetValue(20) as string;
        var ds2 = reader.GetValue(21) as string;
        var dd2 = reader.GetValue(22) as string;
        var sss3 = reader.GetValue(23) as string;
        var ssd3 = reader.GetValue(24) as string;
        var sds3 = reader.GetValue(25) as string;
        var sdd3 = reader.GetValue(26) as string;
        var dss3 = reader.GetValue(27) as string;
        var dsd3 = reader.GetValue(28) as string;
        var dds3 = reader.GetValue(29) as string;
        var ddd3 = reader.GetValue(30) as string;
        var ssss4 = reader.GetValue(31) as string;
        var sssd4 = reader.GetValue(32) as string;
        var ssds4 = reader.GetValue(33) as string;
        var ssdd4 = reader.GetValue(34) as string;
        var sdss4 = reader.GetValue(35) as string;
        var sdsd4 = reader.GetValue(36) as string;
        var sdds4 = reader.GetValue(37) as string;
        var sddd4 = reader.GetValue(38) as string;
        var dsss4 = reader.GetValue(39) as string;
        var dssd4 = reader.GetValue(40) as string;
        var dsds4 = reader.GetValue(41) as string;
        var dsdd4 = reader.GetValue(42) as string;
        var ddss4 = reader.GetValue(43) as string;
        var ddsd4 = reader.GetValue(44) as string;
        var ddds4 = reader.GetValue(45) as string;
        var dddd4 = reader.GetValue(46) as string;
        var sssss5 = reader.GetValue(47) as string;
        var ssssd5 = reader.GetValue(48) as string;
        var sssds5 = reader.GetValue(49) as string;
        var sssdd5 = reader.GetValue(50) as string;
        var ssdss5 = reader.GetValue(51) as string;
        var ssdsd5 = reader.GetValue(52) as string;
        var ssdds5 = reader.GetValue(53) as string;
        var ssddd5 = reader.GetValue(54) as string;
        var sdsss5 = reader.GetValue(55) as string;
        var sdssd5 = reader.GetValue(56) as string;
        var sdsds5 = reader.GetValue(57) as string;
        var sdsdd5 = reader.GetValue(58) as string;
        var sddss5 = reader.GetValue(59) as string;
        var sddsd5 = reader.GetValue(60) as string;
        var sddds5 = reader.GetValue(61) as string;
        var sdddd5 = reader.GetValue(62) as string;
        var dssss5 = reader.GetValue(63) as string;
        var dsssd5 = reader.GetValue(64) as string;
        var dssds5 = reader.GetValue(65) as string;
        var dssdd5 = reader.GetValue(66) as string;
        var dsdss5 = reader.GetValue(67) as string;
        var dsdsd5 = reader.GetValue(68) as string;
        var dsdds5 = reader.GetValue(69) as string;
        var dsddd5 = reader.GetValue(70) as string;
        var ddsss5 = reader.GetValue(71) as string;
        var ddssd5 = reader.GetValue(72) as string;
        var ddsds5 = reader.GetValue(73) as string;
        var ddsdd5 = reader.GetValue(74) as string;
        var dddss5 = reader.GetValue(75) as string;
        var dddsd5 = reader.GetValue(76) as string;
        var dddds5 = reader.GetValue(77) as string;
        var ddddd5 = reader.GetValue(78) as string;
        
        // TODO Rest of family tree
        return new FamilyRow(
            litterId,
            showFullName,
            petName,
            breeder,
            variety,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            s1, d1,
            ss2, sd2, ds2, dd2,
            sss3, ssd3, sds3, sdd3, dss3, dsd3, dds3, ddd3,
            ssss4, sssd4, ssds4, ssdd4, sdss4, sdsd4, sdds4, sddd4,
            dsss4, dssd4, dsds4, dsdd4, ddss4, ddsd4, ddds4, dddd4,
            sssss5, ssssd5, sssds5, sssdd5, ssdss5, ssdsd5, ssdds5, ssddd5, sdsss5, sdssd5, sdsds5, sdsdd5, sddss5, sddsd5, sddds5, sdddd5,
            dssss5, dsssd5, dssds5, dssdd5, dsdss5, dsdsd5, dsdds5, dsddd5, ddsss5, ddssd5, ddsds5, ddsdd5, dddss5, dddsd5, dddds5, ddddd5);
    }
}