using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using System.Text;
using System.Text.Json;
using ACC.Client.Authentication.Tokens;
using ACC.Client.RestApiResponses;
using ACC.Client.Authentication.Credentials;
using NSubstitute;

namespace ACC.Client.Test.Tokens;

public sealed class TokenServiceTests(ITestOutputHelper outputHelper)
{
	// Secret
	private readonly KeyValuePair<string, string>[] _credentials = [
		new("testClientId", "testClientSecret"),
		new("testClientId2", "testClientSecret2")
	];
	private static string GetBase64ClientIdAndSecretBytes(ICredential credential) => 
		Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credential.Id}:{credential.Secret}"));

	// Request
	private readonly Uri _requestUri = new("https://developer.api.autodesk.com/authentication/v2/token");

	private readonly Dictionary<string, string> _defaultRequestBody = new()
	{
		{ "grant_type", "client_credentials" },
		{ "scope", "data:read" }
	};
	
	// Response
	private readonly AuthenticateResponse _successfulAuthenticationResponse = new()
	{
		BearerToken = "testToken",
		ExpiresIn = 3600,
	};

	private readonly AuthenticateResponse _expiringAuthenticationResponse = new()
	{
		BearerToken = "testToken",
		ExpiresIn = 1,
	};

	[Fact]
	public async Task GetBearerTokenAsync_ShouldReturnToken_WhenSuccessful()
	{
		// Arrange
		// Set up credentials
		var credential = Substitute.For<ICredential>();
		credential.Id.Returns(_credentials[0].Key);
		credential.Secret.Returns(_credentials[0].Value);

		var credentialProvider = Substitute.For<ICredentialProvider>();
		credentialProvider
			.GetEnumerator()
			.Returns(_ => new List<ICredential> { credential }.GetEnumerator());

		// Set up mock HTTP client
		var mockHttp = new MockHttpMessageHandler();
		mockHttp
			.When(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token")
			.WithHeaders("Authorization", $"Basic {GetBase64ClientIdAndSecretBytes(credential)}")
			.WithExactFormData(_defaultRequestBody)
			.Respond("application/json", JsonSerializer.Serialize(_successfulAuthenticationResponse));
		var httpClient = mockHttp.ToHttpClient();

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(credentialProvider)
			.AddSingleton<ITokenService, TokenService>()
			.AddSingleton(httpClient)
			.BuildServiceProvider();
		var tokenService = services.GetRequiredService<ITokenService>();

		// Act
		var token = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal("testToken", token);
	}

	[Fact]
	public async Task GetBearerTokenAsync_ShouldReturnOriginalToken_WhenNotExpired()
	{
		// Arrange
		// Set up credentials
		var credential = Substitute.For<ICredential>();
		credential.Id.Returns(_credentials[0].Key);
		credential.Secret.Returns(_credentials[0].Value);

		var credentialProvider = Substitute.For<ICredentialProvider>();
		credentialProvider
			.GetEnumerator()
			.Returns(_ => new List<ICredential> { credential }.GetEnumerator());

		// Set up mock HTTP client
		var mockHttp = new MockHttpMessageHandler();
		var request = mockHttp
			.When(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token")
			.WithHeaders("Authorization", $"Basic {GetBase64ClientIdAndSecretBytes(credential)}")
			.WithExactFormData(_defaultRequestBody)
			.Respond("application/json", JsonSerializer.Serialize(_successfulAuthenticationResponse));
		var httpClient = mockHttp.ToHttpClient();

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(credentialProvider)
			.AddSingleton<ITokenService, TokenService>()
			.AddSingleton(httpClient)
			.BuildServiceProvider();
		var tokenService = services.GetRequiredService<ITokenService>();

		// Act
		var token = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);
		await Task.Delay(1000, TestContext.Current.CancellationToken);
		var secondToken = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(token, secondToken);
		Assert.Equal(1, mockHttp.GetMatchCount(request));
	}

	[Fact]
	public async Task GetBearerTokenAsync_ShouldRefreshToken_WhenExpired()
	{
		// Arrange
		// Set up credentials
		var credential = Substitute.For<ICredential>();
		credential.Id.Returns(_credentials[0].Key);
		credential.Secret.Returns(_credentials[0].Value);

		var credentialProvider = Substitute.For<ICredentialProvider>();
		credentialProvider
			.GetEnumerator()
			.Returns(_ => new List<ICredential> { credential }.GetEnumerator());

		// Set up mock HTTP client
		var mockHttp = new MockHttpMessageHandler();
		var request = mockHttp
			.When(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token")
			.WithHeaders("Authorization", $"Basic {GetBase64ClientIdAndSecretBytes(credential)}")
			.WithExactFormData(_defaultRequestBody)
			.Respond("application/json", JsonSerializer.Serialize(_expiringAuthenticationResponse));
		var httpClient = mockHttp.ToHttpClient();

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(credentialProvider)
			.AddSingleton<ITokenService, TokenService>()
			.AddSingleton(httpClient)
			.BuildServiceProvider();
		var tokenService = services.GetRequiredService<ITokenService>();

		// Act
		var token = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);
		await Task.Delay(5000, TestContext.Current.CancellationToken);
		var secondToken = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(token, secondToken);
		Assert.Equal(2, mockHttp.GetMatchCount(request));
	}

	[Fact]
	public async Task GetBearerTokenAsync_ShouldRotateToken_EveryRequest()
	{
		// Arrange
		// Set up credentials
		var credential = Substitute.For<ICredential>();
		credential.Id.Returns(_credentials[0].Key);
		credential.Secret.Returns(_credentials[0].Value);
		var secondCredential = Substitute.For<ICredential>();
		secondCredential.Id.Returns(_credentials[1].Key);
		secondCredential.Secret.Returns(_credentials[1].Value);

		var credentialProvider = Substitute.For<ICredentialProvider>();
		credentialProvider
			.GetEnumerator()
			.Returns(_ => new List<ICredential> { credential, secondCredential }.GetEnumerator());

		// Set up mock HTTP client
		var mockHttp = new MockHttpMessageHandler();
		var request = mockHttp
			.When(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token")
			.WithHeaders("Authorization", $"Basic {GetBase64ClientIdAndSecretBytes(credential)}")
			.WithExactFormData(_defaultRequestBody)
			.Respond("application/json", JsonSerializer.Serialize(_successfulAuthenticationResponse));
		var secondRequest = mockHttp
			.When(HttpMethod.Post, "https://developer.api.autodesk.com/authentication/v2/token")
			.WithHeaders("Authorization", $"Basic {GetBase64ClientIdAndSecretBytes(secondCredential)}")
			.WithExactFormData(_defaultRequestBody)
			.Respond("application/json", JsonSerializer.Serialize(_successfulAuthenticationResponse));
		var httpClient = mockHttp.ToHttpClient();

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(credentialProvider)
			.AddSingleton<ITokenService, TokenService>()
			.AddSingleton(httpClient)
			.BuildServiceProvider();
		var tokenService = services.GetRequiredService<ITokenService>();

		// Act
		var token = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);
		var secondToken = await tokenService.GetNextBearerTokenAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(token, secondToken);
		Assert.Equal(1, mockHttp.GetMatchCount(request));
		Assert.Equal(1, mockHttp.GetMatchCount(secondRequest));
	}
}
