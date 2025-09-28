using ACC.Backup.Core.Reporting;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACC.Backup.Core.Test.Reporting;

public sealed class BasicReportingServiceTests(ITestOutputHelper outputHelper)
{
	[Fact]
	public void GenerateReport_ShouldReturnString_Always()
	{
		// Arrange
		// Set up DI
		using var services = new ServiceCollection()
			.AddLogging(builder =>
			{
				builder.AddXUnit(outputHelper);
				builder.SetMinimumLevel(LogLevel.Trace);
			})
			.AddSingleton<IReportingService, BasicReportingService>()
			.BuildServiceProvider();
		var reportingService = services.GetRequiredService<IReportingService>();

		reportingService.AddHub("hub1", "Test Hub 1");
		reportingService.AddProject("proj1", "Test Project 1", "hub1");
		reportingService.AddFile("file1", "Test File 1", "proj1", ReportingState.Successful);
		reportingService.AddFile("file2", "Test File 2", "proj1", ReportingState.Failed);
		reportingService.AddFile("file3", "Test File 3", "proj1", ReportingState.UpToDate);

		reportingService.AddHub("hub2", "Test Hub 2", isExcluded: true);

		reportingService.AddHub("hub3", "Test Hub 3");
		reportingService.AddProject("proj3", "Test Project 3", "hub3", isExcluded: true);

		reportingService.AddMessage("This is a test message.");

		// Act
		var html = reportingService.GenerateReport();

		// Assert
		var htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(html);

		var tables = htmlDocument.DocumentNode.SelectNodes("//table");
		Assert.NotNull(tables);
		Assert.Equal(4, tables.Count);

		var summaryCells = tables[0].SelectNodes(".//tr[2]/td");
		Assert.Equal("3", summaryCells[0].InnerText);
		Assert.Equal("1", summaryCells[1].InnerText);
		Assert.Equal("1", summaryCells[2].InnerText);
		Assert.Equal("1", summaryCells[3].InnerText);

		var projectCells = tables[1].SelectNodes(".//tr[2]/td");
		Assert.Equal("Test Project 1", projectCells[0].InnerText);
		Assert.Equal("Test Hub 1", projectCells[1].InnerText);
		Assert.Equal("3", projectCells[2].InnerText);
		Assert.Equal("1", projectCells[3].InnerText);
		Assert.Equal("1", projectCells[4].InnerText);
		Assert.Equal("1", projectCells[5].InnerText);

		var exclusionCells = tables[2].SelectNodes(".//tr[2]/td");
		Assert.Equal("hub2", exclusionCells[0].InnerText);
		Assert.Equal("Hub", exclusionCells[1].InnerText);
		Assert.Equal("Test Hub 2", exclusionCells[2].InnerText);

		var messageCell = tables[3].SelectSingleNode(".//tr[2]/td");
		Assert.Equal("This is a test message.", messageCell.InnerText);
	}
}
