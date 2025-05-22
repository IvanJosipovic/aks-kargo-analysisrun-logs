using FluentValidation;

namespace AKS.Kargo.AnalysisRun.Logs;

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.Authentication).SetValidator(new AuthenticationValidator());

        RuleFor(x => x.Shards).NotEmpty();
        RuleFor(x => x.Shards).Custom((shards, context) =>
        {
            if (shards != null && shards.GroupBy(e => e.Name).Any(g => g.Count() > 1))
            {
                context.AddFailure("Shards", "Shard names must be unique.");
            }
        });
        RuleForEach(x => x.Shards).SetValidator(new ShardValidator());
    }
}

public class AuthenticationValidator : AbstractValidator<Authentication>
{
    public AuthenticationValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().When(x => !string.IsNullOrEmpty(x.ClientId) || !string.IsNullOrEmpty(x.ClientSecret));
        RuleFor(x => x.ClientId).NotEmpty().When(x => !string.IsNullOrEmpty(x.TenantId) || !string.IsNullOrEmpty(x.ClientSecret));
        RuleFor(x => x.ClientSecret).NotEmpty().When(x => !string.IsNullOrEmpty(x.TenantId) || !string.IsNullOrEmpty(x.ClientId));
    }
}

public class ShardValidator : AbstractValidator<Shard>
{
    public ShardValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.AzureMonitorWorkspaceId).NotEmpty().Must(x => Guid.TryParse(x, out var result)).WithMessage("Must be Guid");
    }
}
