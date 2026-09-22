using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

[TestClass]
public class FigureTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("^^^\nhello world\n^^^ caption", "<figure><p>hello world</p><figcaption>caption</figcaption></figure>")]
	[TranslationDataRow("^^^\n\nhello world\n\n^^^ caption", "<figure><p>hello world</p><figcaption>caption</figcaption></figure>")]

	// the caption is optional
	[TranslationDataRow("^^^\nhello world\n^^^", "<figure><p>hello world</p></figure>")]
	[TranslationDataRow("^^^\nhello world\n^^^   ", "<figure><p>hello world</p></figure>")]

	// empty
	[TranslationDataRow("^^^\n^^^", "<figure></figure>")]
	[TranslationDataRow("^^^\n\n^^^", "<figure></figure>")]
	[TranslationDataRow("^^^\n^^^ caption", "<figure><figcaption>caption</figcaption></figure>")]

	// the caption carries inline content
	[TranslationDataRow("^^^\nx\n^^^ *hello* **world**", "<figure><p>x</p><figcaption><i>hello</i> <b>world</b></figcaption></figure>")]
	[TranslationDataRow("^^^\nx\n^^^ [link](https://example.org)", "<figure><p>x</p><figcaption><a href=\"https://example.org\">link</a></figcaption></figure>")]
	[TranslationDataRow("^^^\nx\n^^^ a `code`", "<figure><p>x</p><figcaption>a <code>code</code></figcaption></figure>")]
	[TranslationDataRow("^^^\nx\n^^^ <script>", "<figure><p>x</p><figcaption>&lt;script&gt;</figcaption></figure>")]

	// the caption is trimmed, and needs no whitespace after the fence
	[TranslationDataRow("^^^\nx\n^^^   caption   ", "<figure><p>x</p><figcaption>caption</figcaption></figure>")]
	[TranslationDataRow("^^^\nx\n^^^caption", "<figure><p>x</p><figcaption>caption</figcaption></figure>")]

	// the content carries blocks
	[TranslationDataRow("^^^\n# heading\n^^^", "<figure><h1>heading</h1></figure>")]
	[TranslationDataRow("^^^\n- a\n- b\n^^^", "<figure><ul><li>a</li><li>b</li></ul></figure>")]
	[TranslationDataRow("^^^\n> quoted\n^^^", "<figure><blockquote><p>quoted</p></blockquote></figure>")]
	[TranslationDataRow("^^^\nfirst\n\nsecond\n^^^", "<figure><p>first</p><p>second</p></figure>")]
	[TranslationDataRow("^^^\n| a | b |\n^^^", "<figure><table><tr><td>a</td><td>b</td></tr></table></figure>")]

	// line endings
	[TranslationDataRow("^^^\r\nhello\r\n^^^ caption\r\n", "<figure><p>hello</p><figcaption>caption</figcaption></figure>")]

	// indentation of the opening fence
	[TranslationDataRow("  ^^^\nhello\n^^^", "<figure><p>hello</p></figure>")]
	[TranslationDataRow("\t^^^\nhello\n^^^", "<figure><p>hello</p></figure>")]

	// only whitespace may follow the opening fence
	[TranslationDataRow("^^^   \nhello\n^^^", "<figure><p>hello</p></figure>")]
	[TranslationDataRow("^^^ caption\nhello\n^^^", "<p>^^^ caption</p><p>hello</p><p>^^^</p>")]

	// only three carets make a fence
	[TranslationDataRow("^^^^\nhello\n^^^^", "<p>^^^^</p><p>hello</p><p>^^^^</p>")]
	[TranslationDataRow("^^\nhello\n^^", "<p>^^</p><p>hello</p><p>^^</p>")]
	[TranslationDataRow("^^^\nhello\n^^^^\n^^^", "<figure><p>hello</p><p>^^^^</p></figure>")]

	// a figure which is never closed is not recognized at all
	[TranslationDataRow("^^^", "<p>^^^</p>")]
	[TranslationDataRow("^^^\nhello", "<p>^^^</p><p>hello</p>")]
	[TranslationDataRow("^^^\nhello\n^^^^", "<p>^^^</p><p>hello</p><p>^^^^</p>")]

	// the closing fence starts a line
	[TranslationDataRow("^^^\nhello ^^^\n^^^", "<figure><p>hello ^^^</p></figure>")]

	// the figure is closed by the first fence, so a figure written inside another one is not one of its own
	[TranslationDataRow("^^^\n^^^\nhello\n^^^\n^^^", "<figure></figure><p>hello</p><figure></figure>")]

	// whatever follows the closing line is a block of its own
	[TranslationDataRow("^^^\nhello\n^^^ caption\n# heading", "<figure><p>hello</p><figcaption>caption</figcaption></figure><h1>heading</h1>")]
	[TranslationDataRow("^^^\nhello\n^^^\n\nworld", "<figure><p>hello</p></figure><p>world</p>")]

	// surrounding blocks
	[TranslationDataRow("# heading\n^^^\nhello\n^^^", "<h1>heading</h1><figure><p>hello</p></figure>")]
	[TranslationDataRow("^^^\nfirst\n^^^ a\n^^^\nsecond\n^^^ b", "<figure><p>first</p><figcaption>a</figcaption></figure><figure><p>second</p><figcaption>b</figcaption></figure>")]

	// a fence written in the middle of a line is nothing but text
	[TranslationDataRow("hello ^^^\nworld\n^^^", "<p>hello ^^^</p><p>world</p><p>^^^</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	public void ParseContentAndCaption()
	{
		var document = Parser.Parse("^^^\n\nfirst\n\nsecond\n\n^^^ caption");

		var figure = Assert.IsInstanceOfType<FigureBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(3, figure.Blocks);
		Assert.AreEqual("first", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(figure.Blocks[0]).Runs)).Text);
		Assert.IsInstanceOfType<EmptyLineNode>(figure.Blocks[1]);
		Assert.AreEqual("second", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(figure.Blocks[2]).Runs)).Text);
		Assert.AreEqual("caption", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(figure.Caption)).Text);
	}


	[TestMethod]
	public void ParseCaptionWithInlineContent()
	{
		var document = Parser.Parse("^^^\nx\n^^^ *hello* world");

		var figure = Assert.IsInstanceOfType<FigureBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, figure.Caption);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ItalicNode>(figure.Caption[0]).Runs)).Text);
		Assert.AreEqual(" world", Assert.IsInstanceOfType<TextNode>(figure.Caption[1]).Text);
	}


	[TestMethod]
	public void ParseWithoutCaption()
	{
		var document = Parser.Parse("^^^\nhello\n^^^");

		var figure = Assert.IsInstanceOfType<FigureBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.IsEmpty(figure.Caption);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(figure.Blocks)).Runs)).Text);
	}


	[TestMethod]
	public void ParseWithoutContent()
	{
		var document = Parser.Parse("^^^\n^^^ caption");

		var figure = Assert.IsInstanceOfType<FigureBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.IsEmpty(figure.Blocks);
		Assert.AreEqual("caption", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(figure.Caption)).Text);
	}


	[TestMethod]
	public void ParseFollowedByContent()
	{
		var document = Parser.Parse("^^^\nhello\n^^^ caption\n\n# heading");

		Assert.IsInstanceOfType<FigureBlockNode>(document.Blocks[0]);
		var heading = Assert.IsInstanceOfType<HeadingNode>(document.Blocks[^1]);
		Assert.AreEqual(1, heading.Level);
		Assert.AreEqual("heading", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(heading.Runs)).Text);
	}


	[TestMethod]
	public void VisitContentAndCaption()
	{
		var document = Parser.Parse("^^^\nhello\n^^^ caption");

		var visitor = new TextCollectingVisitor();
		visitor.VisitDocument(document);

		// the content is visited before the caption, just like it is written
		Assert.AreEqual("hello|caption", String.Join('|', visitor.Texts));
	}

	private sealed class TextCollectingVisitor : DocumentObjectModelVisitor
	{
		public List<string> Texts { get; } = [];

		protected override void Visit(TextNode node) => Texts.Add(node.Text);
	}


	[TestMethod]

	// never closed
	[DataRow("^^^")]
	[DataRow("^^^\nhello")]
	[DataRow("^^^\nhello\n^^^^")]

	// only three carets make a fence
	[DataRow("^^\nhello\n^^")]
	[DataRow("^^^^\nhello\n^^^^")]

	// only whitespace may follow the opening fence
	[DataRow("^^^ caption\nhello\n^^^")]

	// not at the beginning of a line
	[DataRow("hello ^^^\nworld\n^^^")]

	public void ParseWithoutFigure(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is FigureBlockNode));
	}
}
