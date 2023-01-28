using Core.Contracts;
using Microsoft.EntityFrameworkCore;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Persistence.Repositories;

namespace Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        public ApplicationDbContext? DbContext { get; private set; }

        public UnitOfWork()
        {
            DbContext = new ApplicationDbContext();
            Category = new CategoryRepository(DbContext);
            Perk = new PerkRepository(DbContext);
        }

        public ICategoryRepository Category { get; }
        public IPerkRepository Perk { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            var entities = DbContext!.ChangeTracker.Entries()
                .Where(entity => entity.State == EntityState.Added
                                 || entity.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToArray();  // Geänderte Entities ermitteln

            foreach (var entity in entities)
            {
                ValidateEntity(entity);
            }
            return await DbContext.SaveChangesAsync();
        }

        private void ValidateEntity(object entity)
        {
            var validationContext = new ValidationContext(entity, null, null);
            // so the validating entity class will be able to request this unit of work as a service
            validationContext.InitializeServiceProvider(serviceType => this);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(entity, validationContext, validationResults,
                validateAllProperties: true);

            if (!isValid)
            {
                var memberNames = new List<string>();
                var validationExceptions = new List<ValidationException>();
                foreach (var validationResult in validationResults)
                {
                    validationExceptions.Add(new ValidationException(validationResult, null, null));
                    memberNames.AddRange(validationResult.MemberNames);
                }

                if (validationExceptions.Count == 1)  // eine Validationexception werfen
                {
                    throw validationExceptions.Single();
                }
                else  // AggregateException mit allen ValidationExceptions als InnerExceptions werfen
                {
                    throw new ValidationException($"Entity validation failed for {string.Join(", ", memberNames)}",
                        new AggregateException(validationExceptions));
                }
            }

        }

        public async Task DeleteDatabaseAsync() => await DbContext!.Database.EnsureDeletedAsync();
        public async Task CreateDatabaseAsync() => await DbContext!.Database.EnsureCreatedAsync();
        public async Task MigrateDatabaseAsync() => await DbContext!.Database.MigrateAsync();

        #region Dispose/AsyncDispose lt. MS: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbContext?.Dispose();
                DbContext = null;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (DbContext is not null)
            {
                await DbContext.DisposeAsync().ConfigureAwait(false);
            }

            DbContext = null;
        }
        #endregion
    }
}
