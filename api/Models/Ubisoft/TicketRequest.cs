using System;
using System.Text;

namespace api.Models.Ubisoft
{
    public class TicketRequest
    {
        // obtain from config file
        private string Email { get; set; }
        private string Password { get; set; }
        public string AppId { get; set; }

        public string Key
        {
            get
            {
                var encoded = Base64Encode($"{Email}:{Password}");
                return $"Basic{encoded}";
            }
        }

        private string Base64Encode(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }
    }
}
