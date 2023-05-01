namespace Basemix.Lib.Ingestion.RatRecordsSpreadsheet;

public class Validator
{
    public ValidationResult Validate(RatRecords records)
    {
        var problems = new List<ValidationProblem>();
        
        var ratsByLitter = records.Rats
            .Where(x => !string.IsNullOrEmpty(x.LitterIdentifier))
            .GroupBy(x => x.LitterIdentifier).ToList();
        
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
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter ID '{rat.LitterIdentifier}' but there is no litter with that ID."));
            }

            if (rat.Sex == null)
            {
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has no sex."));
            }

            if (!string.IsNullOrEmpty(rat.LitterMum) && records.Rats.FirstOrDefault(x => x.RatName == rat.LitterMum) is null)
            {
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter mum '{rat.LitterMum}' but there is no rat with that name (Litter {rat.LitterIdentifier})."));
            }
            
            if (!string.IsNullOrEmpty(rat.LitterDad) && records.Rats.FirstOrDefault(x => x.RatName == rat.LitterDad) is null)
            {
                problems.Add(new ValidationProblem($"Rat '{preferredName}' has litter dad '{rat.LitterDad}' but there is no rat with that name (Litter {rat.LitterIdentifier})."));
            }
        }

        return new ValidationResult(problems, records.Litters.Count, records.Rats.Count);
    }
}

public record ValidationResult(
    IReadOnlyList<ValidationProblem> Problems,
    int NumberOfLittersBeingAdded,
    int NumberOfRatsBeingAdded);

public class ValidationProblem
{
    public ValidationProblem(string message)
    {
        this.Message = message;
    }

    public string Message { get; }
}