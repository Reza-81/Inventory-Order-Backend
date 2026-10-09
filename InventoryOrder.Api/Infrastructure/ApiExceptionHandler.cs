using FluentValidation;
using InventoryOrder.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace InventoryOrder.Api.Infrastructure;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            ArgumentException or InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        await Results.Problem(
            statusCode: statusCode,
            title: statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception is DbUpdateConcurrencyException
                    ? "The resource was changed by another request."
                    : exception.Message,
            detail: statusCode == StatusCodes.Status500InternalServerError ? null : exception.Message)
            .ExecuteAsync(httpContext);

        return true;
    }
}
