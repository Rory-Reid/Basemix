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
            else if (owner.Notes != null && !string.IsNullOrEmpty(rat.OwnerContactDetails))
            {
                owner.Notes = $"**Contact Details**{Environment.NewLine}{rat.OwnerContactDetails}";
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

        var unknownSires = new Dictionary<string, RatToCreate>();
        foreach (var sireGroup in records.Rats.GroupBy(x => x.LitterDad).ToList())
        {
            var sireName = sireGroup.Key;
            if (string.IsNullOrEmpty(sireName) || rats.Keys.Any(r => 
                (r.RatName != null && r.RatName.Equals(sireName, StringComparison.InvariantCultureIgnoreCase)) ||
                (r.PetName != null && r.PetName.Equals(sireName, StringComparison.InvariantCultureIgnoreCase))))
            {
                continue;
            }

            unknownSires[sireName] = new RatToCreate
            {
                Name = sireName,
                Sex = Sex.Buck,
                Notes =
                    $"**Notes from spreadsheet below**{Environment.NewLine}No explicit record was found for this " +
                    $"rat. It is the imported father of {sireGroup.Count()} rats:{Environment.NewLine}- " +
                    $"{string.Join($"{Environment.NewLine}- ", sireGroup.Select(x => x.RatName ?? x.PetName))}"
            };
        }
        
        var unknownDams = new Dictionary<string, RatToCreate>();
        foreach (var damGroup in records.Rats.GroupBy(x => x.LitterMum).ToList())
        {
            var damName = damGroup.Key;
            if (string.IsNullOrEmpty(damName) || rats.Keys.Any(r => 
                    (r.RatName != null && r.RatName.Equals(damName, StringComparison.InvariantCultureIgnoreCase)) ||
                    (r.PetName != null && r.PetName.Equals(damName, StringComparison.InvariantCultureIgnoreCase))))
            {
                continue;
            }

            unknownDams[damName] = new RatToCreate
            {
                Name = damName,
                Sex = Sex.Doe,
                Notes =
                    $"**Notes from spreadsheet below**{Environment.NewLine}No explicit record was found for this " +
                    $"rat. It is the imported mother of {damGroup.Count()} rats:{Environment.NewLine}- " +
                    $"{string.Join($"{Environment.NewLine}- ", damGroup.Select(x => x.RatName ?? x.PetName))}"
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
            var sireRow = string.IsNullOrEmpty(sireName) ? null : records.Rats.FirstOrDefault(x => x.RatName == sireName || x.PetName == sireName);
            var damRow = string.IsNullOrEmpty(damName) ? null : records.Rats.FirstOrDefault(x => x.RatName == damName || x.PetName == damName);
            var sire = sireRow != null ? rats[sireRow] : !string.IsNullOrEmpty(sireName) && unknownSires.TryGetValue(sireName, out var unknownSire) ? unknownSire : null;
            var dam = damRow != null ? rats[damRow] : !string.IsNullOrEmpty(damName) && unknownDams.TryGetValue(damName, out var unknownDam) ? unknownDam : null;

            var notes = this.MapNotes(litter, options);
            litters.Add(new LitterToCreate
            {
                DateOfBirth = litter?.DateOfBirth ?? ratsInLitter.FirstOrDefault()?.DateOfBirth,
                Sire = sire,
                Dam = dam,
                Offspring = ratsInLitter.Select(x => rats[x]).ToList(),
                Notes = notes.Any()
                    ? $"**Notes from spreadsheet below**{Environment.NewLine}{string.Join(Environment.NewLine, notes)}"
                    : null
            });
        }

        return new RatIngestionData(
            rats.Values.Concat(unknownSires.Values).Concat(unknownDams.Values).ToList(),
            litters,
            owners);
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