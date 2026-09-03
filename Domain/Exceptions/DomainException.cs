namespace Domain.Exceptions
{
    public abstract class DomainException : Exception 
        //Abstract because we don't want to create a general DomainException, but specific exceptions. Only inheritate, not instantiate   
    {
        protected DomainException(string message) //Sends error message to the Exception class. The Exception class is a built in feature in EF Core.
            : base(message) //Only "child"-classes can call the constructor.
        {
        }
    }
}
