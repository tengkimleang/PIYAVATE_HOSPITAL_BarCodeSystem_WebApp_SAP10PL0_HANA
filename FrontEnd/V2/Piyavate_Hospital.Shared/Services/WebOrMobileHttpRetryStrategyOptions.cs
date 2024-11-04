using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Piyavate_Hospital.Shared.Services;

public class WebOrMobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
{
    public WebOrMobileHttpRetryStrategyOptions()
    {
        BackoffType = DelayBackoffType.Exponential;
        MaxRetryAttempts = 5;
        UseJitter = true;
        Delay = TimeSpan.FromSeconds(1.5);
    }
}
