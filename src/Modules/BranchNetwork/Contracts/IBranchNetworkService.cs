using Finitech.Modules.BranchNetwork.Contracts.DTOs;

namespace Finitech.Modules.BranchNetwork.Contracts;

public interface IBranchNetworkService
{
    Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetBranchByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> ListBranchesAsync(string? city = null, string? service = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchSearchResultDto>> FindNearbyAsync(FindNearbyRequest request, CancellationToken cancellationToken = default);
    Task UpdateBranchAsync(Guid branchId, CreateBranchRequest request, CancellationToken cancellationToken = default);
    Task CloseBranchAsync(Guid branchId, string reason, CancellationToken cancellationToken = default);
}
