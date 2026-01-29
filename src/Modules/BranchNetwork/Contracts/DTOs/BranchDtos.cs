namespace Finitech.Modules.BranchNetwork.Contracts.DTOs;

public record BranchDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AddressDto Address { get; init; } = new();
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public List<string> Services { get; init; } = new();
    public WorkingHoursDto WorkingHours { get; init; } = new();
    public string Status { get; init; } = string.Empty; // Active, Closed, TemporaryClosed
}

public record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string? AdditionalAddress { get; init; }
    public string City { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
    public string Country { get; init; } = string.Empty;
}

public record WorkingHoursDto
{
    public WorkingDayDto Monday { get; init; } = new();
    public WorkingDayDto Tuesday { get; init; } = new();
    public WorkingDayDto Wednesday { get; init; } = new();
    public WorkingDayDto Thursday { get; init; } = new();
    public WorkingDayDto Friday { get; init; } = new();
    public WorkingDayDto Saturday { get; init; } = new();
    public WorkingDayDto Sunday { get; init; } = new();
}

public record WorkingDayDto
{
    public bool IsOpen { get; init; }
    public TimeSpan? OpenTime { get; init; }
    public TimeSpan? CloseTime { get; init; }
    public List<TimeSlotDto>? Breaks { get; init; }
}

public record TimeSlotDto
{
    public TimeSpan Start { get; init; }
    public TimeSpan End { get; init; }
}

public record CreateBranchRequest
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AddressDto Address { get; init; } = new();
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public List<string> Services { get; init; } = new();
    public WorkingHoursDto WorkingHours { get; init; } = new();
}

public record FindNearbyRequest
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double RadiusKm { get; init; } = 10;
    public int MaxResults { get; init; } = 20;
}

public record BranchSearchResultDto
{
    public BranchDto Branch { get; init; } = new();
    public double DistanceKm { get; init; }
}
