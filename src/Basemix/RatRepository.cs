using Dapper;

namespace Basemix;

public class RatRepository
{
    private readonly GetDatabase getDatabase;

    public RatRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<long> AddRat(Rat rat)
    {
        using var db = this.getDatabase();

        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO rat (name, sex, date_of_birth)
               VALUES (@Name, @Sex, @DateOfBirth)
               RETURNING id",
            new { Name = rat.Name, Sex = rat.Sex, DateOfBirth = rat.DateOfBirth });
    }

    public async Task<Rat> GetRat(long id)
    {
        using var db = this.getDatabase();

        return await db.QuerySingleOrDefaultAsync<Rat>(
            @"SELECT id, name, sex, date_of_birth FROM rat WHERE id=@Id",
            new { Id = id });
    }
}

public class Rat
{
    public string Name { get; init; } = null!;
    public string Sex { get; init; } = null!;
    public long DateOfBirth { get; init; }
    public string Notes { get; set; } = null!;
}