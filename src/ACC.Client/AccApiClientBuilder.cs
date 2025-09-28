using ACC.Client.Authentication.Credentials;
using ACC.Client.Authentication.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace ACC.Client;

public sealed class AccApiClientBuilder
{
	internal Func<IServiceProvider, ITokenService>? TokenServiceFactory { get; private set; }
	internal Func<IServiceProvider, ICredentialProvider>? TokenCredentialProviderFactory { get; private set; }

	public AccApiClientBuilder WithTokenService<T>() where T : class, ITokenService
	{
		TokenServiceFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public AccApiClientBuilder WithTokenService(ITokenService tokenService)
	{
		TokenServiceFactory = _ => tokenService;
		return this;
	}

	public AccApiClientBuilder WithTokenService(Func<IServiceProvider, ITokenService> factory)
	{
		TokenServiceFactory = factory;
		return this;
	}

	public AccApiClientBuilder WithTokenCredentialProvider<T>() where T : class, ICredentialProvider
	{
		TokenCredentialProviderFactory = serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider);
		return this;
	}

	public AccApiClientBuilder WithTokenCredentialProvider(ICredentialProvider credentialProvider)
	{
		TokenCredentialProviderFactory = _ => credentialProvider;
		return this;
	}

	public AccApiClientBuilder WithTokenCredentialProvider(Func<IServiceProvider, ICredentialProvider> factory)
	{
		TokenCredentialProviderFactory = factory;
		return this;
	}
}
