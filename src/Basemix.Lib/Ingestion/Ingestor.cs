using System.Runtime.CompilerServices;
using Basemix.Lib.Litters;
using Basemix.Lib.Litters.Persistence;
using Basemix.Lib.Owners;
using Basemix.Lib.Owners.Persistence;
using Basemix.Lib.Rats;
using Basemix.Lib.Rats.Persistence;

namespace Basemix.Lib.Ingestion;

public class Ingestor
{
    private readonly ILittersRepository littersRepository;
    private readonly IRatsRepository ratsRepository;
    private readonly IOwnersRepository ownersRepository;

    public Ingestor(ILittersRepository littersRepository, IRatsRepository ratsRepository, IOwnersRepository ownersRepository)
    {
        this.littersRepository = littersRepository;
        this.ratsRepository = ratsRepository;
        this.ownersRepository = ownersRepository;
    }

    public async Task Ingest(RatIngestionData data)
    {
        var createdOwners = new Dictionary<OwnerToCreate, Owner>();
        foreach (var ownerData in data.Owners)
        {
            var owner = await Owner.Create(this.ownersRepository);
            
            ownerData.Apply(owner);
            await owner.Save(this.ownersRepository);
            createdOwners.Add(ownerData, owner);
        }
        
        var createdRats = new Dictionary<RatToCreate, Rat>();
        foreach (var ratData in data.Rats)
        {
            var rat = await Rat.Create(this.ratsRepository);

            ratData.Apply(rat);
            await rat.Save(this.ratsRepository);

            _ = ratData.Owner != null
                ? await rat.SetOwner(this.ratsRepository, createdOwners[ratData.Owner])
                : OwnerAddResult.Success;
            
            createdRats.Add(ratData, rat);
        }

        foreach (var litterData in data.Litters)
        {
            var litter = await Litter.Create(this.littersRepository);
            
            litterData.Apply(litter);
            await litter.Save(this.littersRepository);
            
            _ = litterData.Sire != null
                ? await litter.SetSire(this.littersRepository, createdRats[litterData.Sire])
                : LitterAddResult.Success;
            _ = litterData.Dam != null
                ? await litter.SetDam(this.littersRepository, createdRats[litterData.Dam])
                : LitterAddResult.Success;
            
            foreach (var offspring in litterData.Offspring)
            {
                await litter.AddOffspring(this.littersRepository, createdRats[offspring]);
            }
        }
    }
}