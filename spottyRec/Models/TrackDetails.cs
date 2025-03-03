namespace spottyRec.Models
{
    public class TrackDetails
    {
        public string trackID { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string genre { get; set; }
        public int year { get; set; }
        public float danceability { get; set; }
        public float energy { get; set; }
        public float tempo { get; set; }
        public float valence { get; set; }
    }
}
