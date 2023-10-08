namespace Basemix.Lib.Litters;

public class LitterOverview
{
    public LitterOverview(LitterIdentity id) => this.Id = id;
    
    public LitterIdentity Id { get; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Name { get; init; }
    public string? Dam { get; init; }
    public string? Sire { get; init; }
    public int OffspringCount { get; init; }

    public string LitterName
    {
        get
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return this.Name;
            }
            if (!string.IsNullOrEmpty(this.Dam) && !string.IsNullOrEmpty(this.Sire))
            {
                return $"{this.Dam} & {this.Sire}'s litter";
            }
            if (!string.IsNullOrEmpty(this.Dam))
            {
                return $"{this.Dam}'s litter";
            }
            if (!string.IsNullOrEmpty(this.Sire))
            {
                return $"{this.Sire}'s litter";
            }
            
            return "Anonymous litter";
        }
    }
}