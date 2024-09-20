using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AltF4_OSC.Misc
{
    [JsonSerializable(typeof(NetworkConfig))]
    [JsonSerializable(typeof(Config))]
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, WriteIndented = true, AllowTrailingCommas = true)]
    internal partial class ConfigSourceGenerationContext : JsonSerializerContext;

    [Serializable]
    public class NetworkConfig
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int ListeningPort { get; set; } = 9001;
        public int SendingPort { get; set; } = 9000;
        public bool UseConfigPorts { get; set; }
    }

    [Serializable]
    public class Config
    {
        static readonly string ConfigPath = $"{AppContext.BaseDirectory}config.json";
        public static Config Instance { get; } = LoadConfig();

        public NetworkConfig Network { get; set; } = new();

        public string Parameter { get; set; } = "Misc/Disconnect";

        public bool AutoScroll { get; set; } = true;

        static Config LoadConfig()
        {
            Config? cfg = File.Exists(ConfigPath) ? JsonSerializer.Deserialize(File.ReadAllText(ConfigPath), ConfigSourceGenerationContext.Default.Config) : null;
            if(cfg == null)
            {
                cfg = new Config();
                cfg.SaveConfig();
            }

            return cfg;
        }

        void SaveConfig()
        {
            string json = JsonSerializer.Serialize(this, ConfigSourceGenerationContext.Default.Config);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
