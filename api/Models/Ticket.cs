using System;

namespace api.Models
{
    public class Ticket
    {
        public virtual string Id => "ticket";
        private string _value { get; set; }
        public string Value
        {
            get => _value;
            set => _value = $"Ubi_v1 t={value}";
        }
        public DateTime Created { get; set; }
        // Check if ticket was created less than 3 hours ago
        public bool Valid => Created > DateTime.Now.AddHours(-3);
    }
}
