using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class CommentTests : TranslationTestBase
{
	private const int TimeoutMilliseconds = 10_000;

	[TestMethod]

	// basic, a comment is not rendered at all, so the test translator writes it as an element of its own
	[TranslationDataRow("<!-- hello -->", "<comment>hello</comment>")]
	[TranslationDataRow("<!--hello-->", "<comment>hello</comment>")]
	[TranslationDataRow("<!-- hello world -->", "<comment>hello world</comment>")]
	[TranslationDataRow("<!--   hello   -->", "<comment>hello</comment>")]

	// empty
	[TranslationDataRow("<!---->", "<comment></comment>")]
	[TranslationDataRow("<!-- -->", "<comment></comment>")]

	// multiple lines
	[TranslationDataRow("<!-- first\nsecond -->", "<comment>first&#xA;second</comment>")]
	[TranslationDataRow("<!--\nfirst\nsecond\n-->", "<comment>first&#xA;second</comment>")]

	// line endings
	[TranslationDataRow("<!-- hello -->\r\n", "<comment>hello</comment>")]
	[TranslationDataRow("<!-- first\r\nsecond -->", "<comment>first&#xD;&#xA;second</comment>")]

	// indentation
	[TranslationDataRow("  <!-- hello -->", "<comment>hello</comment>")]
	[TranslationDataRow("\t<!-- hello -->", "<comment>hello</comment>")]

	// a comment which is never closed runs to the end of the document
	[TranslationDataRow("<!-- hello", "<comment>hello</comment>")]
	[TranslationDataRow("<!-- hello\nworld", "<comment>hello&#xA;world</comment>")]
	[TranslationDataRow("<!--", "<comment></comment>")]
	[TranslationDataRow("<!-- hello -", "<comment>hello -</comment>")]
	[TranslationDataRow("<!-- hello --", "<comment>hello --</comment>")]

	// contents are not parsed at all
	[TranslationDataRow("<!-- # heading -->", "<comment># heading</comment>")]
	[TranslationDataRow("<!-- *italic* **bold** `code` -->", "<comment>*italic* **bold** `code`</comment>")]
	[TranslationDataRow("<!-- [link](https://example.org) -->", "<comment>[link](https://example.org)</comment>")]
	[TranslationDataRow("<!-- - a\n- b -->", "<comment>- a&#xA;- b</comment>")]
	[TranslationDataRow("<!-- not \\*escaped\\* -->", "<comment>not \\*escaped\\*</comment>")]
	[TranslationDataRow("<!-- </p> -->", "<comment>&lt;/p&gt;</comment>")]
	[TranslationDataRow("<!-- a < b & c -->", "<comment>a &lt; b &amp; c</comment>")]

	// surrounding blocks
	[TranslationDataRow("<!-- hello -->\nworld", "<comment>hello</comment><p>world</p>")]
	[TranslationDataRow("hello\n<!-- note -->\nworld", "<p>hello</p><comment>note</comment><p>world</p>")]
	[TranslationDataRow("hello\n\n<!-- note -->\n\nworld", "<p>hello</p><comment>note</comment><p>world</p>")]
	[TranslationDataRow("# heading\n<!-- note -->", "<h1>heading</h1><comment>note</comment>")]
	[TranslationDataRow("<!-- first\nsecond -->\nworld", "<comment>first&#xA;second</comment><p>world</p>")]
	[TranslationDataRow("<!-- first -->\n<!-- second -->", "<comment>first</comment><comment>second</comment>")]

	// whatever follows the closing delimiter is a block of its own
	[TranslationDataRow("<!-- note --> world", "<comment>note</comment><p>world</p>")]
	[TranslationDataRow("<!-- note --># heading", "<comment>note</comment><h1>heading</h1>")]

	// a comment among the text of a block
	[TranslationDataRow("hello <!-- note -->", "<p>hello <comment>note</comment></p>")]
	[TranslationDataRow("hello <!-- note --> world", "<p>hello <comment>note</comment> world</p>")]
	[TranslationDataRow("hello <!-- note --> *world*", "<p>hello <comment>note</comment> <i>world</i></p>")]
	[TranslationDataRow("*hello* <!-- note -->", "<p><i>hello</i> <comment>note</comment></p>")]
	[TranslationDataRow("hello <!-- first --> world <!-- second -->", "<p>hello <comment>first</comment> world <comment>second</comment></p>")]
	[TranslationDataRow("# heading <!-- note -->", "<h1>heading <comment>note</comment></h1>")]
	[TranslationDataRow("- a <!-- note -->\n- b", "<ul><li>a <comment>note</comment></li><li>b</li></ul>")]
	[TranslationDataRow("1. a <!-- note -->", "<ol><li>a <comment>note</comment></li></ol>")]
	[TranslationDataRow("> quote <!-- note -->", "<blockquote><p>quote <comment>note</comment></p></blockquote>")]
	[TranslationDataRow("> [!NOTE]\n> hello <!-- note -->", "<blockquote class=\"NOTE\"><p>hello <comment>note</comment></p></blockquote>")]
	[TranslationDataRow("[link <!-- note -->](https://example.org)", "<p><a href=\"https://example.org\">link <comment>note</comment></a></p>")]

	// an inline comment which is not closed runs to the end of the text it is written in
	[TranslationDataRow("hello <!-- note", "<p>hello <comment>note</comment></p>")]
	[TranslationDataRow("hello <!--", "<p>hello <comment></comment></p>")]
	[TranslationDataRow("hello <!-- note\nworld", "<p>hello <comment>note</comment></p><p>world</p>")]

	// a comment only opens where its whole marker is written
	[TranslationDataRow("<!", "<p>&lt;!</p>")]
	[TranslationDataRow("<!-", "<p>&lt;!-</p>")]
	[TranslationDataRow("< !-- hello -->", "<p>&lt; !-- hello --&gt;</p>")]
	[TranslationDataRow("<!- - hello -->", "<p>&lt;!- - hello --&gt;</p>")]
	[TranslationDataRow("hello <! world", "<p>hello &lt;! world</p>")]
	[TranslationDataRow("-->", "<p>--&gt;</p>")]
	[TranslationDataRow("hello --> world", "<p>hello --&gt; world</p>")]

	// an escaped marker is a literal one
	[TranslationDataRow("\\<!-- hello -->", "<p>&lt;!-- hello --&gt;</p>")]
	[TranslationDataRow("hello \\<!-- note --> world", "<p>hello &lt;!-- note --&gt; world</p>")]

	// a marker is not recognized where nothing is parsed
	[TranslationDataRow("`<!-- note -->`", "<p><code>&lt;!-- note --&gt;</code></p>")]
	[TranslationDataRow("```\n<!-- note -->\n```", "<code>&lt;!-- note --&gt;</code>")]
	[TranslationDataRow("---\n<!-- note -->\n---", "<frontmatter>&lt;!-- note --&gt;</frontmatter>")]

	// other constructs which are written between angle brackets are untouched
	[TranslationDataRow("<https://example.org>", "<p><a href=\"https://example.org\">https://example.org</a></p>")]
	[TranslationDataRow("hello <https://example.org> world", "<p>hello <a href=\"https://example.org\">https://example.org</a> world</p>")]
	[TranslationDataRow("1 < 2", "<p>1 &lt; 2</p>")]
	[TranslationDataRow("1 < 2 <!-- note -->", "<p>1 &lt; 2 <comment>note</comment></p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("<!-- hello -->", "hello")]
	[DataRow("<!--hello-->", "hello")]
	[DataRow("<!--   hello   -->", "hello")]
	[DataRow("<!-- hello world -->", "hello world")]
	[DataRow("  <!-- hello -->", "hello")]
	[DataRow("<!-- # heading -->", "# heading")]
	[DataRow("<!-- *not italic* -->", "*not italic*")]
	[DataRow("<!-- not \\*escaped\\* -->", "not \\*escaped\\*")]
	[DataRow("<!-- a < b & c -->", "a < b & c")]
	[DataRow("<!-- first\nsecond -->", "first\nsecond")]
	[DataRow("<!--\nfirst\nsecond\n-->", "first\nsecond")]

	// a comment which is never closed carries everything up to where it ends
	[DataRow("<!-- hello", "hello")]
	[DataRow("<!-- hello\nworld", "hello\nworld")]
	[DataRow("<!-- hello --", "hello --")]

	// empty
	[DataRow("<!---->", "")]
	[DataRow("<!-- -->", "")]
	[DataRow("<!--", "")]

	public void ParseComment(string markdown, string expectedComment)
	{
		var document = Parser.Parse(markdown);

		var comment = Assert.IsInstanceOfType<CommentNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedComment, comment.Comment);
	}


	[TestMethod]
	public void ParseCommentFollowedByContent()
	{
		var document = Parser.Parse("<!-- note -->\n# heading");

		Assert.AreEqual("note", Assert.IsInstanceOfType<CommentNode>(document.Blocks[0]).Comment);
		var heading = Assert.IsInstanceOfType<HeadingNode>(document.Blocks[^1]);
		Assert.AreEqual(1, heading.Level);
		Assert.AreEqual("heading", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(heading.Runs)).Text);
	}


	[TestMethod]
	public void ParseCommentBetweenParagraphs()
	{
		var document = Parser.Parse("hello\n<!-- note -->\nworld");

		var blocks = document.Blocks.Where(b => b is not EmptyLineNode).ToList();
		Assert.HasCount(3, blocks);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(blocks[0]).Runs)).Text);
		Assert.AreEqual("note", Assert.IsInstanceOfType<CommentNode>(blocks[1]).Comment);
		Assert.AreEqual("world", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(blocks[2]).Runs)).Text);
	}


	[TestMethod]

	[DataRow("hello <!-- note -->", "note")]
	[DataRow("hello <!--note-->", "note")]
	[DataRow("hello <!-- a < b -->", "a < b")]

	// an inline comment which is not closed carries the rest of the text it is written in
	[DataRow("hello <!-- note", "note")]
	[DataRow("hello <!--", "")]

	public void ParseInlineComment(string markdown, string expectedComment)
	{
		var document = Parser.Parse(markdown);

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, paragraph.Runs);
		Assert.AreEqual("hello ", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual(expectedComment, Assert.IsInstanceOfType<InlineCommentNode>(paragraph.Runs[1]).Comment);
	}


	[TestMethod]
	public void ParseInlineCommentBetweenText()
	{
		var document = Parser.Parse("hello <!-- note --> world");

		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(3, paragraph.Runs);
		Assert.AreEqual("hello ", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual("note", Assert.IsInstanceOfType<InlineCommentNode>(paragraph.Runs[1]).Comment);
		Assert.AreEqual(" world", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[2]).Text);
	}


	[TestMethod]

	// a marker which does not open a comment
	[DataRow("<")]
	[DataRow("<!")]
	[DataRow("<!-")]
	[DataRow("-->")]
	[DataRow("< !-- hello -->")]
	[DataRow("\\<!-- hello -->")]
	[DataRow("hello <! world")]
	[DataRow("<https://example.org>")]
	[DataRow("`<!-- note -->`")]

	public void ParseWithoutComment(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is CommentNode));
	}


	/// <summary>
	/// Every parser loop must consume at least one character per iteration, otherwise it never terminates.
	/// </summary>
	[TestMethod]

	[DataRow("<")]
	[DataRow("<!")]
	[DataRow("<!-")]
	[DataRow("<!--")]
	[DataRow("<!---")]
	[DataRow("<!---->")]
	[DataRow("-->")]
	[DataRow("<!-- -->-->")]
	[DataRow("<<<<")]
	[DataRow("a<")]

	public void ParseTerminates(string markdown) => AssertParseTerminates(markdown);


	[TestMethod]

	[DataRow("<")]
	[DataRow("<!--")]
	[DataRow("<!-- -->")]
	[DataRow("-->")]
	[DataRow("a <!-- b --> ")]

	public void ParseLongRepetitionTerminates(string unit) => AssertParseTerminates(String.Concat(Enumerable.Repeat(unit, 10_000)));

	private static void AssertParseTerminates(string markdown)
	{
		var parse = Task.Run(() => Parser.Parse(markdown));

		Assert.IsTrue(parse.Wait(TimeoutMilliseconds), "Parsing did not terminate.");
		Assert.IsNotNull(parse.Result);
	}
}
