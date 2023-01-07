namespace Basemix.Rats.Persistence;

public interface IRatsRepository
{
    Task<long> AddRat(Rat rat);
    Task<Rat?> GetRat(long id);
    Task<List<Rat>> GetAll();
}