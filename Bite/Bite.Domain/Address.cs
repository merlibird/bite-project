using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Domain
{
    public class Address(
        int id,
        string street,
        string number,
        string zipCode,
        string city,
        string country,
        string? additionalInfo,
        double longitude,
        double latitude)
    {
        public int Id { get; init; } = id;
        public string Street { get; init; } = street;
        public string Number { get; init; } = number;
        public string ZipCode { get; init; } = zipCode;
        public string City { get; init; } = city;
        public string Country { get; init; } = country;
        public string? AdditionalInfo { get; set; } = additionalInfo;
        public double Longitude { get; init; } = longitude;
        public double Latitude { get; init; } = latitude;

        public override string ToString()
        {
            return $"Address {Id}: {Street} {Number}, {ZipCode} {City}, {Country} (Longitude: {Longitude}, Latitude: {Latitude})";
        }
    }
}
