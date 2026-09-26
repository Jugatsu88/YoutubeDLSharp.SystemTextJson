using System.Text.Json.Serialization;

namespace YoutubeDLSharp.Metadata
{
    public class ChapterData
    {
        [JsonPropertyName("start_time")]
        public float? StartTime { get; set; }
        [JsonPropertyName("end_time")]
        public float? EndTime { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
