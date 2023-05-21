using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;

namespace Basemix.Lib.Litters;

public class BulkKittenCreator
{
    private readonly ILittersRepository littersRepository;
    private readonly IRatsRepository ratsRepository;

    public BulkKittenCreator(ILittersRepository littersRepository, IRatsRepository ratsRepository)
    {
        this.littersRepository = littersRepository;
        this.ratsRepository = ratsRepository;
    }
    
    public string Amount { get; set; } = 1.ToString();
    public string? Sex { get; set; }
    public string? Variety { get; set; }
    public bool SetAsUnowned { get; set; }
    public string? Error { get; private set; }

    public async Task Create(Litter litter)
    {
        if (this.Validate())
        {
            var amount = int.Parse(this.Amount);
            var sex = string.IsNullOrEmpty(this.Sex) ? (Sex?)null : Enum.Parse<Sex>(this.Sex);
            await litter.CreateMultipleOffspring(this.littersRepository, this.ratsRepository, amount, sex, this.Variety,
                !this.SetAsUnowned);
        }
    }

    private bool Validate()
    {
        return true;
    }
}