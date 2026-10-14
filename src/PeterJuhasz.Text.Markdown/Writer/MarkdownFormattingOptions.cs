namespace PeterJuhasz.Text.Markdown.Writer;

/// <param name="NewLine">The line ending every line is written with.</param>
/// <param name="ListIndentation">
/// The indentation a list item is written with for each list it is nested into. It is four spaces by default, which reaches
/// the content of any item marked with a number of up to two digits, so that parsers which nest by the content of an item
/// rather than by its marker read it as nested too.
/// </param>
public record class MarkdownFormattingOptions(
	string NewLine = "\n",
	string ListIndentation = "    "
)
{
	public static readonly MarkdownFormattingOptions Default = new();
}
