namespace Basemix.Pedigrees;

public class Node
{
    public string? Name { get; set; }
    public string? Variety { get; set; }
    public Node? Dam { get; set; }
    public Node? Sire { get; set; }
}
