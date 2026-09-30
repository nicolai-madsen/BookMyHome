using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Application.ErrorHandling
{
    public sealed class DomainExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService problemDetailsService;
        public DomainExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            this.problemDetailsService = problemDetailsService;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                UnauthorizedDomainActionException => (StatusCodes.Status403Forbidden, "Forbidden"),
                HostCannotBookOwnAccommodationException => (StatusCodes.Status403Forbidden, "Forbidden"),
                OverlappingBookingException => (StatusCodes.Status409Conflict, "Booking conflict"),
                BookingNotActiveException => (StatusCodes.Status409Conflict, "Booking conflict"),
                ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrency conflict"),
                BookingNotFoundException => (StatusCodes.Status404NotFound, "Booking not found"),
                BookingStartDateCannotBeInThePastException => (StatusCodes.Status400BadRequest, "Invalid booking"),
                BookingStartAndEndDateCannotBeEqualException => (StatusCodes.Status400BadRequest, "Invalid booking"),
                EndDateIsBeforeStartDateException => (StatusCodes.Status400BadRequest, "Invalid booking"),

                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
                DomainException => (StatusCodes.Status400BadRequest, "Invalid request"),

                _ => (0, "")
            };

            if (status == 0) // Unknown exceptions becomes 500 
                return false;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = problem
            });
        }
    }
}
