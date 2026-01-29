using Microsoft.Extensions.Logging;

namespace Finitech.BuildingBlocks.Infrastructure.Notifications;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
}

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK EMAIL] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK TEMPLATED EMAIL] To: {To}, Template: {Template}", to, templateName);
        return Task.CompletedTask;
    }
}
