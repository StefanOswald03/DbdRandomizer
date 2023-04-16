using System;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        //Set interfaces of repositories
        ICategoryRepository Category { get; }
        IPerkRepository Perk { get; }

        Task<int> SaveChangesAsync();
        Task DeleteDatabaseAsync();
        Task CreateDatabaseAsync();
        Task MigrateDatabaseAsync();
    }
}
