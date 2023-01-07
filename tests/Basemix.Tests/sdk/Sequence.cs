namespace Basemix.Tests.sdk;

public class Sequence
{
    private long next = 1;
    
    public long Get()
    {
        var nextInSequence = this.next;
        this.next += 1;
        return nextInSequence;
    }
}

public delegate long NextId(); 