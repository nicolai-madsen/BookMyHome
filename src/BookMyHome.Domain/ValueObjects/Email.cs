namespace BookMyHome.Domain.ValueObjects
{
    public sealed record Email
    {
        public string? EmailAddress{ get; }

        private Email() { }
        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.')) // Basic validation for email format
                throw new ArgumentException("Invalid email address");

            EmailAddress = email.ToLowerInvariant();
        }
        public override string ToString() => EmailAddress ?? string.Empty;
    }
}
