using Basemix.Lib.Rats;

namespace Basemix.Lib.Pedigrees;

public class Node
{
    public RatIdentity? Id { get; set; }
    public string? Name { get; set; }
    public string? Variety { get; set; }
    public string? LitterName { get; set; }
    public Node? Dam { get; set; }
    public Node? Sire { get; set; }
}
