using Basemix.Persistence;
using Basemix.Rats;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Basemix.Litters.Persistence;

public class LittersRepository
{
    private readonly GetDatabase getDatabase;

    public LittersRepository(GetDatabase getDatabase)
    {
        this.getDatabase = getDatabase;
    }

    public async Task<Litter> GetLitter(long id)
    {
        using var db = this.getDatabase();

        using var reader = await db.QueryMultipleAsync(
            @"SELECT
                litter.id, litter.dam_id, litter.sire_id, litter.date_of_birth,
                dam.name as dam_name, sire.name AS sire_name
            FROM litter
            LEFT JOIN rat dam on dam.id=dam_id
            LEFT JOIN rat sire on sire.id=sire_id
            WHERE litter.id=@Id;

            SELECT rat.id, rat.name
            FROM litter_kin
            LEFT JOIN rat on litter_kin.offspring_id = rat.id
            WHERE litter_kin.litter_id=@Id",
            new { Id = id });

        var litter = await reader.ReadSingleAsync<LitterReadModel>();
        var offspring = await reader.ReadAsync<LitterOffspringReadModel>();

        return litter.ToModelledLitter(offspring);
    }
    
    public Task<long> AddLitter(RatIdentity? damId = null, RatIdentity? sireId = null)
    {
        using var db = this.getDatabase();

        return db.ExecuteScalarAsync<long>(
            @"INSERT INTO litter (dam_id, sire_id)
            VALUES (@DamId, @SireId)
            RETURNING id",
            new PersistedLitter {DamId = damId, SireId = sireId});
    }

    public Task UpdateLitter(Litter litter)
    {
        using var db = this.getDatabase();

        return db.ExecuteAsync(
            @"UPDATE litter
            SET
                dam_id=@DamId,
                sire_id=@SireId,
                date_of_birth=@DateOfBirth
            WHERE id=@Id",
            new PersistedLitter(litter));
    }

    public async Task<AddOffspringResult> AddOffspring(LitterIdentity id, RatIdentity ratId)
    {
        using var db = this.getDatabase();

        try
        {
            await db.ExecuteAsync(
                @"INSERT INTO litter_kin (litter_id, offspring_id) VALUES (@LitterId, @OffspringId)
                ON CONFLICT DO NOTHING",
                new {LitterId = id.Value, OffspringId = ratId.Value});
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

    public Task RemoveOffspring(LitterIdentity id, RatIdentity ratId)
    {
        using var db = this.getDatabase();
        
        return db.ExecuteAsync(
            @"DELETE FROM litter_kin WHERE litter_id=@LitterId AND offspring_id=@OffspringId",
            new {LitterId = id.Value, OffspringId = ratId.Value});
    }
    
    public Task DeleteLitter(LitterIdentity id)
    {
        using var db = this.getDatabase();
        return db.ExecuteAsync("DELETE FROM litter WHERE id=@Id", new {Id = id.Value});
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
    Success
}