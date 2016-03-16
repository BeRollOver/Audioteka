using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audioteka
{
    class Data<T>
    {
        public class Responce
        {
            [JsonProperty("count")]
            public int Count { get; set; }
            [JsonProperty("items")]
            public List<T> Items { get; set; }
        }

        [JsonProperty("response")]
        public Responce Response { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
        [JsonProperty("photo_50")]
        public string Photo { get; set; }
    }

    public class Post
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("date")]
        public long Date { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; set; }
        
        public DateTime Time { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("audio")]
        public Audio Audio { get; set; }
    }

    public class Audio
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("album_id")]
        public int AlbumID { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }

        public TimeSpan Time { get; set; }
    }

    public class Album
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        public List<Audio> Songs { get; set; } = new List<Audio>();
    }
}
