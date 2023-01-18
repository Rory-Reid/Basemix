using Basemix.Persistence;
using Dapper;

namespace Basemix.Rats.Persistence;

public class SqliteRatsRepository : IRatsRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteRatsRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<long> CreateRat()
    {
        using var db = this.getDatabase();
        
        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO rat DEFAULT VALUES RETURNING id");
    }
    
    public async Task<long> AddRat(Rat rat) // TODO remove
    {
        using var db = this.getDatabase();

        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO rat (name, sex, date_of_birth, notes)
               VALUES (@Name, @Sex, @DateOfBirth, @Notes)
               RETURNING id",
            new PersistedRat(rat));
    }

    public Task UpdateRat(Rat rat)
    {
        using var db = this.getDatabase();

        return db.ExecuteAsync(
            @"UPDATE rat
            SET
                name=@Name,
                sex=@Sex,
                date_of_birth=@DateOfBirth,
                notes=@Notes
            WHERE id=@Id",
            new PersistedRat(rat));
    }
    
    public async Task<Rat?> GetRat(long id)
    {
        using var db = this.getDatabase();
        
        using var reader = await db.QueryMultipleAsync( // TODO simplify the weird dam/sire stuff
            @"SELECT
                id, name, sex, date_of_birth, notes
            FROM rat WHERE id=@Id;

            SELECT
                litter.id,
                litter.date_of_birth,
                dam.name as dam_name,
                sire.name as sire_name,
                (SELECT COUNT(offspring_id) FROM litter_kin WHERE litter_id=litter.id) AS offspring_count
            FROM litter
            LEFT JOIN rat dam on dam.id=dam_id
            LEFT JOIN rat sire on sire.id=sire_id
            WHERE litter.dam_id=@Id OR litter.sire_id=@Id",
            new { Id = id });

        var rat = await reader.ReadSingleOrDefaultAsync<PersistedRat>();
        if (rat == null)
        {
            return null;
        }
        
        var litters = await reader.ReadAsync<PersistedRatLitter>();
        return rat?.ToModelledRat(litters.ToList());
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

    public async Task DeleteRat(long id)
    {
        using var db = this.getDatabase();

        await db.ExecuteAsync(
            @"UPDATE litter SET dam_id=NULL WHERE dam_id=@Id;
            UPDATE litter SET sire_id=NULL WHERE sire_id=@Id;
            DELETE FROM rat WHERE id=@Id",
            new {Id = id});
    }
    
    public async Task<List<RatSearchResult>> SearchRat(string nameSearchTerm)
    {
        using var db = this.getDatabase();

        var results = await db.QueryAsync<PersistedRatSearchResult>(
            @"SELECT
               rat.id,
               rat.name,
               rat.sex,
               rat.date_of_birth
            FROM rat_search
            JOIN rat ON rat.id=rat_search.id
            WHERE rat_search.name LIKE @NameSearchTerm",
            new {NameSearchTerm = $"%{nameSearchTerm}%"});

        return results.Select(x => x.ToResult()).ToList();
    }

    public class PersistedRatSearchResult
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Sex { get; set; }
        public long? DateOfBirth { get; set; }

        public RatSearchResult ToResult()
        {
            var sex = Enum.TryParse<Sex>(this.Sex, out var parsedSex) ? parsedSex : (Sex?)null;
            return new RatSearchResult(this.Id, this.Name, sex, this.DateOfBirth?.ToDateOnly());
        }
    }
}