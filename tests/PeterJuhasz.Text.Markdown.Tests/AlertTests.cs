using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

[TestClass]
public class AlertTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("> [!NOTE]\n> hello world", "<blockquote class=\"NOTE\"><p>hello world</p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> *hello* **world**", "<blockquote class=\"NOTE\"><p><i>hello</i> <b>world</b></p></blockquote>")]

	// every type GitHub knows
	[TranslationDataRow("> [!TIP]\n> hello", "<blockquote class=\"TIP\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!IMPORTANT]\n> hello", "<blockquote class=\"IMPORTANT\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!WARNING]\n> hello", "<blockquote class=\"WARNING\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!CAUTION]\n> hello", "<blockquote class=\"CAUTION\"><p>hello</p></blockquote>")]

	// any other type is accepted too
	[TranslationDataRow("> [!CUSTOM]\n> hello", "<blockquote class=\"CUSTOM\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!note]\n> hello", "<blockquote class=\"note\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!Note]\n> hello", "<blockquote class=\"Note\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!a]\n> hello", "<blockquote class=\"a\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!1]\n> hello", "<blockquote class=\"1\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!<script>]\n> hello", "<blockquote class=\"&lt;script&gt;\"><p>hello</p></blockquote>")]

	// multiple lines
	[TranslationDataRow("> [!NOTE]\n> first\n> second", "<blockquote class=\"NOTE\"><p>first</p><p>second</p></blockquote>")]

	// no body at all
	[TranslationDataRow("> [!NOTE]", "<blockquote class=\"NOTE\"></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n", "<blockquote class=\"NOTE\"></blockquote>")]

	// line endings
	[TranslationDataRow("> [!NOTE]\r\n> hello", "<blockquote class=\"NOTE\"><p>hello</p></blockquote>")]

	// the marker line may have trailing whitespace of its own
	[TranslationDataRow("> [!NOTE]  \n> hello", "<blockquote class=\"NOTE\"><p>hello</p></blockquote>")]

	// indentation
	[TranslationDataRow("  > [!NOTE]\n  > hello", "<blockquote class=\"NOTE\"><p>hello</p></blockquote>")]

	// the body is quoted line by line, just like a block quote
	[TranslationDataRow("> [!NOTE]\n> # heading", "<blockquote class=\"NOTE\"><p># heading</p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> - a\n> - b", "<blockquote class=\"NOTE\"><p>- a</p><p>- b</p></blockquote>")]

	// the marker is the only thing on its line
	[TranslationDataRow("> [!NOTE] hello", "<blockquote><p>[!NOTE] hello</p></blockquote>")]
	[TranslationDataRow("> [!NOTE] \\\n> hello", "<blockquote><p>[!NOTE] \\</p><p>hello</p></blockquote>")]

	// only a single space is allowed between the quote marker and the type
	[TranslationDataRow(">[!NOTE]\n>hello", "<p>&gt;[!NOTE]</p><p>&gt;hello</p>")]
	[TranslationDataRow(">  [!NOTE]\n>  hello", "<blockquote><p>[!NOTE]</p><p>hello</p></blockquote>")]

	// the type is closed, not empty, and a single word
	[TranslationDataRow("> [!NOTE\n> hello", "<blockquote><p>[!NOTE</p><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!]\n> hello", "<blockquote><p>[!]</p><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!NO TE]\n> hello", "<blockquote><p>[!NO TE]</p><p>hello</p></blockquote>")]
	[TranslationDataRow("> [NOTE]\n> hello", "<blockquote><p>[NOTE]</p><p>hello</p></blockquote>")]
	[TranslationDataRow("> !NOTE\n> hello", "<blockquote><p>!NOTE</p><p>hello</p></blockquote>")]

	// the marker only opens an alert at the beginning of a quote
	[TranslationDataRow("> hello\n> [!NOTE]", "<blockquote><p>hello</p><p>[!NOTE]</p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> [!TIP]", "<blockquote class=\"NOTE\"><p>[!TIP]</p></blockquote>")]

	// a lone marker line is not recognized outside a quote
	[TranslationDataRow("[!NOTE]\nhello", "<p>[!NOTE]</p><p>hello</p>")]

	// an unquoted line ends the alert
	[TranslationDataRow("> [!NOTE]\n> hello\nworld", "<blockquote class=\"NOTE\"><p>hello</p></blockquote><p>world</p>")]
	[TranslationDataRow("> [!NOTE]\n>\n> hello", "<blockquote class=\"NOTE\"></blockquote><p>&gt;</p><blockquote><p>hello</p></blockquote>")]

	// surrounding blocks
	[TranslationDataRow("# heading\n> [!NOTE]\n> hello", "<h1>heading</h1><blockquote class=\"NOTE\"><p>hello</p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> hello\n\n# heading", "<blockquote class=\"NOTE\"><p>hello</p></blockquote><h1>heading</h1>")]
	[TranslationDataRow("> [!NOTE]\n> first\n\n> [!TIP]\n> second", "<blockquote class=\"NOTE\"><p>first</p></blockquote><blockquote class=\"TIP\"><p>second</p></blockquote>")]

	// a plain quote is still a plain quote
	[TranslationDataRow("> hello world", "<blockquote><p>hello world</p></blockquote>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("> [!NOTE]\n> hello", "NOTE")]
	[DataRow("> [!TIP]\n> hello", "TIP")]
	[DataRow("> [!IMPORTANT]\n> hello", "IMPORTANT")]
	[DataRow("> [!WARNING]\n> hello", "WARNING")]
	[DataRow("> [!CAUTION]\n> hello", "CAUTION")]
	[DataRow("> [!CUSTOM]\n> hello", "CUSTOM")]
	[DataRow("> [!note]\n> hello", "note")]
	[DataRow("> [!NOTE]\r\n> hello", "NOTE")]
	[DataRow("> [!NOTE]  \n> hello", "NOTE")]

	public void ParseAlertType(string markdown, string expectedType)
	{
		var document = Parser.Parse(markdown);
		var alert = Assert.IsInstanceOfType<AlertNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedType, alert.Type);
	}


	[TestMethod]
	public void ParseAlertContent()
	{
		var document = Parser.Parse("> [!NOTE]\n> first\n> second");

		var alert = Assert.IsInstanceOfType<AlertNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual("NOTE", alert.Type);
		Assert.HasCount(2, alert.Blocks);
		Assert.AreEqual("first", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(alert.Blocks[0]).Runs)).Text);
		Assert.AreEqual("second", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(alert.Blocks[1]).Runs)).Text);
	}


	[TestMethod]
	public void ParseAlertWithoutContent()
	{
		var document = Parser.Parse("> [!NOTE]");

		var alert = Assert.IsInstanceOfType<AlertNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual("NOTE", alert.Type);
		Assert.IsEmpty(alert.Blocks);
	}


	[TestMethod]
	public void ParseAlertFollowedByContent()
	{
		var document = Parser.Parse("> [!NOTE]\n> hello\n\n# heading");

		Assert.AreEqual("NOTE", Assert.IsInstanceOfType<AlertNode>(document.Blocks[0]).Type);
		var heading = Assert.IsInstanceOfType<HeadingNode>(document.Blocks[^1]);
		Assert.AreEqual(1, heading.Level);
		Assert.AreEqual("heading", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(heading.Runs)).Text);
	}


	[TestMethod]

	// the marker is the only thing on its line
	[DataRow("> [!NOTE] hello")]

	// only a single space is allowed between the quote marker and the type
	[DataRow(">[!NOTE]\n>hello")]
	[DataRow(">  [!NOTE]\n>  hello")]

	// the type is closed, not empty, and a single word
	[DataRow("> [!NOTE\n> hello")]
	[DataRow("> [!]\n> hello")]
	[DataRow("> [!NO TE]\n> hello")]
	[DataRow("> [NOTE]\n> hello")]

	// the marker only opens an alert at the beginning of a quote
	[DataRow("> hello\n> [!NOTE]")]

	// not quoted at all
	[DataRow("[!NOTE]\nhello")]

	public void ParseWithoutAlert(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is AlertNode));
	}
}
