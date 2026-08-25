using Host.WebApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SharedKernal.Seeding;

namespace Users.Infrastructure.UnitTests;

public sealed class DataSeedingHostedServiceTests
{
    [Fact]
    public async Task StartAsync_WhenDisabled_DoesNotRunSeeders()
    {
        var calls = new List<int>();
        var hostedService = CreateHostedService(false, "Development", calls);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Empty(calls);
    }

    [Fact]
    public async Task StartAsync_WhenEnabled_RunsSeedersInOrder()
    {
        var calls = new List<int>();
        var hostedService = CreateHostedService(true, "Development", calls);

        await hostedService.StartAsync(CancellationToken.None);

        Assert.Equal([10, 20], calls);
    }

    [Fact]
    public async Task StartAsync_InDisallowedEnvironment_FailsBeforeRunningSeeders()
    {
        var calls = new List<int>();
        var hostedService = CreateHostedService(true, "Production", calls);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            hostedService.StartAsync(CancellationToken.None));

        Assert.Empty(calls);
    }

    private static DataSeedingHostedService CreateHostedService(
        bool enabled,
        string environmentName,
        List<int> calls)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedData:Enabled"] = enabled.ToString(),
                ["SeedData:AllowedEnvironments:0"] = "Development"
            })
            .Build();
        var services = new ServiceCollection()
            .AddSingleton<IDataSeeder>(new RecordingSeeder(20, calls))
            .AddSingleton<IDataSeeder>(new RecordingSeeder(10, calls))
            .BuildServiceProvider();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(candidate => candidate.EnvironmentName).Returns(environmentName);

        return new DataSeedingHostedService(
            services,
            configuration,
            environment.Object,
            NullLogger<DataSeedingHostedService>.Instance);
    }

    private sealed class RecordingSeeder(int order, List<int> calls) : IDataSeeder
    {
        public int Order => order;

        public Task SeedAsync(CancellationToken cancellationToken = default)
        {
            calls.Add(order);
            return Task.CompletedTask;
        }
    }
}
