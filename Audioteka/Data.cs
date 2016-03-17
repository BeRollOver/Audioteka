﻿using Newtonsoft.Json;
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
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        
        public DateTime Time { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("photo")]
        public Media Photo { get; set; }
        [JsonProperty("video")]
        public Media Video { get; set; }
        [JsonProperty("audio")]
        public Audio Audio { get; set; }
        [JsonProperty("doc")]
        public Media Doc { get; set; }
        [JsonProperty("graffiti")]
        public Media Graffiti { get; set; }
        [JsonProperty("page")]
        public Media Page { get; set; }
        [JsonProperty("note")]
        public Media Note { get; set; }
        [JsonProperty("poll")]
        public Media Poll { get; set; }
        [JsonProperty("album")]
        public Media AlbumPhoto { get; set; }
        [JsonProperty("link")]
        public Link Link { get; set; }
    }

    public class Media
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("owner_id")]
        public int OwnerId { get; set; }
    }
    public class Audio : Media
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("album_id")]
        public int AlbumID { get; set; }
        [JsonProperty("duration")]
        public int Duration { get; set; }

        public TimeSpan Time { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
    public class Photo : Media
    {

    }
    public class Video : Media
    {

    }
    public class Doc : Media
    {

    }
    public class Graffiti : Media
    {

    }
    public class Note : Media
    {

    }
    public class Poll : Media
    {

    }
    public class PageWiki : Media
    {

    }
    public class AlbumPhoto : Media
    {

    }

    public class Link
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Album
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        public List<Audio> Songs { get; set; } = new List<Audio>();

        public override string ToString()
        {
            return Title;
        }
    }

    class Result
    {
        [JsonProperty("response")]
        public int Response { get; set; }
    }
}
