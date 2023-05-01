using System.Collections.Immutable;

namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public record RatRow(
    string? LitterIdentifier,
    string? RatName,
    string? PetName,
    string? Variety,
    RatRow.SexType? Sex,
    RatRow.EarType? Ear,
    RatRow.EyeType? Eye,
    RatRow.CoatType? Coat,
    RatRow.MarkingType? Marking,
    RatRow.ShadingType? Shading,
    RatRow.ColourType? Colour,
    bool? TailKink,
    string? Owner,
    string? OwnerContactDetails,
    DateOnly? DateOfBirth,
    DateOnly? DateOfDeath,
    string? LitterMum,
    string? LitterDad,
    string? KnownGenetics,
    int? NumberOfMatings,
    string? LitterDetails,
    RatRow.LifeIndicator? DeadOrAlive,
    double? AgeInMonths)
{
    public bool IsNullRat =>
        this.LitterIdentifier is null &&
        this.RatName is null &&
        this.PetName is null &&
        this.Variety is null;
    
    public enum SexType
    {
        Error = default,
        Buck,
        Doe
    }

    public enum EarType
    {
        Error = default,
        TopEared,
        Dumbo
    }
    
    public enum EyeType
    {
        Error = default,
        Black,
        RedOrRuby,
        Pink
    }

    public enum CoatType
    {
        Error = default,
        StandardSmooth,
        DoubleRex,
        Hairless,
        Rex,
        Satin,
        Velvet
    }
    
    public enum MarkingType
    {
        Error = default,
        Self,
        Berkshire,
        Badger,
        Bareback,
        Capped,
        CappedEssex,
        Chinchilla,
        Downunder,
        Essex,
        Hooded,
        HoodedDownunder,
        Irish,
        Merle,
        Roan,
        SpottedDownunder,
        Squirrel,
        StripedRoan,
        Turpin,
        Variegated,
    }

    public enum ShadingType
    {
        Error = default,
        ArgenteCreme,
        Burmese,
        GoldenHimalayan,
        GoldenSiamese,
        Himalayan,
        Marten,
        Siamese,
        SilverAgouti,
        SableBurmese,
        WheatenBurmese
    }

    public enum ColourType
    {
        Error = default,
        // TODO
    }

    public enum LifeIndicator
    {
        Error = default,
        Dead,
        Alive
    }
};

public static class RatRowExtensions
{
    public static string ToFriendlyString(this RatRow.EarType ear) => ear switch
    {
        RatRow.EarType.TopEared => "Top Eared",
        RatRow.EarType.Dumbo => "Dumbo",
        _ => throw new ArgumentOutOfRangeException(nameof(ear), ear, null)
    };
    
    public static string ToFriendlyString(this RatRow.EyeType eye) => eye switch
    {
        RatRow.EyeType.Black => "Black",
        RatRow.EyeType.RedOrRuby => "Red/Ruby",
        RatRow.EyeType.Pink => "Pink",
        _ => throw new ArgumentOutOfRangeException(nameof(eye), eye, null)
    };
    
    public static string ToFriendlyString(this RatRow.CoatType coat) => coat switch
    {
        RatRow.CoatType.StandardSmooth => "Standard/Smooth",
        RatRow.CoatType.DoubleRex => "Double Rex",
        RatRow.CoatType.Hairless => "Hairless",
        RatRow.CoatType.Rex => "Rex",
        RatRow.CoatType.Satin => "Satin",
        RatRow.CoatType.Velvet => "Velvet",
        _ => throw new ArgumentOutOfRangeException(nameof(coat), coat, null)
    };
    
    public static string ToFriendlyString(this RatRow.MarkingType marking) => marking switch
    {
        RatRow.MarkingType.Self => "Self",
        RatRow.MarkingType.Berkshire => "Berkshire",
        RatRow.MarkingType.Badger => "Badger",
        RatRow.MarkingType.Bareback => "Bareback",
        RatRow.MarkingType.Capped => "Capped",
        RatRow.MarkingType.CappedEssex => "Capped Essex",
        RatRow.MarkingType.Chinchilla => "Chinchilla",
        RatRow.MarkingType.Downunder => "Downunder",
        RatRow.MarkingType.Essex => "Essex",
        RatRow.MarkingType.Hooded => "Hooded",
        RatRow.MarkingType.HoodedDownunder => "Hooded Downunder",
        RatRow.MarkingType.Irish => "Irish",
        RatRow.MarkingType.Merle => "Merle",
        RatRow.MarkingType.Roan => "Roan",
        RatRow.MarkingType.SpottedDownunder => "Spotted Downunder",
        RatRow.MarkingType.Squirrel => "Squirrel",
        RatRow.MarkingType.StripedRoan => "Striped Roan",
        RatRow.MarkingType.Turpin => "Turpin",
        RatRow.MarkingType.Variegated => "Variegated",
        _ => throw new ArgumentOutOfRangeException(nameof(marking), marking, null)
    };
    
    public static string ToFriendlyString(this RatRow.ShadingType shading) => shading switch
    {
        RatRow.ShadingType.ArgenteCreme => "Argente Creme",
        RatRow.ShadingType.Burmese => "Burmese",
        RatRow.ShadingType.GoldenHimalayan => "Golden Himalayan",
        RatRow.ShadingType.GoldenSiamese => "Golden Siamese",
        RatRow.ShadingType.Himalayan => "Himalayan",
        RatRow.ShadingType.Marten => "Marten",
        RatRow.ShadingType.Siamese => "Siamese",
        RatRow.ShadingType.SilverAgouti => "Silver Agouti",
        RatRow.ShadingType.SableBurmese => "Sable Burmese",
        RatRow.ShadingType.WheatenBurmese => "Wheaten Burmese",
        _ => throw new ArgumentOutOfRangeException(nameof(shading), shading, null)
    };
}