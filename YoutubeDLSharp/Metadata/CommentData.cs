using System.Text.Json.Serialization;
using System;
using YoutubeDLSharp.Converters;

namespace YoutubeDLSharp.Metadata
{
    //https://github.com/yt-dlp/yt-dlp/blob/9c53b9a1b6b8914e4322263c97c26999f2e5832e/yt_dlp/extractor/common.py#L105-L403
    public class CommentData
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("author_id")]
        public string AuthorID { get; set; }
        [JsonPropertyName("author_thumbnail")]
        public string AuthorThumbnail { get; set; }
        [JsonPropertyName("html")]
        public string Html { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Timestamp { get; set; } //UNIX Timestamp
        [JsonPropertyName("parent")]
        public string Parent { get; set; }
        [JsonPropertyName("like_count")]
        public int? LikeCount { get; set; }
        [JsonPropertyName("dislike_count")]
        public int? DislikeCount { get; set; }
        [JsonPropertyName("is_favorited")]
        public bool? IsFavorited { get; set; }
        [JsonPropertyName("author_is_uploader")]
        public bool? AuthorIsUploader { get; set; }

        //Unused Fields (These are fields that were excluded, but documented for future use:
        //time_text  
    }
}
