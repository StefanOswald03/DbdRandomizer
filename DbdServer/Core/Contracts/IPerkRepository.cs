using Core.Entities;

namespace Core.Contracts
{
    public interface IPerkRepository : IGenericRepository<Perk>
    {
        Task ClearTable();
        Task<Perk[]> GetFourRandomAsync(string role);
    }
}
