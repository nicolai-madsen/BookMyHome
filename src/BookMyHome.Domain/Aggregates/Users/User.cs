using BookMyHome.Domain.Exceptions;
using BookMyHome.Domain.ValueObjects;

namespace BookMyHome.Domain.Aggregates.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public FullName FullName { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTimeOffset? HostAcceptedTermsAt { get; private set; }
        public bool IsHost => HostAcceptedTermsAt is not null;

        private User() { }
        public User(Guid id, FullName fullName, Email email, PhoneNumber phoneNumber, string username, string passwordHash)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty;", nameof(id));

            ArgumentNullException.ThrowIfNull(fullName);
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(phoneNumber);

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));

            Id = id;
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            Username = username;
            PasswordHash = passwordHash;
        }

        public void BecomeHost(DateTimeOffset hostAcceptedTermsAt)
        {
            if (IsHost)
                throw new UserAlreadyRegisteredAsHostException(Id);

            HostAcceptedTermsAt = hostAcceptedTermsAt;
        }
    }
}