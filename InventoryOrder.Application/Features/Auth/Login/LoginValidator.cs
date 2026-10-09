using FluentValidation;

namespace InventoryOrder.Application.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Username).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Password).NotEmpty().MaximumLength(200);
    }
}
