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
    }
}
