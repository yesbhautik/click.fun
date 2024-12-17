using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClickTracker.Models
{
    public class ServerConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty("added_date")]
        public DateTime AddedDate { get; set; }
    }

    public class AppSettings
    {
        [JsonProperty("servers")]
        public List<ServerConfig> Servers { get; set; } = new List<ServerConfig>();

        [JsonProperty("selected_server")]
        public string SelectedServer { get; set; }

        [JsonProperty("last_username")]
        public string LastUsername { get; set; }

        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }

        public AppSettings()
        {
            // Add default server
            if (Servers.Count == 0)
            {
                Servers.Add(new ServerConfig
                {
                    Name = "Default Server",
                    ConnectionString = "http://localhost:3000",
                    IsDefault = true,
                    AddedDate = DateTime.UtcNow
                });
                SelectedServer = "Default Server";
            }
        }
    }
} 