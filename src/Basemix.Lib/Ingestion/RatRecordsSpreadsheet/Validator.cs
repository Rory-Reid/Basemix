namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public class Validator
{
    public ValidationResult Validate(RatRecords records)
    {
        var problems = new List<ValidationProblem>();
        var inferredRats = new HashSet<string>();
        var inferredLitters = new HashSet<string>();
        var ratOwners = new HashSet<string>();
        var extendedFamilyNames = new HashSet<string?>();
        
        var ratsByLitter = records.Rats
            .Where(x => !string.IsNullOrEmpty(x.LitterIdentifier))
            .GroupBy(x => x.LitterIdentifier).ToList();

        var mostRecentDeaths = records.Rats.Where(x => x.DateOfDeath != null).GroupBy(x => x.DateOfDeath).MaxBy(x => x.Key);
        if (mostRecentDeaths != null && mostRecentDeaths.Count() > 1)
        {
            if (mostRecentDeaths.Count() == 1)
            {
                var rat = mostRecentDeaths.Single();
                var preferredName = string.IsNullOrEmpty(rat.RatName) ? rat.RatName : rat.PetName;
                problems.Add(new ValidationProblem($"One rat ({preferredName}) has a date of death of {rat.DateOfDeath!.Value.ToLocalizedString()}. This is assumed to be a default value in the spreadsheet and will be imported as alive. If this is wrong, you should correct this rat after import."));
            }
            else
            {
                var totalRatsAffected = mostRecentDeaths.Count();
                var date = mostRecentDeaths.First().DateOfDeath!.Value;
                problems.Add(new ValidationProblem($"{totalRatsAffected} rats have a date of death of {date.ToLocalizedString()}. This is assumed to be a default value in the spreadsheet, and all affected rats will be imported with no date of death."));
            }
        }

        if (records.FamilyData.Any())
        {
            problems.Add(new ValidationProblem($"{records.FamilyData.Count} rats have been loaded from the Family Tree Data sheet. Any new rats here will be created and linked to rats or litters identified in the summary sheets. However, Basemix does not support building the extended family tree from this data yet so only the immediate rats will be added (and not, for example, SSD3)"));
        }

        foreach (var familyMember in records.FamilyData)
        {
            var preferredName = string.IsNullOrEmpty(familyMember.ShowOrFullName)
                ? familyMember.PetName
                : familyMember.ShowOrFullName;
            if (string.IsNullOrEmpty(preferredName))
            {
                problems.Add(new ValidationProblem($"Extended family tree member for litter '{familyMember.LitterId}' has no name."));
            }
            else
            {
                if (string.IsNullOrEmpty(familyMember.ShowOrFullName) && !string.IsNullOrEmpty(familyMember.PetName))
                {
                    problems.Add(new ValidationProblem($"Extended family tree member has no name but has a pet name '{familyMember.PetName}' - this will be used as the name instead."));
                }
                
                extendedFamilyNames.Add(preferredName);
            }
            
            GatherFamilyNames(extendedFamilyNames, familyMember);
        }

        if (ratsByLitter.Count != records.Litters.Count)
        {
            problems.Add(new ValidationProblem($"Mismatch between number of litters and litter identifiers on rats. There are {ratsByLitter.Count} unique litters against rats but {records.Litters.Count} unique litters on the litters page."));
        }

        foreach (var litter in records.Litters)
        {
            var group = ratsByLitter.SingleOrDefault(x => x.Key == litter.LitterIdentifier);
            if (group is null)
            {
                problems.Add(new ValidationProblem($"Litter '{litter.LitterIdentifier}' does not have any rats."));
                continue;
            }
            
            var groupCount = group.Count();
            if (litter is {TotalInLitterCalculated: > 0} or {TotalInLitterIncludingStillbornCalculated: > 0})
            {
                if (groupCount != litter.TotalInLitterCalculated ||
                    groupCount != litter.TotalInLitterIncludingStillbornCalculated)
                {
                    var expected = litter switch
                    {
                        {TotalInLitterCalculated: > 0, TotalInLitterIncludingStillbornCalculated: > 0} =>
                            $"{litter.TotalInLitterCalculated} ({litter.TotalInLitterIncludingStillbornCalculated} including stillborn)",
                        {TotalInLitterCalculated: > 0} =>
                            litter.TotalInLitterCalculated.ToString(),
                        {TotalInLitterIncludingStillbornCalculated: > 0} =>
                            $"{litter.TotalInLitterIncludingStillbornCalculated} (including stillborn)",
                        _ => null
                    };
                    
                    problems.Add(new ValidationProblem(
                        $"Found {group.Count()} rats with litter ID '{litter.LitterIdentifier}' has  but the " +
                        $"litters page says there should be {expected}."));
                }
            }
        }

        foreach (var litterGroup in ratsByLitter)
        {
            // If dates of birth differ
            // If mothers differ
            // If fathers differ
        }
        
        var namelessRats = records.Rats.Count(x => string.IsNullOrEmpty(x.RatName) && string.IsNullOrEmpty(x.PetName));
        if (namelessRats > 0)
        {
            problems.Add(new ValidationProblem($"{namelessRats} were detected with no name."));
        }

        foreach (var rat in records.Rats)
        {
            var preferredName = rat.RatName ?? rat.PetName ?? "unnamed rat";
            if (string.IsNullOrEmpty(rat.RatName) && !string.IsNullOrEmpty(rat.PetName))
            {
                problems.Add(new ValidationProblem($"Rat has no name but has a pet name '{rat.PetName}' - this will be used as the name instead."));
            }
            
            if (string.IsNullOrEmpty(rat.LitterIdentifier))
            {
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has no litter ID."));
            }
            else if (ratsByLitter.All(x => x.Key != rat.LitterIdentifier))
            {
                inferredLitters.Add(rat.LitterIdentifier);
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter ID '{rat.LitterIdentifier}' but there is no litter with that ID."));
            }

            if (rat.Sex == null)
            {
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has no sex."));
            }

            if (!string.IsNullOrEmpty(rat.LitterMum) &&
                records.Rats.FirstOrDefault(x => rat.LitterMum.Equals(x.RatName, StringComparison.InvariantCultureIgnoreCase)) == null &&
                extendedFamilyNames.FirstOrDefault(x => rat.LitterMum.Equals(x, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                inferredRats.Add(rat.LitterMum);
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter mum '{rat.LitterMum}' but there is no rat with that name (Litter {rat.LitterIdentifier})."));
            }
            
            if (!string.IsNullOrEmpty(rat.LitterDad) &&
                records.Rats.FirstOrDefault(x => rat.LitterDad.Equals(x.RatName, StringComparison.InvariantCultureIgnoreCase)) == null &&
                extendedFamilyNames.FirstOrDefault(x => rat.LitterDad.Equals(x, StringComparison.InvariantCultureIgnoreCase)) == null)
            {
                inferredRats.Add(rat.LitterDad);
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter dad '{rat.LitterDad}' but there is no rat with that name (Litter {rat.LitterIdentifier})."));
            }

            if (rat.Owner != null)
            {
                ratOwners.Add(rat.Owner);
            }
        }

        return new ValidationResult(
            problems,
            records.Litters.Count + inferredLitters.Count,
            records.Rats.Count + inferredRats.Count + records.FamilyData.Count,
            ratOwners.Count);
    }

    private static void GatherFamilyNames(HashSet<string?> allNames, FamilyRow row)
    {
        allNames.Add(row.S1);
        allNames.Add(row.D1);
        allNames.Add(row.Ss2);
        allNames.Add(row.Sd2);
        allNames.Add(row.Ds2);
        allNames.Add(row.Dd2);
        allNames.Add(row.Sss3);
        allNames.Add(row.Ssd3);
        allNames.Add(row.Sds3);
        allNames.Add(row.Sdd3);
        allNames.Add(row.Dss3);
        allNames.Add(row.Dsd3);
        allNames.Add(row.Dds3);
        allNames.Add(row.Ddd3);
        allNames.Add(row.Ssss4);
        allNames.Add(row.Sssd4);
        allNames.Add(row.Ssds4);
        allNames.Add(row.Ssdd4);
        allNames.Add(row.Sdss4);
        allNames.Add(row.Sdsd4);
        allNames.Add(row.Sdds4);
        allNames.Add(row.Sddd4);
        allNames.Add(row.Dsss4);
        allNames.Add(row.Dssd4);
        allNames.Add(row.Dsds4);
        allNames.Add(row.Dsdd4);
        allNames.Add(row.Ddss4);
        allNames.Add(row.Ddsd4);
        allNames.Add(row.Ddds4);
        allNames.Add(row.Dddd4);
        allNames.Add(row.Sssss5);
        allNames.Add(row.Ssssd5);
        allNames.Add(row.Sssds5);
        allNames.Add(row.Sssdd5);
        allNames.Add(row.Ssdss5);
        allNames.Add(row.Ssdsd5);
        allNames.Add(row.Ssdds5);
        allNames.Add(row.Ssddd5);
        allNames.Add(row.Sdsss5);
        allNames.Add(row.Sdssd5);
        allNames.Add(row.Sdsds5);
        allNames.Add(row.Sdsdd5);
        allNames.Add(row.Sddss5);
        allNames.Add(row.Sddsd5);
        allNames.Add(row.Sddds5);
        allNames.Add(row.Sdddd5);
        allNames.Add(row.Dssss5);
        allNames.Add(row.Dsssd5);
        allNames.Add(row.Dssds5);
        allNames.Add(row.Dssdd5);
        allNames.Add(row.Dsdss5);
        allNames.Add(row.Dsdsd5);
        allNames.Add(row.Dsdds5);
        allNames.Add(row.Dsddd5);
        allNames.Add(row.Ddsss5);
        allNames.Add(row.Ddssd5);
        allNames.Add(row.Ddsds5);
        allNames.Add(row.Ddsdd5);
        allNames.Add(row.Dddss5);
        allNames.Add(row.Dddsd5);
        allNames.Add(row.Dddds5);
        allNames.Add(row.Ddddd5);
    }
}

public record ValidationResult(
    IReadOnlyList<ValidationProblem> Problems,
    int NumberOfLittersToAdd,
    int NumberOfRatsToAdd,
    int NumberOfOwnersToAdd);

public class ValidationProblem
{
    public ValidationProblem(string message)
    {
        this.Message = message;
    }

    public string Message { get; }
}