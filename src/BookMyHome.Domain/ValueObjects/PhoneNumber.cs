using System.Text.RegularExpressions;

namespace BookMyHome.Domain.ValueObjects
{
    public sealed partial record PhoneNumber
    {
        public string Value { get; }

        [GeneratedRegex(@"^\+?[1-9]\d{1,14}$")]
        private static partial Regex E164Regex();

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));

            if (!E164Regex().IsMatch(value))
                throw new ArgumentException("Invalid phone number format.", nameof(value));

            Value = value;
        }
    }
}
