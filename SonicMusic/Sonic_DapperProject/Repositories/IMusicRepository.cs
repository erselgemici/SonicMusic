using Sonic_DapperProject.Models;

namespace Sonic_DapperProject.Repositories
{
    public interface IMusicRepository
    {
        Task<DashboardViewModel> GetDashboardDataAsync();

        Task<DashboardViewModel> GetReportsDataAsync();

        Task<MusicLibraryViewModel> GetMusicLibraryAsync(int page, string search);
    }
}
