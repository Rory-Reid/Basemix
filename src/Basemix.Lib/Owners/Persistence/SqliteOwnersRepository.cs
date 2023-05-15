using Dapper;

namespace Basemix.Lib.Owners.Persistence;

public class SqliteOwnersRepository : IOwnersRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteOwnersRepository(GetDatabase getDatabase) =>
        this.getDatabase = getDatabase;

    public async Task<Owner?> GetOwner(OwnerIdentity id)
    {
        using var db = this.getDatabase();
        var reader = await db.QueryMultipleAsync(
            @"SELECT
                id,
                name,
                email,
                phone,
                notes
            FROM owner
            WHERE id=@Id;

            SELECT rat.id, rat.name
            FROM rat
            JOIN owner o on o.id=rat.owner_id
            WHERE o.id=@Id;",
            new {Id = id.Value});

        var owner = await reader.ReadSingleOrDefaultAsync<PersistedOwner>();
        if (owner == null)
        {
            return null;
        }
        
        var rats = await reader.ReadAsync<PersistedOwnedRat>();
        return owner.ToModelledOwner(rats);
    }

    public async Task<OwnerIdentity> CreateOwner()
    {
        using var db = this.getDatabase();
        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO owner DEFAULT VALUES RETURNING id");
    }

    public Task UpdateOwner(Owner owner)
    {
        using var db = this.getDatabase();
        return db.ExecuteAsync(
            @"UPDATE owner
            SET
                name=@Name,
                email=@Email,
                phone=@Phone,
                notes=@Notes
            WHERE id=@Id",
            new PersistedOwner(owner));
    }

    public async Task DeleteOwner(OwnerIdentity id)
    {
        using var db = this.getDatabase();
        db.Open();
        using var transaction = db.BeginTransaction();
        
        await db.ExecuteAsync("UPDATE rat SET owner_id=NULL WHERE owner_id=@Id", new {Id = id.Value}, transaction);
        await db.ExecuteAsync(@"DELETE FROM owner WHERE id=@Id", new {Id = id.Value}, transaction);
        
        transaction.Commit();
    }

    public async Task<List<OwnerSearchResult>> SearchOwner(string? nameSearchTerm = null)
    {
        using var db = this.getDatabase();
        var nameLike = nameSearchTerm == null ? string.Empty : $"%{nameSearchTerm}%";
        
        var results = await db.QueryAsync<PersistedOwnerSearchResult>(
            @$"SELECT
                owner.id,
                owner.name
            FROM owner
            JOIN owner_search ON owner.id=owner_search.id
            {Filters(nameLike)}
            ORDER BY owner.name ASC, owner.id",
            new {NameSearchTerm = nameLike});

        return results.Select(x => x.ToResult()).ToList();
    }

    private static string Filters(string? nameSearchTerm)
    {
        var filters = new List<string>();
        if (!string.IsNullOrEmpty(nameSearchTerm))
        {
            filters.Add("(owner_search.name LIKE @NameSearchTerm)");
        }
        
        return filters.Any() ? $"WHERE {string.Join(" AND ", filters)}" : string.Empty;
    }
}