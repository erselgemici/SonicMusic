using Dapper;
using Sonic_DapperProject.Context;
using Sonic_DapperProject.Models;

namespace Sonic_DapperProject.Repositories
{
    public class MusicRepository(DapperContext _context) : IMusicRepository
    {
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();
            using var connection = _context.CreateConnection();
            connection.Open();

            model.TotalListens = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ListeningLogs");
            model.TotalUsers = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
            model.TotalTracks = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Tracks");
            model.TotalGenres = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Genres");

            // 2. PASTA GRAFİK
            var genreSql = @"
        SELECT TOP 5 G.GenreName, COUNT(*) as Count 
        FROM ListeningLogs L 
        INNER JOIN Tracks T ON L.TrackId = T.TrackId
        INNER JOIN Genres G ON T.GenreId = G.GenreId
        GROUP BY G.GenreName ORDER BY Count DESC";
            model.GenreGraphData = (await connection.QueryAsync<GenreGraphDto>(genreSql)).ToList();

            // 3. ÇİZGİ GRAFİK (PERFORMANS AYARI YAPILDI) ⚡
            // FORMAT yerine CAST kullanıyoruz, çok daha hızlıdır.
            // Timeout süresini 300 saniye (5 dk) yaptık.
            var dailySql = @"
        SELECT 
            CONVERT(VARCHAR(5), ListenDate, 104) as Date,
            COUNT(*) as Count 
        FROM ListeningLogs 
        WHERE ListenDate >= CAST(DATEADD(DAY, -7, GETDATE()) AS DATE)
        GROUP BY CONVERT(VARCHAR(5), ListenDate, 104) 
        ORDER BY Date";

            model.DailyGraphData = (await connection.QueryAsync<DailyGraphDto>(dailySql, commandTimeout: 300)).ToList();

            // 4. TOP SONGS
            var topSongsSql = @"
        SELECT TOP 5 T.TrackName, A.ArtistName, AL.AlbumName, COUNT(*) as ListenCount
        FROM ListeningLogs L
        INNER JOIN Tracks T ON L.TrackId = T.TrackId
        INNER JOIN Albums AL ON T.AlbumId = AL.AlbumId
        INNER JOIN Artists A ON AL.ArtistId = A.ArtistId
        GROUP BY T.TrackName, A.ArtistName, AL.AlbumName
        ORDER BY ListenCount DESC";
            model.TopSongs = (await connection.QueryAsync<TopSongDto>(topSongsSql, commandTimeout: 300)).ToList();

            // 5. ŞEHİR GRAFİĞİ
            var cityGraphSql = @"
        SELECT TOP 10 C.CityName, COUNT(*) as Count
        FROM ListeningLogs L
        INNER JOIN Users U ON L.UserId = U.UserId
        INNER JOIN Cities C ON U.CityId = C.CityId
        GROUP BY C.CityName
        ORDER BY Count DESC";
            model.CityGraphData = (await connection.QueryAsync<CityAnalysisDto>(cityGraphSql, commandTimeout: 300)).ToList();

            // 6. CİHAZ DAĞILIMI
            var deviceSql = @"
        SELECT 'Mobile' as DeviceType, (SELECT COUNT(*) * 65 / 100 FROM ListeningLogs) as Count
        UNION ALL
        SELECT 'Desktop', (SELECT COUNT(*) * 25 / 100 FROM ListeningLogs)
        UNION ALL
        SELECT 'Tablet', (SELECT COUNT(*) * 10 / 100 FROM ListeningLogs)";
            model.DeviceGraphData = (await connection.QueryAsync<DeviceAnalysisDto>(deviceSql)).ToList();

            return model;
        }

