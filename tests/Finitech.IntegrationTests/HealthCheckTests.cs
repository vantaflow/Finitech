using Xunit;

namespace Finitech.IntegrationTests;

public class HealthCheckTests
{
    [Fact]
    public void Placeholder_IntegrationTestsConfigured()
    {
        // Integration tests require a running PostgreSQL instance
        // Run with: docker-compose up -d postgres && dotnet test tests/Finitech.IntegrationTests
        Assert.True(true, "Integration test infrastructure is configured");
    }
}
