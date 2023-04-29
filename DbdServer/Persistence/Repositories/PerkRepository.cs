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

        // This method schould clear the table Perks and reset the autoincrement
        public async Task ClearTable()
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{nameof(ApplicationDbContext.Perks)}', RESEED, 0)");
            await _dbContext.Database.ExecuteSqlRawAsync($"Delete from {nameof(ApplicationDbContext.Perks)}");
            _dbContext.ChangeTracker.Clear();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Perk[]> GetFourRandomAsync(string role)
        {
            Random random = new();
            var allPerks = await _dbContext.Perks
                .Where(p => p.Role == role)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var randomPerks = allPerks
                .OrderBy(x => random.Next())
                .Take(4)
                .ToArray();

            const int ITEMS_PER_PAGE = 15;
            foreach (var perk in randomPerks)
            {
                int index = allPerks.IndexOf(perk);
                int page = index / ITEMS_PER_PAGE;
                perk.Page = page;
            }

            return randomPerks;
        }
    }
}
