using Spectre.Console;
using Spectre.Console.Rendering;

namespace ACC.Backup.Cli.Internal;

public sealed class ValueColumn : ProgressColumn
{
	public Style Style { get; set; } = Style.Plain;

	public Style CompletedStyle { get; set; } = Color.Green;

	public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
	{
		var current = (int)task.Value;
		var total = (int)task.MaxValue;

		var style = current == total ? CompletedStyle : Style ?? Style.Plain;
		return new Text($"{current}/{total}", style).RightJustified();
	}
}