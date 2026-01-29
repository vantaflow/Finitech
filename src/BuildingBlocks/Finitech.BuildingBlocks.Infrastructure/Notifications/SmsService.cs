using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Notifications;

public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    Task SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes = 5, CancellationToken cancellationToken = default);
}

public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK SMS] To: {Phone}, Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }

    public Task SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes = 5, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK OTP] To: {Phone}, Code: {Code}, Expires in: {Expiry} minutes",
            phoneNumber, otpCode, expiryMinutes);
        return Task.CompletedTask;
    }
}
