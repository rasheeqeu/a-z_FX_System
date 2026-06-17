using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.Services;

public interface IWorkspaceLayoutService
{
    Task<WorkspaceLayout> LoadAsync();
    Task SaveAsync(WorkspaceLayout layout);
}
