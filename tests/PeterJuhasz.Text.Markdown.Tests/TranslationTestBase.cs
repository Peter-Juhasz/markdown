using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;

namespace PeterJuhasz.Text.Markdown.Tests;

public abstract class TranslationTestBase
{
	protected static void AssertTranslation(string markdown, string expectedHtml)
	{
		var buffer = new ArrayBufferWriter<char>();
		var translator = new TestMarkdownStringTranslator(new HtmlWriter<ArrayBufferWriter<char>>(buffer, HtmlEncoder.Default));
		translator.VisitDocument(markdown);
		var actualHtml = new string(buffer.WrittenSpan);
		Assert.AreEqual(expectedHtml, actualHtml);
	}
}
