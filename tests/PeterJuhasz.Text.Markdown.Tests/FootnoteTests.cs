using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class FootnoteTests : TranslationTestBase
{
	private const int TimeoutMilliseconds = 10_000;

	[TestMethod]

	// basic, a reference is written among the text it belongs to
	[TranslationDataRow("hello[^1]", "<p>hello<footnoteref>1</footnoteref></p>")]
	[TranslationDataRow("[^1]", "<p><footnoteref>1</footnoteref></p>")]
	[TranslationDataRow("hello[^1] world", "<p>hello<footnoteref>1</footnoteref> world</p>")]
	[TranslationDataRow("hello [^1] world", "<p>hello <footnoteref>1</footnoteref> world</p>")]

	// the number is written with as many digits as it takes
	[TranslationDataRow("hello[^12]", "<p>hello<footnoteref>12</footnoteref></p>")]
	[TranslationDataRow("hello[^0]", "<p>hello<footnoteref>0</footnoteref></p>")]
	[TranslationDataRow("hello[^007]", "<p>hello<footnoteref>7</footnoteref></p>")]
	[TranslationDataRow("hello[^2147483647]", "<p>hello<footnoteref>2147483647</footnoteref></p>")]

	// a number which an Int32 does not hold is not a reference at all
	[TranslationDataRow("hello[^2147483648]", "<p>hello[^2147483648]</p>")]
	[TranslationDataRow("hello[^99999999999]", "<p>hello[^99999999999]</p>")]

	// several references
	[TranslationDataRow("a[^1] b[^2]", "<p>a<footnoteref>1</footnoteref> b<footnoteref>2</footnoteref></p>")]
	[TranslationDataRow("a[^1][^2]", "<p>a<footnoteref>1</footnoteref><footnoteref>2</footnoteref></p>")]

	// the same number may be referenced any number of times
	[TranslationDataRow("a[^1] b[^1]", "<p>a<footnoteref>1</footnoteref> b<footnoteref>1</footnoteref></p>")]

	// a reference is written wherever inline content is
	[TranslationDataRow("# heading[^1]", "<h1>heading<footnoteref>1</footnoteref></h1>")]
	[TranslationDataRow("*hello[^1]*", "<p><i>hello<footnoteref>1</footnoteref></i></p>")]
	[TranslationDataRow("**hello[^1]**", "<p><b>hello<footnoteref>1</footnoteref></b></p>")]
	[TranslationDataRow("- a[^1]\n- b[^2]", "<ul><li>a<footnoteref>1</footnoteref></li><li>b<footnoteref>2</footnoteref></li></ul>")]
	[TranslationDataRow("1. a[^1]", "<ol><li>a<footnoteref>1</footnoteref></li></ol>")]
	[TranslationDataRow("> quote[^1]", "<blockquote><p>quote<footnoteref>1</footnoteref></p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> hello[^1]", "<blockquote class=\"NOTE\"><p>hello<footnoteref>1</footnoteref></p></blockquote>")]
	[TranslationDataRow("| a[^1] |", "<table><tr><td>a<footnoteref>1</footnoteref></td></tr></table>")]

	// only a number between the markers is a reference
	[TranslationDataRow("hello[^]", "<p>hello[^]</p>")]
	[TranslationDataRow("hello[^a]", "<p>hello[^a]</p>")]
	[TranslationDataRow("hello[^1a]", "<p>hello[^1a]</p>")]
	[TranslationDataRow("hello[^a1]", "<p>hello[^a1]</p>")]
	[TranslationDataRow("hello[^-1]", "<p>hello[^-1]</p>")]
	[TranslationDataRow("hello[^1.5]", "<p>hello[^1.5]</p>")]

	// the number is written without any whitespace of its own
	[TranslationDataRow("hello[^ 1]", "<p>hello[^ 1]</p>")]
	[TranslationDataRow("hello[^1 ]", "<p>hello[^1 ]</p>")]
	[TranslationDataRow("hello[^1 2]", "<p>hello[^1 2]</p>")]
	[TranslationDataRow("hello[ ^1]", "<p>hello[ ^1]</p>")]

	// a reference only opens where its whole marker is written
	[TranslationDataRow("hello[1]", "<p>hello[1]</p>")]
	[TranslationDataRow("hello^1", "<p>hello^1</p>")]
	[TranslationDataRow("hello[^1", "<p>hello[^1</p>")]
	[TranslationDataRow("hello[^", "<p>hello[^</p>")]
	[TranslationDataRow("hello^1]", "<p>hello^1]</p>")]

	// an escaped marker is a literal one
	[TranslationDataRow("hello\\[^1]", "<p>hello[^1]</p>")]

	// a marker is not recognized where nothing is parsed
	[TranslationDataRow("`[^1]`", "<p><code>[^1]</code></p>")]
	[TranslationDataRow("```\n[^1]\n```", "<code>[^1]</code>")]
	[TranslationDataRow("---\n[^1]\n---", "<frontmatter>[^1]</frontmatter>")]
	[TranslationDataRow("<!-- [^1] -->", "<comment>[^1]</comment>")]

	// a link is still a link
	[TranslationDataRow("[^1](https://example.org)", "<p><a href=\"https://example.org\">^1</a></p>")]

	public void TranslateReference(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// basic, the content declares the number it belongs to
	[TranslationDataRow("[^1]: hello", "<footnote id=\"1\">hello</footnote>")]
	[TranslationDataRow("[^1]:hello", "<footnote id=\"1\">hello</footnote>")]
	[TranslationDataRow("[^1]:   hello   ", "<footnote id=\"1\">hello</footnote>")]
	[TranslationDataRow("[^12]: hello", "<footnote id=\"12\">hello</footnote>")]
	[TranslationDataRow("[^007]: hello", "<footnote id=\"7\">hello</footnote>")]

	// content of its own is optional
	[TranslationDataRow("[^1]:", "<footnote id=\"1\"></footnote>")]
	[TranslationDataRow("[^1]:   ", "<footnote id=\"1\"></footnote>")]

	// indentation
	[TranslationDataRow("  [^1]: hello", "<footnote id=\"1\">hello</footnote>")]
	[TranslationDataRow("\t[^1]: hello", "<footnote id=\"1\">hello</footnote>")]

	// line endings
	[TranslationDataRow("[^1]: hello\r\n", "<footnote id=\"1\">hello</footnote>")]

	// the content is parsed as inline content, just like a paragraph is
	[TranslationDataRow("[^1]: *hello*", "<footnote id=\"1\"><i>hello</i></footnote>")]
	[TranslationDataRow("[^1]: **hello**", "<footnote id=\"1\"><b>hello</b></footnote>")]
	[TranslationDataRow("[^1]: `code`", "<footnote id=\"1\"><code>code</code></footnote>")]
	[TranslationDataRow("[^1]: [link](https://example.org)", "<footnote id=\"1\"><a href=\"https://example.org\">link</a></footnote>")]
	[TranslationDataRow("[^1]: a < b", "<footnote id=\"1\">a &lt; b</footnote>")]

	// a footnote may point at another one
	[TranslationDataRow("[^1]: see[^2]", "<footnote id=\"1\">see<footnoteref>2</footnoteref></footnote>")]

	// the content is written on the single line the number is declared on
	[TranslationDataRow("[^1]: hello\nworld", "<footnote id=\"1\">hello</footnote><p>world</p>")]

	// several footnotes, which need not be written in any order, nor be referenced at all
	[TranslationDataRow("[^1]: a\n[^2]: b", "<footnote id=\"1\">a</footnote><footnote id=\"2\">b</footnote>")]
	[TranslationDataRow("[^2]: b\n[^1]: a", "<footnote id=\"2\">b</footnote><footnote id=\"1\">a</footnote>")]
	[TranslationDataRow("[^1]: a\n[^1]: b", "<footnote id=\"1\">a</footnote><footnote id=\"1\">b</footnote>")]

	// surrounding blocks
	[TranslationDataRow("hello[^1]\n\n[^1]: world", "<p>hello<footnoteref>1</footnoteref></p><footnote id=\"1\">world</footnote>")]
	[TranslationDataRow("# heading\n[^1]: hello", "<h1>heading</h1><footnote id=\"1\">hello</footnote>")]
	[TranslationDataRow("[^1]: hello\n# heading", "<footnote id=\"1\">hello</footnote><h1>heading</h1>")]

	// the separator follows the marker the number is closed with
	[TranslationDataRow("[^1] : hello", "<p><footnoteref>1</footnoteref> : hello</p>")]
	[TranslationDataRow("[^1]; hello", "<p><footnoteref>1</footnoteref>; hello</p>")]
	[TranslationDataRow("[^1] hello", "<p><footnoteref>1</footnoteref> hello</p>")]

	// only a number declares content
	[TranslationDataRow("[^a]: hello", "<p>[^a]: hello</p>")]
	[TranslationDataRow("[^]: hello", "<p>[^]: hello</p>")]
	[TranslationDataRow("[1]: hello", "<p>[1]: hello</p>")]
	[TranslationDataRow("[^2147483648]: hello", "<p>[^2147483648]: hello</p>")]

	// a block quote carries paragraphs, so a marker written in one is a reference
	[TranslationDataRow("> [^1]: hello", "<blockquote><p><footnoteref>1</footnoteref>: hello</p></blockquote>")]

	public void TranslateContent(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	/// <summary>
	/// The default rendering ties a reference and the content written for it together by the number alone,
	/// which is all a footnote carries.
	/// </summary>
	[TestMethod]

	[DataRow("hello[^1]", "<p>hello<sup><a href=\"#footnote-1\">1</a></sup></p>")]
	[DataRow("hello[^12]", "<p>hello<sup><a href=\"#footnote-12\">12</a></sup></p>")]
	[DataRow("hello[^2147483647]", "<p>hello<sup><a href=\"#footnote-2147483647\">2147483647</a></sup></p>")]
	[DataRow("[^1]: world", "<p id=\"footnote-1\"><sup>1</sup> world</p>")]
	[DataRow("[^12]: *world*", "<p id=\"footnote-12\"><sup>12</sup> <i>world</i></p>")]
	[DataRow("hello[^1]\n[^1]: world", "<p>hello<sup><a href=\"#footnote-1\">1</a></sup></p><p id=\"footnote-1\"><sup>1</sup> world</p>")]

	public void TranslateToHtml(string markdown, string expectedHtml)
	{
		var buffer = new ArrayBufferWriter<char>();
		var translator = new HtmlStringMarkdownTranslator<ArrayBufferWriter<char>>(new HtmlWriter<ArrayBufferWriter<char>>(buffer, HtmlEncoder.Default));
		translator.VisitDocument(markdown);

		Assert.AreEqual(expectedHtml, new string(buffer.WrittenSpan));
	}


	[TestMethod]

	[DataRow("hello[^1]", 1)]
	[DataRow("hello[^12]", 12)]
	[DataRow("hello[^0]", 0)]
	[DataRow("hello[^007]", 7)]
	[DataRow("hello[^2147483647]", 2147483647)]

	public void ParseReference(string markdown, int expectedNumber)
	{
		var document = Parser.Parse(markdown);

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, paragraph.Runs);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual(expectedNumber, Assert.IsInstanceOfType<FootnoteReferenceNode>(paragraph.Runs[1]).Number);
	}


	[TestMethod]
	public void ParseReferenceBetweenText()
	{
		var document = Parser.Parse("hello[^1] world");

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(3, paragraph.Runs);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual(1, Assert.IsInstanceOfType<FootnoteReferenceNode>(paragraph.Runs[1]).Number);
		Assert.AreEqual(" world", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[2]).Text);
	}


	[TestMethod]
	public void ParseSeveralReferences()
	{
		var document = Parser.Parse("a[^1] b[^2]");

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		var references = paragraph.Runs.OfType<FootnoteReferenceNode>().ToList();
		Assert.HasCount(2, references);
		Assert.AreEqual(1, references[0].Number);
		Assert.AreEqual(2, references[1].Number);
	}


	[TestMethod]

	[DataRow("[^1]: hello", 1, "hello")]
	[DataRow("[^1]:hello", 1, "hello")]
	[DataRow("[^1]:   hello   ", 1, "hello")]
	[DataRow("[^12]: hello world", 12, "hello world")]
	[DataRow("[^007]: hello", 7, "hello")]
	[DataRow("  [^1]: hello", 1, "hello")]
	[DataRow("[^2147483647]: hello", 2147483647, "hello")]

	public void ParseContent(string markdown, int expectedNumber, string expectedText)
	{
		var document = Parser.Parse(markdown);

		var footnote = Assert.IsInstanceOfType<FootnoteContentNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedNumber, footnote.Number);
		Assert.AreEqual(expectedText, Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(footnote.Runs)).Text);
	}


	[TestMethod]

	[DataRow("[^1]:")]
	[DataRow("[^1]:   ")]

	public void ParseContentWithoutText(string markdown)
	{
		var document = Parser.Parse(markdown);

		var footnote = Assert.IsInstanceOfType<FootnoteContentNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(1, footnote.Number);
		Assert.IsEmpty(footnote.Runs);
	}


	[TestMethod]
	public void ParseContentWithInlineContent()
	{
		var document = Parser.Parse("[^1]: see *this* and[^2]");

		var footnote = Assert.IsInstanceOfType<FootnoteContentNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(1, footnote.Number);
		Assert.HasCount(4, footnote.Runs);
		Assert.AreEqual("see ", Assert.IsInstanceOfType<TextNode>(footnote.Runs[0]).Text);
		Assert.AreEqual("this", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ItalicNode>(footnote.Runs[1]).Runs)).Text);
		Assert.AreEqual(" and", Assert.IsInstanceOfType<TextNode>(footnote.Runs[2]).Text);
		Assert.AreEqual(2, Assert.IsInstanceOfType<FootnoteReferenceNode>(footnote.Runs[3]).Number);
	}


	/// <summary>
	/// Nothing ties the content of a footnote to the references which point at it, so a number
	/// may be written any number of times, or no time at all.
	/// </summary>
	[TestMethod]
	public void ParseReferenceAndContentIndependently()
	{
		var document = Parser.Parse("hello[^1] world[^3]\n\n[^1]: first\n[^2]: second");

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(document.Blocks[0]);
		var references = paragraph.Runs.OfType<FootnoteReferenceNode>().Select(r => r.Number).ToList();
		Assert.AreSequenceEqual([1, 3], references);

		var footnotes = document.Blocks.OfType<FootnoteContentNode>().Select(f => f.Number).ToList();
		Assert.AreSequenceEqual([1, 2], footnotes);
	}


	[TestMethod]

	// only a number between the markers is a reference
	[DataRow("hello[^]")]
	[DataRow("hello[^a]")]
	[DataRow("hello[^1a]")]
	[DataRow("hello[^-1]")]

	// the number is written without any whitespace of its own
	[DataRow("hello[^ 1]")]
	[DataRow("hello[^1 ]")]
	[DataRow("hello[ ^1]")]

	// a number which an Int32 does not hold
	[DataRow("hello[^2147483648]")]
	[DataRow("hello[^99999999999]")]

	// a marker which is not written whole
	[DataRow("hello[1]")]
	[DataRow("hello^1")]
	[DataRow("hello[^1")]
	[DataRow("hello[^")]

	// an escaped marker is a literal one
	[DataRow("hello\\[^1]")]

	// a marker is not recognized where nothing is parsed
	[DataRow("`[^1]`")]

	public void ParseWithoutReference(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsEmpty(document.Blocks.OfType<ParagraphNode>().SelectMany(p => p.Runs).OfType<FootnoteReferenceNode>());
	}


	[TestMethod]

	[DataRow("[^a]: hello")]
	[DataRow("[^]: hello")]
	[DataRow("[1]: hello")]
	[DataRow("[^2147483648]: hello")]
	[DataRow("[^1] : hello")]
	[DataRow("[^1]; hello")]
	[DataRow("[^1] hello")]
	[DataRow("hello [^1]: world")]
	[DataRow("`[^1]: hello`")]

	public void ParseWithoutContent(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsEmpty(document.Blocks.OfType<FootnoteContentNode>());
	}


	/// <summary>
	/// Every parser loop must consume at least one character per iteration, otherwise it never terminates.
	/// </summary>
	[TestMethod]

	[DataRow("[")]
	[DataRow("[^")]
	[DataRow("[^]")]
	[DataRow("[^1")]
	[DataRow("[^1]")]
	[DataRow("[^1]:")]
	[DataRow("[^]:")]
	[DataRow("^1]")]
	[DataRow("[[[[")]
	[DataRow("[^^^^")]
	[DataRow("[^1][^1][^1]")]
	[DataRow("a[")]

	public void ParseTerminates(string markdown) => AssertParseTerminates(markdown);


	[TestMethod]

	[DataRow("[^1]")]
	[DataRow("[^1]:")]
	[DataRow("[^1]: a\n")]
	[DataRow("a[^1] ")]
	[DataRow("[^")]

	public void ParseLongRepetitionTerminates(string unit) => AssertParseTerminates(String.Concat(Enumerable.Repeat(unit, 10_000)));

	private static void AssertParseTerminates(string markdown)
	{
		var parse = Task.Run(() => Parser.Parse(markdown));

		Assert.IsTrue(parse.Wait(TimeoutMilliseconds), "Parsing did not terminate.");
		Assert.IsNotNull(parse.Result);
	}
}
