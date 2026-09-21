using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Markdown.Benchmarks.Visitors;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Benchmarks;

using MarkdigMarkdown = Markdig.Markdown;

/// <summary>
/// Runs every benchmarked code path once and prints what it produced, so that a benchmark which
/// visits nothing, or which sees a different document than the others, is easy to spot.
/// </summary>
internal static class SampleDocumentReport
{
	public static void Print()
	{
		var markdown = SampleDocument.Read();
		Console.WriteLine($"{SampleDocument.FileName}: {markdown.Length:N0} characters");
		Console.WriteLine();

		var document = Parser.Parse(markdown);
		var markdigDocument = MarkdigMarkdown.Parse(markdown, MarkdigPipeline.Default);
		Console.WriteLine($"Markdig blocks:                           {markdigDocument.Count:N0}");
		Console.WriteLine($"PeterJuhasz.Text.Markdown blocks:         {document.Blocks.Length:N0}");
		Console.WriteLine();

		var highLevel = new NoopDocumentObjectModelVisitor();
		highLevel.VisitDocument(document);

		var lowLevel = new NoopInplaceMarkdownVisitor();
		lowLevel.VisitDocument(markdown);

		Console.WriteLine($"Markdig nodes walked:                     {new NoopMarkdigWalker().Walk(markdigDocument):N0}");
		Console.WriteLine($"PeterJuhasz.Text.Markdown nodes (high):   {highLevel.Count:N0}");
		Console.WriteLine($"PeterJuhasz.Text.Markdown nodes (low):    {lowLevel.Count:N0}");
		Console.WriteLine();

		var buffer = new ArrayBufferWriter<char>();
		var translator = new HtmlStringMarkdownTranslator<ArrayBufferWriter<char>>(new HtmlWriter<ArrayBufferWriter<char>>(buffer, HtmlEncoder.Default));
		translator.VisitDocument(markdown);
		Console.WriteLine($"Markdig HTML:                             {MarkdigMarkdown.ToHtml(markdown, MarkdigPipeline.Default).Length:N0} characters");
		Console.WriteLine($"PeterJuhasz.Text.Markdown HTML:           {buffer.WrittenCount:N0} characters");
	}
}
