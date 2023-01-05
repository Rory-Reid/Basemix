﻿using Dapper;

namespace Basemix.Rats.Persistence;

public class RatsRepository
{
    private readonly GetDatabase getDatabase;

    public RatsRepository(GetDatabase getDatabase)
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

    public async Task<Rat> GetRat(long id)
    {
        using var db = this.getDatabase();

        var rat = await db.QuerySingleOrDefaultAsync<PersistedRat>(
            @"SELECT
                id, name, sex, date_of_birth, genotype, variety, notes, birth_notes, type_score,
                temperament_score, date_of_death, death_reason
            FROM rat WHERE id=@Id",
            new { Id = id });

        return rat.AsModelledRat();
    }

    public async Task<List<Rat>> GetAll()
    {
        using var db = this.getDatabase();

        var rat = await db.QueryAsync<PersistedRat>(
            @"SELECT
                id, name, sex, date_of_birth, genotype, variety, notes, birth_notes, type_score,
                temperament_score, date_of_death, death_reason
            FROM rat
            ORDER BY date_of_birth DESC");

        return rat.Select(x => x.AsModelledRat()).ToList();
    }
}