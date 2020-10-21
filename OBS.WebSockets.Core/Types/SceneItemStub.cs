using Newtonsoft.Json;

namespace OBS.WebSockets.Core.Types
{
    /// <summary>
    /// Stub for scene item that only contains the name or ID of an item
    /// </summary>
    public class SceneItemStub
    {
        /// <summary>
        /// Source name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string SourceName;

        /// <summary>
        /// Scene item ID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int ID { set; get; }
    }
}
