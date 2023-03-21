namespace Basemix.Lib.Identity;

public class NoIdentityException : Exception
{
    public NoIdentityException(string message) : base(message)
    {
    }
}