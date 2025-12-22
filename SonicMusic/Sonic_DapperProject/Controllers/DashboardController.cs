using Microsoft.AspNetCore.Mvc;
using Sonic_DapperProject.Repositories;

namespace Sonic_DapperProject.Controllers
{
    public class DashboardController(IMusicRepository _musicRepo) : Controller
    {      

        public async Task<IActionResult> Index()
        {
            var model = await _musicRepo.GetDashboardDataAsync();
            return View(model);
        }

        public async Task<IActionResult> Reports()
        {
            var model = await _musicRepo.GetReportsDataAsync();
            return View(model);
        }

        public async Task<IActionResult> MusicLibrary(int page = 1, string search = "")
        {
            var model = await _musicRepo.GetMusicLibraryAsync(page, search);
            return View(model);
        }

        public async Task<IActionResult> AdvancedCharts()
        {
            var model = await _musicRepo.GetDashboardDataAsync();
            return View(model);
        }
    }
}
