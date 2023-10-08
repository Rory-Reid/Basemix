using Basemix.Lib.Persistence;
using Basemix.Lib.Rats;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Basemix.Lib.Litters.Persistence;

public class SqliteLittersRepository : ILittersRepository
{
    private readonly GetDatabase getDatabase;

    public SqliteLittersRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<Litter?> GetLitter(long id)
    {
        using var db = this.getDatabase();

        using var reader = await db.QueryMultipleAsync(
            @"SELECT
                litter.id, litter.name, litter.bred_by_me, litter.dam_id,
                litter.sire_id, litter.date_of_birth, litter.date_of_pairing,
                dam.name as dam_name, sire.name AS sire_name,
                litter.notes
            FROM litter
            LEFT JOIN rat dam on dam.id=dam_id
            LEFT JOIN rat sire on sire.id=sire_id
            WHERE litter.id=@Id;

            SELECT offspring.id, offspring.name, owner.name AS owner_name
            FROM rat offspring
            LEFT JOIN owner ON owner.id=offspring.owner_id
            WHERE offspring.litter_id=@Id",
            new { Id = id });

        var litter = await reader.ReadSingleOrDefaultAsync<LitterReadModel?>();
        if (litter == null)
        {
            return null;
        }
        
        var offspring = await reader.ReadAsync<LitterOffspringReadModel>();

        return litter.ToModelledLitter(offspring);
    }
    
    public async Task<List<LitterOverview>> GetAll()
    {
        using var db = this.getDatabase();

        var litters = await db.QueryAsync<PersistedLitterOverview>(
            @"SELECT
                litter.id,
                litter.date_of_birth,
                litter.name,
                sire.name AS sire,
                dam.name AS dam,
                (SELECT COUNT(id) FROM rat WHERE rat.litter_id=litter.id) AS offspring_count
            FROM litter
            LEFT JOIN rat sire on sire.id=litter.sire_id
            LEFT JOIN rat dam on dam.id=litter.dam_id
            ORDER BY litter.date_of_birth DESC");

        return litters.Select(x => x.ToModelledOverview()).ToList();
    }
    
    public async Task<long> CreateLitter(RatIdentity? damId = null, RatIdentity? sireId = null)
    {
        using var db = this.getDatabase();

        return await db.ExecuteScalarAsync<long>(
            @"INSERT INTO litter (dam_id, sire_id)
            VALUES (@DamId, @SireId)
            RETURNING id",
            new PersistedLitter {DamId = damId, SireId = sireId});
    }

    public async Task UpdateLitter(Litter litter)
    {
        using var db = this.getDatabase();

        await db.ExecuteAsync(
            @"UPDATE litter
            SET
                name=@Name,
                bred_by_me=@BredByMe,
                dam_id=@DamId,
                sire_id=@SireId,
                date_of_pairing=@DateOfPairing,
                date_of_birth=@DateOfBirth,
                notes=@Notes
            WHERE id=@Id",
            new PersistedLitter(litter));
    }

    public async Task<AddOffspringResult> AddOffspring(LitterIdentity id, RatIdentity ratId)
    {
        using var db = this.getDatabase();

        try
        {
            var rowsAffected = await db.ExecuteAsync(
                @"UPDATE rat
                SET litter_id=@LitterId
                WHERE id=@OffspringId",
                new {LitterId = id.Value, OffspringId = ratId.Value});
            
            if (rowsAffected == 0)
            {
                return AddOffspringResult.NonExistantRatOrLitter;
            }
        }
        catch (SqliteException e)
        {
            if (e.SqliteErrorCode is SqliteErrorCode.SqliteConstraint)
            {
                return AddOffspringResult.NonExistantRatOrLitter;
            }

            throw;
        }

        return AddOffspringResult.Success;
    }

    public async Task<RemoveOffspringResult> RemoveOffspring(LitterIdentity id, RatIdentity ratId)
    {
        using var db = this.getDatabase();

        var rowsAffected = await db.ExecuteAsync(
            @"UPDATE rat
            SET litter_id=NULL
            WHERE id=@OffspringId AND litter_id=@LitterId",
            new {LitterId = id.Value, OffspringId = ratId.Value});

        return rowsAffected != 0
            ? RemoveOffspringResult.Success
            : RemoveOffspringResult.NothingToRemove;
    }
    
    public async Task DeleteLitter(LitterIdentity id)
    {
        using var db = this.getDatabase();
        db.Open();
        using var transaction = db.BeginTransaction();
        
        await db.ExecuteAsync("UPDATE rat SET litter_id=NULL WHERE litter_id=@Id", new {Id = id.Value}, transaction);
        await db.ExecuteAsync("DELETE FROM litter WHERE id=@Id", new {Id = id.Value}, transaction);
        
        transaction.Commit();
    }
}

public enum AddOffspringResult
{
    Error = default,
    Success,
    NonExistantRatOrLitter,
}

public enum RemoveOffspringResult
{
    Error = default,
    Success,
    NothingToRemove
}