using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

[TestClass]
public class FrontMatterTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("---\ntitle: Test\n---", "<frontmatter>title: Test</frontmatter>")]
	[TranslationDataRow("---\ntitle: Test\nauthor: Peter\n---", "<frontmatter>title: Test&#xA;author: Peter</frontmatter>")]
	[TranslationDataRow("---\ntitle: Test\n\nauthor: Peter\n---", "<frontmatter>title: Test&#xA;&#xA;author: Peter</frontmatter>")]
	[TranslationDataRow("---\nlist:\n  - a\n  - b\n---", "<frontmatter>list:&#xA;  - a&#xA;  - b</frontmatter>")]
	[TranslationDataRow("  ---\ntitle: Test\n---  ", "<frontmatter>title: Test</frontmatter>")]

	// opening delimiter may have trailing whitespace of its own
	[TranslationDataRow("---  \ntitle: Test\n---", "<frontmatter>title: Test</frontmatter>")]

	// line endings
	[TranslationDataRow("---\r\ntitle: Test\r\n---", "<frontmatter>title: Test</frontmatter>")]
	[TranslationDataRow("---\r\ntitle: Test\r\n---\r\n# heading", "<frontmatter>title: Test</frontmatter><h1>heading</h1>")]

	// content is not parsed
	[TranslationDataRow("---\n# heading\n- item\n1. item\n> quote\n---", "<frontmatter># heading&#xA;- item&#xA;1. item&#xA;&gt; quote</frontmatter>")]
	[TranslationDataRow("---\n*italic* **bold** _underline_ ~strikethrough~ `code`\n---", "<frontmatter>*italic* **bold** _underline_ ~strikethrough~ `code`</frontmatter>")]
	[TranslationDataRow("---\n@user #tag :alias: :) https://example.org [link](https://example.org)\n---", "<frontmatter>@user #tag :alias: :) https://example.org [link](https://example.org)</frontmatter>")]
	[TranslationDataRow("---\n|a|b|\n|-|-|\n---", "<frontmatter>|a|b|&#xA;|-|-|</frontmatter>")]
	[TranslationDataRow("---\n```\ncode\n```\n---", "<frontmatter>```&#xA;code&#xA;```</frontmatter>")]
	[TranslationDataRow("---\ntitle: not \\*escaped\\*\n---", "<frontmatter>title: not \\*escaped\\*</frontmatter>")]
	[TranslationDataRow("---\ntitle: \"a < b & c\"\n---", "<frontmatter>title: &quot;a &lt; b &amp; c&quot;</frontmatter>")]

	// empty
	[TranslationDataRow("---\n---", "<frontmatter></frontmatter>")]
	[TranslationDataRow("---\n\n---", "<frontmatter></frontmatter>")]

	// trailing empty lines are trimmed
	[TranslationDataRow("---\ntitle: Test\n\n---", "<frontmatter>title: Test</frontmatter>")]

	// fence
	[TranslationDataRow("---\ntitle: Test\n----", "<frontmatter>title: Test</frontmatter><hr />")]
	[TranslationDataRow("----\ntitle: Test\n----", "<hr /><p>title: Test</p><hr />")]
	[TranslationDataRow("---yaml\ntitle: Test\n---", "<p>---yaml</p><p>title: Test</p><hr />")]

	// not closed
	[TranslationDataRow("---", "<hr />")]
	[TranslationDataRow("---\n", "<hr />")]
	[TranslationDataRow("---\ntitle: Test", "<hr /><p>title: Test</p>")]

	// must be at the very beginning of the document
	[TranslationDataRow("\n---\ntitle: Test\n---", "<hr /><p>title: Test</p><hr />")]
	[TranslationDataRow("# heading\n---\ntitle: Test\n---", "<h1>heading</h1><hr /><p>title: Test</p><hr />")]
	[TranslationDataRow("text\n---\ntitle: Test\n---", "<p>text</p><hr /><p>title: Test</p><hr />")]
	[TranslationDataRow("---\ntitle: Test\n---\n---\nsecond: Test\n---", "<frontmatter>title: Test</frontmatter><hr /><p>second: Test</p><hr />")]

	// not recognized within another block
	[TranslationDataRow("> ---\n> title: Test\n> ---", "<blockquote><p>---</p><p>title: Test</p><p>---</p></blockquote>")]

	// following blocks
	[TranslationDataRow("---\ntitle: Test\n---\n# heading", "<frontmatter>title: Test</frontmatter><h1>heading</h1>")]
	[TranslationDataRow("---\ntitle: Test\n---\n\n# heading\n\ntext", "<frontmatter>title: Test</frontmatter><h1>heading</h1><p>text</p>")]
	[TranslationDataRow("---\ntitle: Test\n---\ntext", "<frontmatter>title: Test</frontmatter><p>text</p>")]
	[TranslationDataRow("---\n---\n# heading", "<frontmatter></frontmatter><h1>heading</h1>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("---\ntitle: Test\n---", "title: Test")]
	[DataRow("---\r\ntitle: Test\r\n---", "title: Test")]
	[DataRow("---\ntitle: Test\nauthor: Peter\n---", "title: Test\nauthor: Peter")]
	[DataRow("---\ntitle: Test\n\nauthor: Peter\n---", "title: Test\n\nauthor: Peter")]
	[DataRow("---\nlist:\n  - a\n---", "list:\n  - a")]
	[DataRow("---\n---", "")]
	[DataRow("---\n\n---", "")]

	public void ParseFrontMatter(string markdown, string expectedContent)
	{
		var document = Parser.Parse(markdown);
		var frontMatter = Assert.IsInstanceOfType<FrontMatterNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedContent, frontMatter.Content);
	}


	[TestMethod]
	public void ParseFrontMatterFollowedByContent()
	{
		var document = Parser.Parse("---\ntitle: Test\n---\n# heading");

		Assert.AreEqual("title: Test", Assert.IsInstanceOfType<FrontMatterNode>(document.Blocks[0]).Content);
		var heading = Assert.IsInstanceOfType<HeadingNode>(document.Blocks[^1]);
		Assert.AreEqual(1, heading.Level);
		Assert.AreEqual("heading", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(heading.Runs)).Text);
	}


	[TestMethod]

	// not at the beginning of the document
	[DataRow("# heading\n---\ntitle: Test\n---")]
	[DataRow("\n---\ntitle: Test\n---")]

	// not closed
	[DataRow("---")]
	[DataRow("---\ntitle: Test")]

	// the opening delimiter is not a whole line of its own
	[DataRow("---yaml\ntitle: Test\n---")]
	[DataRow("----\ntitle: Test\n----")]

	public void ParseWithoutFrontMatter(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is FrontMatterNode));
	}
}
