using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class DetailsTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("<details>\n<summary>title</summary>\n\nhello world\n\n</details>", "<details><summary>title</summary><p>hello world</p></details>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\nhello world\n</details>", "<details><summary>title</summary><p>hello world</p></details>")]

	// written on a single line
	[TranslationDataRow("<details><summary>title</summary>hello world</details>", "<details><summary>title</summary><p>hello world</p></details>")]

	// the summary is optional
	[TranslationDataRow("<details>\nhello world\n</details>", "<details><p>hello world</p></details>")]
	[TranslationDataRow("<details>hello world</details>", "<details><p>hello world</p></details>")]

	// empty
	[TranslationDataRow("<details></details>", "<details></details>")]
	[TranslationDataRow("<details>\n</details>", "<details></details>")]
	[TranslationDataRow("<details>\n\n</details>", "<details></details>")]
	[TranslationDataRow("<details><summary>title</summary></details>", "<details><summary>title</summary></details>")]
	[TranslationDataRow("<details><summary></summary>hello</details>", "<details><summary></summary><p>hello</p></details>")]

	// the summary carries inline content
	[TranslationDataRow("<details><summary>*hello* **world**</summary>x</details>", "<details><summary><i>hello</i> <b>world</b></summary><p>x</p></details>")]
	[TranslationDataRow("<details><summary>[link](https://example.org)</summary>x</details>", "<details><summary><a href=\"https://example.org\">link</a></summary><p>x</p></details>")]
	// inline code, like a mention or an emoji alias, still needs whitespace before it, which the tag it follows is not
	[TranslationDataRow("<details><summary>a `code`</summary>x</details>", "<details><summary>a <code>code</code></summary><p>x</p></details>")]
	[TranslationDataRow("<details><summary> `code` </summary>x</details>", "<details><summary><code>code</code></summary><p>x</p></details>")]
	[TranslationDataRow("<details><summary>`code`</summary>x</details>", "<details><summary>`code`</summary><p>x</p></details>")]
	[TranslationDataRow("<details><summary>a &amp; b</summary>x</details>", "<details><summary>a &amp;amp; b</summary><p>x</p></details>")]
	[TranslationDataRow("<details><summary><script></summary>x</details>", "<details><summary>&lt;script&gt;</summary><p>x</p></details>")]

	// the summary is trimmed, just like the content is
	[TranslationDataRow("<details><summary>   title   </summary>x</details>", "<details><summary>title</summary><p>x</p></details>")]

	// the content carries blocks
	[TranslationDataRow("<details>\n<summary>title</summary>\n\n# heading\n\n</details>", "<details><summary>title</summary><h1>heading</h1></details>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\n\n- a\n- b\n\n</details>", "<details><summary>title</summary><ul><li>a</li><li>b</li></ul></details>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\n\n> quoted\n\n</details>", "<details><summary>title</summary><blockquote><p>quoted</p></blockquote></details>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\n\nfirst\n\nsecond\n\n</details>", "<details><summary>title</summary><p>first</p><p>second</p></details>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\n\n| a | b |\n\n</details>", "<details><summary>title</summary><table><tr><td>a</td><td>b</td></tr></table></details>")]

	// line endings
	[TranslationDataRow("<details>\r\n<summary>title</summary>\r\n\r\nhello\r\n\r\n</details>", "<details><summary>title</summary><p>hello</p></details>")]

	// indentation
	[TranslationDataRow("  <details>\n  <summary>title</summary>\n  hello\n  </details>", "<details><summary>title</summary><p>hello</p></details>")]
	[TranslationDataRow("\t<details>\n\t<summary>title</summary>\n\thello\n\t</details>", "<details><summary>title</summary><p>hello</p></details>")]

	// a summary only opens the block, anywhere else it is content like any other
	[TranslationDataRow("<details>\nhello\n<summary>title</summary>\n</details>", "<details><p>hello</p><p>&lt;summary&gt;title&lt;/summary&gt;</p></details>")]

	// a summary which is never closed is not one
	[TranslationDataRow("<details>\n<summary>title\nhello\n</details>", "<details><p>&lt;summary&gt;title</p><p>hello</p></details>")]

	// a block which is never closed is not recognized at all
	[TranslationDataRow("<details>\nhello", "<p>&lt;details&gt;</p><p>hello</p>")]
	[TranslationDataRow("<details>", "<p>&lt;details&gt;</p>")]
	[TranslationDataRow("<details>\n<summary>title</summary>\nhello", "<p>&lt;details&gt;</p><p>&lt;summary&gt;title&lt;/summary&gt;</p><p>hello</p>")]

	// a closing tag of its own is nothing but text
	[TranslationDataRow("</details>", "<p>&lt;/details&gt;</p>")]

	// the tags are written exactly as they are rendered
	[TranslationDataRow("<DETAILS>\nhello\n</DETAILS>", "<p>&lt;DETAILS&gt;</p><p>hello</p><p>&lt;/DETAILS&gt;</p>")]
	[TranslationDataRow("<details open>\nhello\n</details>", "<p>&lt;details open&gt;</p><p>hello</p><p>&lt;/details&gt;</p>")]

	// the block is closed by the first tag which closes one, so a block written inside another one is not one of its own
	[TranslationDataRow("<details>\n<details>\nhello\n</details>\n</details>", "<details><p>&lt;details&gt;</p><p>hello</p></details><p>&lt;/details&gt;</p>")]

	// whatever follows the closing tag is a block of its own
	[TranslationDataRow("<details>hello</details>world", "<details><p>hello</p></details><p>world</p>")]
	[TranslationDataRow("<details>\nhello\n</details>\n\n# heading", "<details><p>hello</p></details><h1>heading</h1>")]

	// surrounding blocks
	[TranslationDataRow("# heading\n<details>\nhello\n</details>", "<h1>heading</h1><details><p>hello</p></details>")]
	[TranslationDataRow("<details>\nfirst\n</details>\n\n<details>\nsecond\n</details>", "<details><p>first</p></details><details><p>second</p></details>")]

	// a tag written in the middle of a line is inline content, which is nothing but text
	[TranslationDataRow("hello <details>world</details>", "<p>hello &lt;details&gt;world&lt;/details&gt;</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	public void ParseSummaryAndContent()
	{
		var document = Parser.Parse("<details>\n<summary>title</summary>\n\nfirst\n\nsecond\n\n</details>");

		var details = Assert.IsInstanceOfType<DetailsBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual("title", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(details.Summary)).Text);
		Assert.HasCount(3, details.Blocks);
		Assert.AreEqual("first", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(details.Blocks[0]).Runs)).Text);
		Assert.IsInstanceOfType<EmptyLineNode>(details.Blocks[1]);
		Assert.AreEqual("second", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(details.Blocks[2]).Runs)).Text);
	}


	[TestMethod]
	public void ParseSummaryWithInlineContent()
	{
		var document = Parser.Parse("<details><summary>*hello* world</summary>x</details>");

		var details = Assert.IsInstanceOfType<DetailsBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, details.Summary);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ItalicNode>(details.Summary[0]).Runs)).Text);
		Assert.AreEqual(" world", Assert.IsInstanceOfType<TextNode>(details.Summary[1]).Text);
	}


	[TestMethod]
	public void ParseWithoutSummary()
	{
		var document = Parser.Parse("<details>\nhello\n</details>");

		var details = Assert.IsInstanceOfType<DetailsBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.IsEmpty(details.Summary);
		Assert.AreEqual("hello", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(details.Blocks)).Runs)).Text);
	}


	[TestMethod]
	public void ParseWithoutContent()
	{
		var document = Parser.Parse("<details><summary>title</summary></details>");

		var details = Assert.IsInstanceOfType<DetailsBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual("title", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(details.Summary)).Text);
		Assert.IsEmpty(details.Blocks);
	}


	[TestMethod]
	public void ParseFollowedByContent()
	{
		var document = Parser.Parse("<details>\nhello\n</details>\n\n# heading");

		Assert.IsInstanceOfType<DetailsBlockNode>(document.Blocks[0]);
		var heading = Assert.IsInstanceOfType<HeadingNode>(document.Blocks[^1]);
		Assert.AreEqual(1, heading.Level);
		Assert.AreEqual("heading", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(heading.Runs)).Text);
	}


	[TestMethod]
	public void VisitSummaryAndContent()
	{
		var document = Parser.Parse("<details><summary>title</summary>hello</details>");

		var visitor = new TextCollectingVisitor();
		visitor.VisitDocument(document);

		// the summary is visited before the content, just like it is written
		Assert.AreEqual("title|hello", String.Join('|', visitor.Texts));
	}

	private sealed class TextCollectingVisitor : DocumentObjectModelVisitor
	{
		public List<string> Texts { get; } = [];

		protected override void Visit(TextNode node) => Texts.Add(node.Text);
	}


	[TestMethod]

	// never closed
	[DataRow("<details>")]
	[DataRow("<details>\nhello")]
	[DataRow("<details>\n<summary>title</summary>\nhello")]

	// a closing tag of its own
	[DataRow("</details>")]

	// the tags are written exactly as they are rendered
	[DataRow("<DETAILS>\nhello\n</DETAILS>")]
	[DataRow("<details open>\nhello\n</details>")]

	// not at the beginning of a line
	[DataRow("hello <details>world</details>")]

	public void ParseWithoutDetails(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is DetailsBlockNode));
	}
}
