using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
