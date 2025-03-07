using System.Text.Json.Serialization;

    public class RecommendationDetails
    {
        public RecommendationDetails(string trackID,string track, string artist, float score)
        {
            TrackID = trackID;
            Track = track;
            Artist = artist;
            Score = score;
        }
        
        public string TrackID { get; private set; }
        public string Track { get; set; }
        public string Artist { get; set; }
        public float Score { get; set; }
    }


