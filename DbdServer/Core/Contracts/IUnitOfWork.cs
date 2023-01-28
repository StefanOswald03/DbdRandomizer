using System;
using System.Threading.Tasks;

namespace Core.Contracts
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        //Set interfaces of repositories

        Task<int> SaveChangesAsync();
        Task DeleteDatabaseAsync();
        Task CreateDatabaseAsync();
        Task MigrateDatabaseAsync();
    }
}
