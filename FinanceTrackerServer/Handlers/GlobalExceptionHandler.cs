using FinanceTrackerServer.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTrackerServer.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, titile) = exception switch
            {
                IBaseException custom => (custom._stausCode, custom._title),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            httpContext.Response.StatusCode = status;

            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = status,
                    Title = titile,
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path
                }, 
            cancellationToken);

            return true;
        }
    }
}
