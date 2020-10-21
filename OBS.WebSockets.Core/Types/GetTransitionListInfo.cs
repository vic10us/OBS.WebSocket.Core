using System.Collections.Generic;
using Newtonsoft.Json;

namespace OBS.WebSockets.Core.Types
{
    /// <summary>
    /// Response from <see cref="OBSWebsocket.GetTransitionList"/>
    /// </summary>
    public class GetTransitionListInfo
    {
        /// <summary>
        /// Name of the currently active transition
        /// </summary>
        [JsonProperty(PropertyName = "current-transition")]
        public string CurrentTransition { set; get; }

        /// <summary>
        /// List of transitions.
        /// </summary>
        [JsonProperty(PropertyName = "transitions")]
        public List<TransitionSettings> Transitions { set; get; }
    }
}
