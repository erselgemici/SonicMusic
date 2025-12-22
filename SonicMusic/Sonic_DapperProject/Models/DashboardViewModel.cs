using System;
using System.Collections.Generic;

namespace Sonic_DapperProject.Models
{
    public class DashboardViewModel
    {
        // --- GENEL KARTLAR ---
        public int TotalListens { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTracks { get; set; }
        public int TotalGenres { get; set; }

        // --- GRAFİK DATALARI ---
        public List<GenreGraphDto> GenreGraphData { get; set; }
        public List<DailyGraphDto> DailyGraphData { get; set; }
        public List<TopSongDto> TopSongs { get; set; }
        public List<CityAnalysisDto> CityGraphData { get; set; }
        public List<DeviceAnalysisDto> DeviceGraphData { get; set; }

        // --- RAPORLAR (REPORT PAGE) ---
        public List<ReportAlbumDto> ReportTopAlbums { get; set; }
        public List<ReportCityDto> ReportTopCities { get; set; }
        public List<ReportTrendDto> ReportTrends { get; set; }       
        public List<ReportTimeDto> ReportPeakHours { get; set; }      
        public List<ReportTopFanDto> ReportTopFans { get; set; }       
        public List<ReportSubscriptionDto> ReportSubscriptions { get; set; }
    }

    // --- DTO SINIFLARI ---
    public class GenreGraphDto { public string GenreName { get; set; } public int Count { get; set; } }
    public class DailyGraphDto { public string Date { get; set; } public int Count { get; set; } }
    public class TopSongDto { public string TrackName { get; set; } public string ArtistName { get; set; } public string AlbumName { get; set; } public int ListenCount { get; set; } }
    public class CityAnalysisDto { public string CityName { get; set; } public int Count { get; set; } }
    public class DeviceAnalysisDto { public string DeviceType { get; set; } public int Count { get; set; } }

    // --- RAPOR DTO'LARI ---
    public class ReportAlbumDto { public string AlbumName { get; set; } public string ArtistName { get; set; } public DateTime ReleaseDate { get; set; } public int ListenCount { get; set; } }
    public class ReportCityDto { public string CityName { get; set; } public string CountryName { get; set; } public int TotalListens { get; set; } }

    public class ReportTrendDto
    {
        public string ArtistName { get; set; }
        public int ThisWeek { get; set; }
        public int LastWeek { get; set; }
        public double GrowthRate { get; set; }
    }
    public class ReportTimeDto
    {
        public int Hour { get; set; }
        public int ListenCount { get; set; }
        public string TimeLabel => $"{Hour:00}:00 - {Hour:00}:59";
    }
    public class ReportTopFanDto
    {
        public string UserName { get; set; }
        public string City { get; set; }
        public int TotalListens { get; set; }
        public string FavoriteGenre { get; set; }
    }
    public class ReportSubscriptionDto
    {
        public string SubscriptionType { get; set; }
        public int UserCount { get; set; }
        public int TotalListens { get; set; }
        public double AvgListenPerUser => UserCount == 0 ? 0 : (double)TotalListens / UserCount;
    }
}
