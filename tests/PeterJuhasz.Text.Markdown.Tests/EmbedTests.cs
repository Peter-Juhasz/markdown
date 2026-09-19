using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class EmbedTests : TranslationTestBase
{
	[TestMethod]

	// embed without scheme falls back to paragraph
	[TranslationDataRow("!()", "<p>!()</p>")]
	[TranslationDataRow("!(abc)", "<p>!(abc)</p>")]
	[TranslationDataRow("\n!()", "<p>!()</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	[DataRow("!()")]
	[DataRow("\n!()")]
	public void ParseEmbedWithoutScheme(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsInstanceOfType<ParagraphNode>(document.Blocks[^1]);
	}
}
