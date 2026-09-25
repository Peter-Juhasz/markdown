namespace PeterJuhasz.Text.Markdown.Writer;

public record class MarkdownFormattingOptions(
	string NewLine = "\n"
)
{
	public static readonly MarkdownFormattingOptions Default = new();
}
