using System.Net;
using ACC.Client.Authentication.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Client;

public static class AccApiClientExtensions
{
	public static IServiceCollection AddAccApiClient(this IServiceCollection services, Action<AccApiClientBuilder> configure)
	{
		// Build the provided ITokenService
		var builder = new AccApiClientBuilder();
		configure(builder);

		if (builder.TokenServiceFactory is null)
		{
			throw new InvalidOperationException("You must register an ITokenService.");
		}

		if (builder.TokenCredentialProviderFactory is null)
		{
			throw new InvalidOperationException("You must register an ITokenCredentialProvider");
		}

		services.AddSingleton(provider => builder.TokenServiceFactory(provider));
		services.AddSingleton(provider => builder.TokenCredentialProviderFactory(provider));

		// Configure other needed services
		services.AddSingleton<AuthTokenHandler>();
		services.AddSingleton<RetryPolicyFactory>();

		services.AddHttpClient<IAccApiClient, AccApiClient>()
			.AddHttpMessageHandler<AuthTokenHandler>()
			.AddPolicyHandler((serviceProvider, request) =>
			{
				var factory = serviceProvider.GetRequiredService<RetryPolicyFactory>();
				return factory.CreatePolicy();
			});

		return services;
	}
}