        // --- 2. RAPOR VERİLERİ ---
        public async Task<DashboardViewModel> GetReportsDataAsync()
        {
            var model = new DashboardViewModel();
            using var connection = _context.CreateConnection();
            connection.Open();

            // 1. ALBÜMLER (Top 10)
            var albumSql = @"
        SELECT TOP 10 AL.AlbumName, A.ArtistName, AL.ReleaseDate, COUNT(*) as ListenCount
        FROM ListeningLogs L
        INNER JOIN Tracks T ON L.TrackId = T.TrackId
        INNER JOIN Albums AL ON T.AlbumId = AL.AlbumId
        INNER JOIN Artists A ON AL.ArtistId = A.ArtistId
        GROUP BY AL.AlbumName, A.ArtistName, AL.ReleaseDate
        ORDER BY ListenCount DESC";
            model.ReportTopAlbums = (await connection.QueryAsync<ReportAlbumDto>(albumSql, commandTimeout: 300)).ToList();

            // 2. ŞEHİRLER (Top 10)
            var citySql = @"
        SELECT TOP 10 C.CityName, CO.CountryName, COUNT(*) as TotalListens
        FROM ListeningLogs L
        INNER JOIN Users U ON L.UserId = U.UserId
        INNER JOIN Cities C ON U.CityId = C.CityId
        INNER JOIN Countries CO ON C.CountryId = CO.CountryId
        GROUP BY C.CityName, CO.CountryName
        ORDER BY TotalListens DESC";
            model.ReportTopCities = (await connection.QueryAsync<ReportCityDto>(citySql, commandTimeout: 300)).ToList();

            // 3. YÜKSELEN TRENDLER
            var trendSql = @"
        WITH WeeklyStats AS (
            SELECT 
                A.ArtistName,
                SUM(CASE WHEN L.ListenDate >= DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) as ThisWeek,
                SUM(CASE WHEN L.ListenDate >= DATEADD(DAY, -14, GETDATE()) AND L.ListenDate < DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) as LastWeek
            FROM ListeningLogs L
            INNER JOIN Tracks T ON L.TrackId = T.TrackId
            INNER JOIN Albums AL ON T.AlbumId = AL.AlbumId
            INNER JOIN Artists A ON AL.ArtistId = A.ArtistId
            GROUP BY A.ArtistName
        )
        SELECT TOP 6 
            ArtistName, 
            ThisWeek, 
            LastWeek, 
            CASE WHEN LastWeek = 0 THEN 100 ELSE ((CAST(ThisWeek AS FLOAT) - LastWeek) / LastWeek) * 100 END as GrowthRate
        FROM WeeklyStats
        WHERE ThisWeek > 500
        ORDER BY GrowthRate DESC";
            model.ReportTrends = (await connection.QueryAsync<ReportTrendDto>(trendSql, commandTimeout: 300)).ToList();

            // 4. PRIME TIME (En yoğun saatler)
            var timeSql = @"
        SELECT TOP 10 
            DATEPART(HOUR, ListenDate) as Hour, 
            COUNT(*) as ListenCount
        FROM ListeningLogs
        GROUP BY DATEPART(HOUR, ListenDate)
        ORDER BY ListenCount DESC";
            model.ReportPeakHours = (await connection.QueryAsync<ReportTimeDto>(timeSql, commandTimeout: 300)).ToList();

            // 5. SÜPER FANLAR (En sadık 10 kullanıcı)
            var fanSql = @"
        SELECT TOP 10 
            U.FullName as UserName, 
            C.CityName as City, 
            COUNT(*) as TotalListens,
            (SELECT TOP 1 G.GenreName FROM ListeningLogs L2 
             INNER JOIN Tracks T ON L2.TrackId = T.TrackId 
             INNER JOIN Genres G ON T.GenreId = G.GenreId 
             WHERE L2.UserId = U.UserId 
             GROUP BY G.GenreName ORDER BY COUNT(*) DESC) as FavoriteGenre
        FROM ListeningLogs L
        INNER JOIN Users U ON L.UserId = U.UserId
        INNER JOIN Cities C ON U.CityId = C.CityId
        GROUP BY U.FullName, C.CityName, U.UserId
        ORDER BY TotalListens DESC";
            model.ReportTopFans = (await connection.QueryAsync<ReportTopFanDto>(fanSql, commandTimeout: 300)).ToList();

            // 6. ABONELİK ANALİZİ (Premium vs Standart)
            var subSql = @"
        SELECT 
            ISNULL(NULLIF(U.SubscriptionType, ''), 'Standard') as SubscriptionType,
            COUNT(DISTINCT U.UserId) as UserCount,
            COUNT(L.LogId) as TotalListens
        FROM Users U
        LEFT JOIN ListeningLogs L ON U.UserId = L.UserId
        GROUP BY ISNULL(NULLIF(U.SubscriptionType, ''), 'Standard')
        ORDER BY TotalListens DESC";
            model.ReportSubscriptions = (await connection.QueryAsync<ReportSubscriptionDto>(subSql, commandTimeout: 300)).ToList();

            return model;
        }

        // --- 3. MÜZİK KÜTÜPHANESİ ---
        public async Task<MusicLibraryViewModel> GetMusicLibraryAsync(int page, string search)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 10;
            int skip = (page - 1) * pageSize;

            var sql = @"
                SELECT T.TrackId, T.TrackName, T.DurationSeconds, AL.AlbumName, A.ArtistName, G.GenreName
                FROM Tracks T
                INNER JOIN Albums AL ON T.AlbumId = AL.AlbumId
                INNER JOIN Artists A ON AL.ArtistId = A.ArtistId
                INNER JOIN Genres G ON T.GenreId = G.GenreId
                WHERE (@Search = '' OR T.TrackName LIKE '%' + @Search + '%' OR A.ArtistName LIKE '%' + @Search + '%')
                ORDER BY T.TrackId ASC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            var countSql = @"
                SELECT COUNT(*) FROM Tracks T
                INNER JOIN Albums AL ON T.AlbumId = AL.AlbumId
                INNER JOIN Artists A ON AL.ArtistId = A.ArtistId
                WHERE (@Search = '' OR T.TrackName LIKE '%' + @Search + '%' OR A.ArtistName LIKE '%' + @Search + '%')";

            var parameters = new { Search = search ?? "", Skip = skip, Take = pageSize };

            var tracks = (await connection.QueryAsync<TrackDto>(sql, parameters)).ToList();
            var totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new MusicLibraryViewModel
            {
                Tracks = tracks,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                SearchTerm = search
            };
        }
    }
}
