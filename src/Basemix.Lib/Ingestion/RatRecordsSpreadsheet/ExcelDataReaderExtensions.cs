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
}