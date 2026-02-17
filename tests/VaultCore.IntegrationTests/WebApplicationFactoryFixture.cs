using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace VaultCore.IntegrationTests;

/// <summary>
/// Test host using in-memory database for integration tests (no PostgreSQL required).
/// </summary>
public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IDisposable
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDb"] = "true"
            });
        });
    }
}
