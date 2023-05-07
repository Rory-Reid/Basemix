using Basemix.Lib.Litters;
using Basemix.Lib.Rats;

namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public class DataMapper
{
    public RatIngestionData Map(RatRecords records, RatIngestionOptions options)
    {
        var owners = new List<OwnerToCreate>();
        foreach (var rat in records.Rats)
        {
            if (string.IsNullOrEmpty(rat.Owner) ||
                rat.Owner.Equals(options.UserOwnerName, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            
            var owner = owners.FirstOrDefault(x => x.Name == rat.Owner);
            if (owner is null)
            {
                owner = new OwnerToCreate
                {
                    Name = rat.Owner,
                    Notes = string.IsNullOrEmpty(rat.OwnerContactDetails)
                        ? null
                        : $"**Contact Details**{Environment.NewLine}{rat.OwnerContactDetails}"
                };
                owners.Add(owner);
            }
        }
        
        var rats = new Dictionary<RatRow, RatToCreate>();
        foreach (var rat in records.Rats)
        {
            var owned = string.IsNullOrEmpty(rat.Owner) || 
                rat.Owner.Equals(options.UserOwnerName, StringComparison.InvariantCultureIgnoreCase);
            var notes = this.MapNotes(rat, options);
            
            OwnerToCreate? owner = null;
            if (!owned)
            {
                owner = owners.FirstOrDefault(x => x.Name == rat.Owner);
            }

            rats[rat] = new RatToCreate
            {
                Name = rat.RatName ?? rat.PetName,
                Sex = rat.Sex switch 
                {
                    RatRow.SexType.Doe => Sex.Doe,
                    RatRow.SexType.Buck => Sex.Buck,
                    _ => null
                },
                DateOfBirth = rat.DateOfBirth,
                Variety = rat.Variety,
                DateOfDeath = rat.DateOfDeath,
                //DeathReason = TODO
                Owned = owned,
                Owner = owner,
                
                Notes = notes.Any()
                    ? $"**Notes from spreadsheet below**{Environment.NewLine}{string.Join(Environment.NewLine, notes)}"
                    : null
            };
        }

        var litters = new List<LitterToCreate>();
        var litterRats = records.Rats.GroupBy(x => x.LitterIdentifier).ToList();
        var litterIdentifiers = litterRats
            .Select(x => x.Key)
            .Concat(records.Litters.Select(x => x.LitterIdentifier))
            .Distinct().ToList();
        foreach (var litterId in litterIdentifiers)
        {
            var litter = records.Litters.FirstOrDefault(x => x.LitterIdentifier == litterId);
            var ratsInLitter = litterRats.FirstOrDefault(x => x.Key == litterId)?.ToList() ?? new List<RatRow>();
            
            var sireName = ratsInLitter.FirstOrDefault()?.LitterDad;
            var damName = ratsInLitter.FirstOrDefault()?.LitterMum;
            var sire = string.IsNullOrEmpty(sireName) ? null : records.Rats.FirstOrDefault(x => x.RatName == sireName || x.PetName == damName);
            var dam = string.IsNullOrEmpty(damName) ? null : records.Rats.FirstOrDefault(x => x.RatName == damName || x.PetName == damName);
            
            var notes = this.MapNotes(litter, options);
            litters.Add(new LitterToCreate
            {
                DateOfBirth = litter?.DateOfBirth ?? ratsInLitter.FirstOrDefault()?.DateOfBirth,
                Sire = sire != null ? rats[sire] : null,
                Dam = dam != null ? rats[dam] : null,
                Offspring = ratsInLitter.Select(x => rats[x]).ToList(),
                Notes = notes.Any()
                    ? $"**Notes from spreadsheet below**{Environment.NewLine}{string.Join(Environment.NewLine, notes)}"
                    : null
            });
        }

        return new RatIngestionData(rats.Values.ToList(), litters, owners);
    }

    public List<string> MapNotes(RatRow rat, RatIngestionOptions options)
    {
        var notes = new List<string>();
        if (rat.PetName != rat.RatName)
        {
            notes.Add($"Pet name: {rat.PetName}");
        }
        
        if (rat.Owner != options.UserOwnerName)
        {
            var contactDetails = string.IsNullOrEmpty(rat.OwnerContactDetails)
                ? string.Empty
                : $" (Contact: {rat.OwnerContactDetails})";
            notes.Add($"Owner: {rat.Owner}{contactDetails}");
        }

        if (rat.Ear != null)
        {
            notes.Add($"Ear: {rat.Ear.Value.ToFriendlyString()}");
        }

        if (rat.Eye != null)
        {
            notes.Add($"Eye: {rat.Eye.Value.ToFriendlyString()}");
        }

        if (rat.Coat != null)
        {
            notes.Add($"Coat: {rat.Coat.Value.ToFriendlyString()}");
        }

        if (rat.Marking != null)
        {
            notes.Add($"Marking: {rat.Marking.Value.ToFriendlyString()}");
        }

        if (rat.Shading != null)
        {
            notes.Add($"Shading: {rat.Shading.Value.ToFriendlyString()}");
        }
        
        // TODO colour
        
        if (rat.TailKink is true)
        {
            notes.Add("This rat has a tail kink.");
        }

        if (rat.KnownGenetics != null)
        {
            notes.Add($"Known genetics: {rat.KnownGenetics}");
        }

        return notes;
    }

    public List<string> MapNotes(LitterRow? litter, RatIngestionOptions options)
    {
        var notes = new List<string>();
        if (litter == null)
        {
            return notes;
        }
        
        if (!string.IsNullOrEmpty(litter.LitterIdentifier))
        {
            notes.Add($"Litter Identifier: {litter.LitterIdentifier}");
        }
        
        if (!string.IsNullOrEmpty(litter.TimeOfBirth))
        {
            notes.Add($"Time of birth: {litter.TimeOfBirth}");
        }

        if (litter.NumberOfStillborn is > 0)
        {
            notes.Add($"Stillborn: {litter.NumberOfStillborn}");
        }
        return notes;
    }
}