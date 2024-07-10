using Dapper;

namespace Basemix.Lib.Settings.Persistence;


public interface IOptionsRepository
{
    Task<List<DeathReason>> GetDeathReasons();
}

public class SqliteOptionsRepository : IOptionsRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteOptionsRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }
    
    public async Task<List<DeathReason>> GetDeathReasons()
    {
        using var db = this.getDatabase();

        var reasons = await db.QueryAsync<DeathReason>(
            """
            SELECT id, reason
            FROM death_reason
            ORDER BY reason
            """);

        return reasons.ToList();
    }
}

public record DeathReason(long Id, string Reason);