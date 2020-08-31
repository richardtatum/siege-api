using System.Collections.Generic;

namespace api.Models.Ubisoft
{
    internal static class Platforms
    {
        internal static readonly Dictionary<string, string> Urls = new Dictionary<string, string>
        {
            {"pc", "5172a557-50b5-4665-b7db-e3f2e8c5041d/sandboxes/OSBOR_PC_LNCH_A" },
            {"xbox", "98a601e5-ca91-4440-b1c5-753f601a2c90/sandboxes/OSBOR_XBOXONE_LNCH_A" },
            {"ps4", "05bfb3f7-6c21-4c42-be1f-97a33fb5cf66/sandboxes/OSBOR_PS4_LNCH_A" },
        };
    }

    public class UbisoftConfig
    {
        public string AppId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // URLS
        public string BaseUrl { get; set; }
        public string ChallengeUrl { get; set; }
        public string PlatformUrl(string platform)
        {
            return $"v1/spaces/{Platforms.Urls[platform]}/";
        }
        public string AvatarUrl(string playerId)
        {
            return $"https://ubisoft-avatars.akamaized.net/{playerId}/default_256_256.png";
        }
    }
}
