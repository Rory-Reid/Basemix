namespace Basemix.Rats.Persistence;

public interface IRatsRepository
{
    Task<long> CreateRat();
    Task<long> AddRat(Rat rat);
    Task UpdateRat(Rat rat);
    Task<Rat?> GetRat(long id);
    Task<List<Rat>> GetAll();
    Task DeleteRat(long id);
    Task<List<RatSearchResult>> SearchRat(string nameSearchTerm);
}