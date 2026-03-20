using Finitech.Modules.Disputes.Application.Services;
using Xunit;

namespace Finitech.UnitTests;

public class DisputeServiceTests
{
    private readonly DisputeApplicationService _service = new();

    [Fact]
    public async Task ProcessRefund_ReturnsCompleted()
    {
        var result = await _service.ProcessRefundAsync(Guid.NewGuid(), 5000, "Produit défectueux", "refund-001");
        Assert.Equal("Completed", result.Status);
        Assert.NotEqual(Guid.Empty, result.RefundId);
    }

    [Fact]
    public async Task InitiateChargeback_ReturnsInitiated()
    {
        var result = await _service.InitiateChargebackAsync(Guid.NewGuid(), "Non autorisée", "Client nie");
        Assert.Equal("Initiated", result.Status);
        Assert.NotEqual(Guid.Empty, result.ChargebackId);
    }
}
