using Basemix.Lib.Litters;
using Basemix.Lib.Rats;

namespace Basemix.Lib.Ingestion;

public record RatIngestionData(IReadOnlyList<RatToCreate> Rats, IReadOnlyList<LitterToCreate> Litters);

public class RatToCreate
{
    public RatIdentity? AssignedId { get; set; }
    public string? Name { get; set; }
    public Sex? Sex { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Variety { get; set; }
    public string? Notes { get; set; }
    public DateOnly? DateOfDeath { get; set; }
    public string? DeathReason { get; set; }
    public bool Owned { get; set; }

    public void Apply(Rat rat)
    {
        this.AssignedId = rat.Id;
        rat.Name = this.Name;
        rat.Sex = this.Sex;
        rat.DateOfBirth = this.DateOfBirth;
        rat.DateOfDeath = this.DateOfDeath;
        // rat.DeathReason = this.DeathReason; TODO
        rat.Variety = this.Variety;
        rat.Notes = this.Notes;
        rat.Owned = this.Owned;
    }
}

public class LitterToCreate
{
    public LitterIdentity? AssignedId { get; set; }
    public RatToCreate? Sire { get; set; }
    public RatToCreate? Dam { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    
   public List<RatToCreate> Offspring { get; set; } = new();

   public void Apply(Litter litter)
   {
       this.AssignedId = litter.Id;
       
       litter.DateOfBirth = this.DateOfBirth;
       litter.Notes = this.Notes;
   }
}