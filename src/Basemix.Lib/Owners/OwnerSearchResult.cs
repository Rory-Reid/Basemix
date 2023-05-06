namespace Basemix.Lib.Owners;

public record OwnerSearchResult(OwnerIdentity Id, string? Name) : IOwnerDetails;