using Newtonsoft.Json;

namespace RPA_ULTRA_CORE.Plugins.Runtime
{
    /// <summary>
    /// Manifesto JSON de um plugin
    /// </summary>
    public class PluginManifest
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [JsonProperty("icon")]
        public string Icon { get; set; } = "";

        [JsonProperty("sections")]
        public List<ManifestSection> Sections { get; set; } = new();

        [JsonProperty("plans")]
        public List<string> Plans { get; set; } = new();

        [JsonProperty("permissions")]
        public List<string> Permissions { get; set; } = new();
    }

    public class ManifestSection
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("icon")]
        public string Icon { get; set; } = "";

        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; } = 0;

        [JsonProperty("items")]
        public List<ManifestItem> Items { get; set; } = new();
    }

    public class ManifestItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "ShapeBlueprint";

        [JsonProperty("icon")]
        public string Icon { get; set; } = "";

        [JsonProperty("tags")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        [JsonProperty("defaults")]
        public object? Defaults { get; set; }
    }
}