namespace Basemix.Rats;

public record RatSearchResult(RatIdentity Id, string? Name, Sex? Sex, DateOnly? DateOfBirth);