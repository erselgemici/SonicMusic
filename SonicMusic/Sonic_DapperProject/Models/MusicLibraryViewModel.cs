namespace Sonic_DapperProject.Models
{
    public class MusicLibraryViewModel
    {
        public List<TrackDto> Tracks { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
    }

    public class TrackDto
    {
        public int TrackId { get; set; }
        public string TrackName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string GenreName { get; set; }
        public int DurationSeconds { get; set; }
    }
}
