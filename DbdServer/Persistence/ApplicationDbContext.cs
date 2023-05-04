using System.Reflection;

using Base.Helper;

using Core.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Perk> Perks => Set<Perk>();
        public DbSet<PerkTranslation> PerkTranslations => Set<PerkTranslation>();

        public ApplicationDbContext()
        {
        }

        /// <summary>
        /// Für InMemory-DB in UnitTests
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = ConfigurationHelper.GetConfiguration();

                string? connectionString = configuration["ConnectionStrings:DefaultConnection"];

                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.UseLoggerFactory(GetLoggerFactory());

            }
        }

        static ILoggerFactory? GetLoggerFactory()
        {

            Log.Logger = new LoggerConfiguration()
                                          .ReadFrom.Configuration(ConfigurationHelper.GetConfiguration())
                                          .CreateLogger();

            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddSerilog())
                .BuildServiceProvider();

            return serviceProvider.GetService<ILoggerFactory>();
        }

     
    }
}
