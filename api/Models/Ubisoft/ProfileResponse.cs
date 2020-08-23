using System;

namespace api.Models.Ubisoft
{
    public class ProfileResponse
    {
        public Guid ProfileId { get; set; }
        public Guid UserId { get; set; }
        public string PlatformType { get; set; }
        public Guid IdOnPlatform { get; set; }
        public string NameOnPlatform { get; set; }
    }
}
