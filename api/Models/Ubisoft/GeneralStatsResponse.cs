using System.Text.Json.Serialization;

namespace api.Models.Ubisoft
{
    public class GeneralStatsResponse
    {
        [JsonPropertyName("generalpvp_timeplayed:infinite")]
        public int TimePlayed { get; set; }
    }
}
