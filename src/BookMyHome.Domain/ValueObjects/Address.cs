namespace Domain.ValueObjects
{
    public sealed record Address
    {
        public string StreetName { get; }
        public string StreetNumber { get; }
        public string City { get; }
        public string ZipCode { get; }
        public string Country { get; }

        public Address() { }

        public Address(string streetName, string streetNumber, string city, string zipCode, string country)
        {
            if (string.IsNullOrWhiteSpace(streetName))
                throw new ArgumentException(nameof(streetName));

            if (string.IsNullOrWhiteSpace(streetNumber))
                throw new ArgumentException(nameof(streetNumber));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException(nameof(city));

            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException(nameof(zipCode));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException(nameof(country));

            StreetName = streetName;
            StreetNumber = streetNumber;
            City = city;
            ZipCode = zipCode;
            Country = country;
        }

    };
}
