using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class LinkTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("[a](b)", """<p><a href="b">a</a></p>""")]
	[TranslationDataRow("[link](https://example.org)", """<p><a href="https://example.org">link</a></p>""")]
	[TranslationDataRow("this is [link](https://example.org) snippet", """<p>this is <a href="https://example.org">link</a> snippet</p>""")]
	[TranslationDataRow("[a](b) and [c](d)", """<p><a href="b">a</a> and <a href="d">c</a></p>""")]

	// text is parsed as inline content
	[TranslationDataRow("[**bold** and *italic*](b)", """<p><a href="b"><b>bold</b> and <i>italic</i></a></p>""")]
	[TranslationDataRow("[`code`](b)", """<p><a href="b"><code>code</code></a></p>""")]
	[TranslationDataRow("[<b>](b)", """<p><a href="b">&lt;b&gt;</a></p>""")]

	// links are not nested into each other
	[TranslationDataRow("[https://example.org](b)", """<p><a href="b">https://example.org</a></p>""")]
	[TranslationDataRow("[@user #tag](b)", """<p><a href="b">@user #tag</a></p>""")]

	// empty
	[TranslationDataRow("[](b)", """<p><a href="b"></a></p>""")]

	// escaped
	[TranslationDataRow(@"\[a](b)", "<p>[a](b)</p>")]
	[TranslationDataRow(@"[a\]b](c)", """<p><a href="c">a]b</a></p>""")]

	// not a link
	[TranslationDataRow("[a]", "<p>[a]</p>")]
	[TranslationDataRow("[a] (b)", "<p>[a] (b)</p>")]
	[TranslationDataRow("[a](b", "<p>[a](b</p>")]
	[TranslationDataRow("a](b)", "<p>a](b)</p>")]

	// nested in other blocks
	[TranslationDataRow("# [a](b)", """<h1><a href="b">a</a></h1>""")]
	[TranslationDataRow("- [a](b)", """<ul><li><a href="b">a</a></li></ul>""")]
	[TranslationDataRow("1. [a](b)", """<ol><li><a href="b">a</a></li></ol>""")]
	[TranslationDataRow("> [a](b)", """<blockquote><p><a href="b">a</a></p></blockquote>""")]
	[TranslationDataRow("| [a](b) |", """<table><tr><td><a href="b">a</a></td></tr></table>""")]
	[TranslationDataRow("**[a](b)**", """<p><b><a href="b">a</a></b></p>""")]

	// not parsed in code
	[TranslationDataRow("`[a](b)`", "<p><code>[a](b)</code></p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// basic
	[TranslationDataRow("""[a](b "t")""", """<p><a href="b" title="t">a</a></p>""")]
	[TranslationDataRow("""[a](https://example.org "Example site")""", """<p><a href="https://example.org" title="Example site">a</a></p>""")]

	// single quoted
	[TranslationDataRow("[a](b 't')", """<p><a href="b" title="t">a</a></p>""")]
	[TranslationDataRow("[a](b 'two words')", """<p><a href="b" title="two words">a</a></p>""")]

	// whitespace around the title
	[TranslationDataRow("""[a](b   "t")""", """<p><a href="b" title="t">a</a></p>""")]
	[TranslationDataRow("[a](b\t\"t\")", """<p><a href="b" title="t">a</a></p>""")]
	[TranslationDataRow("""[a](b "t" )""", """<p><a href="b" title="t">a</a></p>""")]

	// empty title
	[TranslationDataRow("""[a](b "")""", """<p><a href="b">a</a></p>""")]

	// the title may contain the delimiters of the link
	[TranslationDataRow("""[a](b "x) y")""", """<p><a href="b" title="x) y">a</a></p>""")]
	[TranslationDataRow("""[a](b "x (y)")""", """<p><a href="b" title="x (y)">a</a></p>""")]
	[TranslationDataRow("""[a](b "[x]")""", """<p><a href="b" title="[x]">a</a></p>""")]
	[TranslationDataRow("""[a](b "it's")""", """<p><a href="b" title="it&#x27;s">a</a></p>""")]

	// escaped
	[TranslationDataRow("""[a](b "x \" y")""", """<p><a href="b" title="x &quot; y">a</a></p>""")]

	// encoding
	[TranslationDataRow("""[a](b "<x> & y")""", """<p><a href="b" title="&lt;x&gt; &amp; y">a</a></p>""")]

	// the title is not parsed as inline content
	[TranslationDataRow("""[a](b "*x* `y` @user")""", """<p><a href="b" title="*x* `y` @user">a</a></p>""")]

	// text is still parsed as inline content
	[TranslationDataRow("""[**x** y](b "t")""", """<p><a href="b" title="t"><b>x</b> y</a></p>""")]

	// not closed
	[TranslationDataRow("""[a](b "t""", """<p>[a](b &quot;t</p>""")]

	// nested in other blocks
	[TranslationDataRow("""# [a](b "t")""", """<h1><a href="b" title="t">a</a></h1>""")]
	[TranslationDataRow("""- [a](b "t")""", """<ul><li><a href="b" title="t">a</a></li></ul>""")]
	[TranslationDataRow("""> [a](b "t")""", """<blockquote><p><a href="b" title="t">a</a></p></blockquote>""")]
	[TranslationDataRow("""| [a](b "t") |""", """<table><tr><td><a href="b" title="t">a</a></td></tr></table>""")]

	// not parsed in code
	[TranslationDataRow("""`[a](b "t")`""", "<p><code>[a](b &quot;t&quot;)</code></p>")]

	public void TranslateTitle(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// no title
	[DataRow("[a](b)", "b", (string?)null)]
	[DataRow("[a](https://example.org)", "https://example.org", (string?)null)]
	[DataRow("[a]()", "", (string?)null)]

	// title
	[DataRow("""[a](b "t")""", "b", "t")]
	[DataRow("[a](b 't')", "b", "t")]
	[DataRow("""[a](b   "t"  )""", "b", "t")]
	[DataRow("""[a](b "two words")""", "b", "two words")]
	[DataRow("""[a](b "x) y")""", "b", "x) y")]
	[DataRow("""[a](b "x (y)")""", "b", "x (y)")]

	// an empty title is the same as no title
	[DataRow("""[a](b "")""", "b", (string?)null)]
	[DataRow("[a](b '')", "b", (string?)null)]

	// escapes are decoded
	[DataRow("""[a](b "x \" y")""", "b", "x \" y")]
	[DataRow("""[a](b "x \\ y")""", "b", "x \\ y")]

	// quotes must match, and the title must be the last thing in the target
	[DataRow("""[a](b "t')""", "b \"t'", (string?)null)]
	[DataRow("""[a](b "t" x)""", "b \"t\" x", (string?)null)]
	[DataRow("[a](b t)", "b t", (string?)null)]

	// a quote which does not follow whitespace is a part of the URL
	[DataRow("""[a](b"t")""", "b\"t\"", (string?)null)]
	[DataRow("[a](b'c)", "b'c", (string?)null)]

	public void ParseTarget(string markdown, string expectedUrl, string? expectedTitle)
	{
		var link = ParseSingleLink(markdown);
		Assert.AreEqual(expectedUrl, link.Url);
		Assert.AreEqual(expectedTitle, link.Title);
	}


	[TestMethod]

	[DataRow("[a](b)", "a")]
	[DataRow("""[a](b "t")""", "a")]
	[DataRow(@"[a\]b](c)", "a]b")]
	[DataRow("""[a "t"](b)""", "a \"t\"")]

	public void ParseText(string markdown, string expectedText)
	{
		var link = ParseSingleLink(markdown);
		var text = Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(link.Runs));
		Assert.AreEqual(expectedText, text.Text);
	}


	[TestMethod]
	public void ParseFormattedTextWithTitle()
	{
		var link = ParseSingleLink("""[**x** y](b "t")""");
		Assert.AreEqual("b", link.Url);
		Assert.AreEqual("t", link.Title);
		Assert.HasCount(2, link.Runs);
		Assert.IsInstanceOfType<BoldNode>(link.Runs[0]);
		Assert.AreEqual(" y", Assert.IsInstanceOfType<TextNode>(link.Runs[1]).Text);
	}


	private static LinkNode ParseSingleLink(string markdown)
	{
		var document = Parser.Parse(markdown);
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		return Assert.IsInstanceOfType<LinkNode>(Assert.ContainsSingle(paragraph.Runs));
	}
}
