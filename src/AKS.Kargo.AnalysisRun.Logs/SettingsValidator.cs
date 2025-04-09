using FluentValidation;

namespace AKS.Kargo.AnalysisRun.Logs;

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        //RuleFor(x => x.Authentication).NotNull();
        RuleFor(x => x.Authentication).SetValidator(new AuthenticationValidator());

        RuleFor(x => x.Environments).NotEmpty();
        RuleFor(x => x.Environments).Custom((environments, context) =>
        {
            if (environments != null && environments.GroupBy(e => e.Name).Any(g => g.Count() > 1))
            {
                context.AddFailure("Environments", "Environment names must be unique.");
            }
        });
        RuleForEach(x => x.Environments).SetValidator(new EnvironmentsValidator());
    }
}

public class AuthenticationValidator : AbstractValidator<Authentication>
{
    public AuthenticationValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().When(x => x.ClientId != null || x.ClientSecret != null);
        RuleFor(x => x.ClientId).NotEmpty().When(x => x.TenantId != null || x.ClientSecret != null);
        RuleFor(x => x.ClientSecret).NotEmpty().When(x => x.TenantId != null || x.ClientId != null);
    }
}

public class EnvironmentsValidator : AbstractValidator<Environment>
{
    public EnvironmentsValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.AzureMonitorWorkspaceId).NotEmpty().Must(x => Guid.TryParse(x, out var result)).WithMessage("Must be Guid");
    }
}
