namespace BookMyHome.Domain.Exceptions
{
    public sealed class UnauthorizedDomainActionException : DomainException
    {
        public Guid UserId { get; }
        public string Action { get; }
        public Guid ResourceId { get; }

        public UnauthorizedDomainActionException(Guid userId, string action, Guid resourceId)
            : base($"User {userId} is not allowed to {action} (resource {resourceId}.") 
        {
            UserId = userId;
            Action = action;
            ResourceId = resourceId;
        }
    }
}

