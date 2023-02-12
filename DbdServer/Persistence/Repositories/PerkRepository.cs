using Core.Contracts;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class PerkRepository : GenericRepository<Perk>, IPerkRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PerkRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task ClearTable()
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"Delete from {nameof(ApplicationDbContext.Perks)}");
            _dbContext.ChangeTracker.Clear();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Perk[]> GetFourRandom(string role)
        {
            Random random = new Random();
            var allPerks = await _dbContext.Perks.Where(p => p.Role == role).ToListAsync();
            var randomPerks = allPerks.OrderBy(x => random.Next())
                .Take(4)
                .ToArray();
            return randomPerks;
        }
    }
}
