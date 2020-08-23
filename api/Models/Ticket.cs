﻿using System;

namespace api.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        private string _value { get; set; }

        public string Value
        {
            get => _value;
            set => _value = $"Ubi_v1 t={value}";
        }
        public DateTime Created { get; set; }
        public bool Valid => Created < DateTime.Now - TimeSpan.FromHours(2);
    }
}