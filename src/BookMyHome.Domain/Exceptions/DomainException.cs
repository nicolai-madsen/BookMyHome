namespace Domain.Exceptions
{
    public abstract class DomainException : Exception 
        //Abstract because we don't want to create a general DomainException, but specific exceptions. Only inheritate, not instantiate   
    {
        protected DomainException(string message) //Sends error message to the Exception class.
            : base(message) //Only "child"-classes can call the constructor.
        {
        }
    }
}
