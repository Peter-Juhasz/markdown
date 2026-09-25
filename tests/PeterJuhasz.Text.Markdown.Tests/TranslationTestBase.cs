using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;

namespace PeterJuhasz.Text.Markdown.Tests;

public abstract class TranslationTestBase
{
	private static readonly HtmlWriterFormattingOptions formatter = HtmlWriterFormattingOptions.Indented with
	{
		NewLine = null,
		Indent = null,
	};

	protected static void AssertTranslation(string markdown, string expectedHtml)
	{
		using var _ = ArrayBufferWriterPool<char>.GetPooledObject(out var buffer);
		var translator = new TestMarkdownStringTranslator(new HtmlWriter<ArrayBufferWriter<char>>(buffer, HtmlEncoder.Default, formatter));
		translator.VisitDocument(markdown);
		var actualHtml = new string(buffer.WrittenSpan);
		Assert.AreEqual(expectedHtml, actualHtml);
	}
}
