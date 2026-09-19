using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class RegressionTests : TranslationTestBase
{
	[TestMethod]

	// trailing escape character is kept as literal
	[TranslationDataRow("\\", "<p>\\</p>")]
	[TranslationDataRow("a\\", "<p>a\\</p>")]
	[TranslationDataRow("\\*a\\", "<p>*a\\</p>")]
	[TranslationDataRow("a \\* b \\* c\\", "<p>a * b * c\\</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	[DataRow("\\", "\\")]
	[DataRow("a\\", "a\\")]
	[DataRow("\\*a\\", "*a\\")]
	public void ParseTrailingEscape(string markdown, string expectedText)
	{
		var document = Parser.Parse(markdown);
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		var text = string.Concat(paragraph.Runs.Select(r => Assert.IsInstanceOfType<TextNode>(r).Text));
		Assert.AreEqual(expectedText, text);
	}
}
