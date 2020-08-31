using System;
using System.Text.Json.Serialization;

namespace api.Models
{
    public class Ticket
    {
        public virtual string Id => "ticket";
        private string _content { get; set; }
        public string Content
        {
            get => _content;
            set => _content = $"Ubi_v1 t={value}";
        }
        public DateTime Created { get; set; }
        // Check if ticket was created less than 3 hours ago
        [JsonIgnore]
        public bool Valid => Created > DateTime.Now.AddHours(-3);
    }
}
