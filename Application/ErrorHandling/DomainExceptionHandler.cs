using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Application.ErrorHandling
{
    /// <summary>
    /// This class solves an issue where failed object initializations can throw incorrect/unprecise exceptions.
    /// 
    /// </summary>
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
                OverlappingBookingException => (StatusCodes.Status409Conflict, "Booking conflict"),
                BookingNotFoundException => (StatusCodes.Status404NotFound, "Booking not found"),
                BookingNotActiveException => (StatusCodes.Status409Conflict, "Booking conflict"),
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
