namespace Basemix.Lib.Identity;

public class InvalidIdentityException : Exception
{
    public InvalidIdentityException(string message) : base(message)
    {
    }
}