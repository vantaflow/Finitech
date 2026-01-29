namespace Finitech.BuildingBlocks.Domain.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
}
