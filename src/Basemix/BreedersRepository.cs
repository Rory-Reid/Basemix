using Basemix.Persistence;
using Dapper;

namespace Basemix;

public class BreedersRepository
{
    private readonly GetDatabase getDatabase;

    public BreedersRepository(GetDatabase getDatabase) =>
        this.getDatabase = getDatabase;

    public async Task SetMyBreeder(Breeder input)
    {
        using var db = this.getDatabase();

        var breeder = PersistedBreeder.FromBreeder(input);
        await db.ExecuteAsync(
            @"INSERT INTO breeder (id, name, founded, active, owned)
            VALUES (1, @Name, @Founded, @Active, @Owned)
            ON CONFLICT(id) DO UPDATE
              SET name=@Name,
                  founded=@Founded,
                  active=@Active,
                  owned=@Owned",
            new
            {
                Name = breeder.Name,
                Founded = breeder.Founded,
                Active = breeder.Active,
                Owned = breeder.Owned
            });
    }

    public async Task<Breeder?> GetMyBreeder()
    {
        using var db = this.getDatabase();

        var breeder = await db.QuerySingleOrDefaultAsync<PersistedBreeder>(
            @"SELECT id, name, founded, active, owned
            FROM breeder
            WHERE id=1");

        return breeder?.ToBreeder();
    }
}

public class Breeder
{
    public string Name { get; set; } = null!;
    public DateTime? Founded { get; set; }
    public bool Active { get; set; }
    public bool Owned { get; set; }
}

public class PersistedBreeder
{
    public string Name { get; set; } = null!;
    public long? Founded { get; set; }
    public bool Active { get; set; }
    public bool Owned { get; set; }
    
    public static PersistedBreeder FromBreeder(Breeder breeder) =>
        new()
        {
            Name = breeder.Name,
            Founded = breeder.Founded?.ToPersistedDateTime(),
            Active = breeder.Active,
            Owned = breeder.Owned
        };

    public Breeder ToBreeder() =>
        new Breeder()
        {
            Name = this.Name,
            Founded = this.Founded?.ToDateTime(),
            Active = this.Active,
            Owned = this.Owned
        };
}