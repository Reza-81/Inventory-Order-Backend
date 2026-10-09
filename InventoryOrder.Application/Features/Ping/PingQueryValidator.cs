using FluentValidation;

namespace InventoryOrder.Application.Features.Ping;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        RuleFor(query => query.Message)
            .NotEmpty()
            .MaximumLength(100);
    }
}
