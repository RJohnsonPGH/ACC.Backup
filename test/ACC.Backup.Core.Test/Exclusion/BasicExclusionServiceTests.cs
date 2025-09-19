using ACC.Backup.Core.Exclusion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ACC.Backup.Core.Test.Exclusion;

public sealed class BasicExclusionServiceTests(ITestOutputHelper outputHelper)
{
	[Fact]
	public void ShouldExcludeItem_ShouldReturnFalse_WhenNotInExclusionList()
	{
		// Arrange
		var testExcludedId = "excluded-id";

		var exclusionProvider = Substitute.For<IExclusionProvider>();
		exclusionProvider.IncludedIds.Returns([]);
		exclusionProvider.ExcludedIds.Returns([]);

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(exclusionProvider)
			.AddSingleton<IExclusionService, BasicExclusionService>()
			.BuildServiceProvider();
		var exclusionService = services.GetRequiredService<IExclusionService>();

		// Act
		var excluded = exclusionService.ShouldExcludeItem(testExcludedId);

		// Assert
		Assert.False(excluded);
	}

	[Fact]
	public void ShouldExcludeItem_ShouldReturnFalse_WhenInInclusionListAndNotInExclusionList()
	{
		// Arrange
		var testExcludedId = "excluded-id";

		var exclusionProvider = Substitute.For<IExclusionProvider>();
		exclusionProvider.IncludedIds.Returns([testExcludedId]);
		exclusionProvider.ExcludedIds.Returns([]);

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(exclusionProvider)
			.AddSingleton<IExclusionService, BasicExclusionService>()
			.BuildServiceProvider();
		var exclusionService = services.GetRequiredService<IExclusionService>();

		// Act
		var excluded = exclusionService.ShouldExcludeItem(testExcludedId);

		// Assert
		Assert.False(excluded);
	}

	[Fact]
	public void ShouldExcludeItem_ShouldReturnTrue_WhenInExclusionList()
	{
		// Arrange
		var testExcludedId = "excluded-id";

		var exclusionProvider = Substitute.For<IExclusionProvider>();
		exclusionProvider.IncludedIds.Returns([]);
		exclusionProvider.ExcludedIds.Returns([testExcludedId]);

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(exclusionProvider)
			.AddSingleton<IExclusionService, BasicExclusionService>()
			.BuildServiceProvider();
		var exclusionService = services.GetRequiredService<IExclusionService>();

		// Act
		var excluded = exclusionService.ShouldExcludeItem(testExcludedId);

		// Assert
		Assert.True(excluded);
	}

	[Fact]
	public void ShouldExcludeItem_ShouldReturnTrue_WhenInInclusionListAndInExclusionList()
	{
		// Arrange
		var testExcludedId = "excluded-id";

		var exclusionProvider = Substitute.For<IExclusionProvider>();
		exclusionProvider.IncludedIds.Returns([testExcludedId]);
		exclusionProvider.ExcludedIds.Returns([testExcludedId]);

		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton(exclusionProvider)
			.AddSingleton<IExclusionService, BasicExclusionService>()
			.BuildServiceProvider();
		var exclusionService = services.GetRequiredService<IExclusionService>();

		// Act
		var excluded = exclusionService.ShouldExcludeItem(testExcludedId);

		// Assert
		Assert.True(excluded);
	}
}
