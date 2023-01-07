using Dapper;

namespace Basemix.Rats.Persistence;

public class SqliteRatsRepository : IRatsRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteRatsRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<long> AddRat(Rat rat)
    {
        using var db = this.getDatabase();

        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO rat (name, sex, date_of_birth, notes)
               VALUES (@Name, @Sex, @DateOfBirth, @Notes)
               RETURNING id",
            new PersistedRat(rat));
    }

    public async Task<Rat?> GetRat(long id)
    {
        using var db = this.getDatabase();

        var rat = await db.QuerySingleOrDefaultAsync<PersistedRat>(
            @"SELECT
                id, name, sex, date_of_birth, notes
            FROM rat WHERE id=@Id",
            new { Id = id });

        return rat?.ToModelledRat();
    }

    public async Task<List<Rat>> GetAll()
    {
        using var db = this.getDatabase();

        var rat = await db.QueryAsync<PersistedRat>(
            @"SELECT
                id, name, sex, date_of_birth, notes
            FROM rat
            ORDER BY date_of_birth DESC");

        return rat.Select(x => x.ToModelledRat()).ToList();
    }
}