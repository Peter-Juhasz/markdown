using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

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
