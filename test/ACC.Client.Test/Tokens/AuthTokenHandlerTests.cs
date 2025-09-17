using System.Net;
using ACC.Client.Authentication.Tokens;
using NSubstitute;
using RichardSzalay.MockHttp;

namespace ACC.Client.Test.Tokens;

public sealed class AuthTokenHandlerTests //(ITestOutputHelper outputHelper)
{
	[Fact]
	public async Task HttpRequest_ShouldContainToken_Always()
	{
		// Arrange
		var tokenService = Substitute.For<ITokenService>();
		tokenService
			.GetNextBearerTokenAsync(Arg.Any<CancellationToken>())
			.Returns("testToken");

		// Set up mock HTTP client
		var mockHttp = new MockHttpMessageHandler();
		mockHttp
			.When(HttpMethod.Get, "*")
			.WithHeaders("Bearer", "testToken")
			.Respond(HttpStatusCode.OK);

		var tokenHandler = new AuthTokenHandler(tokenService)
		{
			InnerHandler = mockHttp
		};

		var client = new HttpClient(tokenHandler);

		// Act
		await client.GetAsync("https://localhost/", TestContext.Current.CancellationToken);

		// Assert
		await tokenService
			.Received(1)
			.GetNextBearerTokenAsync(Arg.Any<CancellationToken>());
	}
}
