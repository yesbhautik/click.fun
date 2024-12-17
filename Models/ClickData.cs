using System;
using Newtonsoft.Json;

namespace ClickTracker.Models
{
    public class ClickData
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("mouseClicks")]
        public int MouseClicks { get; set; }

        [JsonProperty("keyboardPresses")]
        public int KeyboardPresses { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        public ClickData()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
} 