using Api;
using Core.Contracts;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

using Persistence;

using Serilog;

using System.Net.Http;
using System.Threading.Tasks;

namespace Tests.ApiTests
{
    public static class ApiTestHelpers
    {
        public static HttpClient GetClient()
        {
            var webBuilder = new WebHostBuilder();
            webBuilder.UseSerilog();
            TestServer server = new(webBuilder.UseStartup<Startup>());
            HttpClient client = server.CreateClient();
            return client;
        }


        public async static Task RecreateDatabaseAsync()
        {
            using ApplicationDbContext dbContext = new();
            using IUnitOfWork uow = new UnitOfWork();
            await uow.DeleteDatabaseAsync();
            await uow.CreateDatabaseAsync();
        }
    }
}
