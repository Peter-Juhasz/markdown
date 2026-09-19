using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class BlockQuoteTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("> hello world", "<blockquote><p>hello world</p></blockquote>")]
	[TranslationDataRow("> *hello* **world**", "<blockquote><p><i>hello</i> <b>world</b></p></blockquote>")]

	// unclosed
	[TranslationDataRow(">", "<p>&gt;</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);
}
