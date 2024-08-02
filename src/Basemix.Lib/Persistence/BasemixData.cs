namespace Basemix.Lib.Persistence;

public class BasemixData
{
    public static string StandardDbName = "db.sqlite";
    
    public static string GetBaseDirectory()
    {
        var docsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        return Path.Combine(docsDirectory, "basemix");
    }

    public static string GetDbFilePath(string? dbName = null)
    {
        return Path.Combine(GetBaseDirectory(), dbName ?? StandardDbName);
    }

    public static string GetLegacyDbFilePath(string? dbName = null)
    {
        var legacyDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(legacyDirectory, $"basemix/{dbName ?? StandardDbName}");
    }
    
    /// <summary>
    /// Moves the database from <paramref name="fromFile"/> to <paramref name="toFile"/> if it exists and the destination does not.
    /// Will not overwrite databases that exist or error if the source does not exist.
    /// </summary>
    public static bool TryMoveDb(string fromFile, string toFile)
    {
        if (!File.Exists(fromFile) || File.Exists(toFile))
        {
            return false;
        }
        
        File.Move(fromFile, toFile);
        return true;
    }
}