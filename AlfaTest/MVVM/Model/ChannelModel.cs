using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace AlfaTest.MVVM.Model
{
    [Serializable]
    public class ChannelModel
    {
        [XmlElement("item")]
        [JsonPropertyName("item")]
        public ChannelItem[] Items { get; set; }
    }

    [Serializable]
    public class ChannelItem
    {
        [XmlElement("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [XmlElement("link")]
        [JsonPropertyName("link")]
        public string? Link { get; set; }

        [XmlElement("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [XmlElement("category")]
        [JsonPropertyName("category")]
        public string? Categories { get; set; }

        [XmlElement("pubDate")]
        [JsonPropertyName("pubDate")]
        public string? PubDate 
        { 
            get { return Date.ToString(); }
            set { Date = DateTime.ParseExact(value, "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture); }
        }

        [XmlIgnore]
        [JsonIgnore]
        public DateTime Date { get; set; }
    }
}