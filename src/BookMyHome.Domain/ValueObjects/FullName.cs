namespace Domain.ValueObjects
{
    public record FullName
    {
        public string FirstName { get; }
        public string LastName { get; }

        private FullName() { }

        public FullName(string firstName, string lastName)
        {
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentException(nameof(firstName));

            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentException(nameof(lastName));

            FirstName = firstName;
            LastName = lastName;
        }
    }
}
