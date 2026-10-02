namespace Domain.ValueObjects
{
    public sealed record Email
    {
        public string? EmailAddress { get; }
        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.')) //Email must contain a valid value
                throw new ArgumentException("Invalid email address");

            EmailAddress = email.ToLowerInvariant();
        }
        public override string ToString() => EmailAddress;
    }
}
