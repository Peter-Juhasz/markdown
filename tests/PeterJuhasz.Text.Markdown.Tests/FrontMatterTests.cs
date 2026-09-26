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

	// basic
	[TranslationDataRow("+++\ntitle = \"Test\"\n+++", "<frontmatter>title = &quot;Test&quot;</frontmatter>")]
	[TranslationDataRow("+++\ntitle = \"Test\"\n\n[author]\nname = \"Peter\"\n+++", "<frontmatter>title = &quot;Test&quot;&#xA;&#xA;[author]&#xA;name = &quot;Peter&quot;</frontmatter>")]
	[TranslationDataRow("  +++\ntitle = \"Test\"\n+++  ", "<frontmatter>title = &quot;Test&quot;</frontmatter>")]
	[TranslationDataRow("+++  \ntitle = \"Test\"\n+++", "<frontmatter>title = &quot;Test&quot;</frontmatter>")]

	// line endings
	[TranslationDataRow("+++\r\ntitle = \"Test\"\r\n+++\r\n# heading", "<frontmatter>title = &quot;Test&quot;</frontmatter><h1>heading</h1>")]

	// content is not parsed
	[TranslationDataRow("+++\n# comment\n- item\n> quote\n+++", "<frontmatter># comment&#xA;- item&#xA;&gt; quote</frontmatter>")]
	[TranslationDataRow("+++\ntitle = \"*italic* @user #tag :alias:\"\n+++", "<frontmatter>title = &quot;*italic* @user #tag :alias:&quot;</frontmatter>")]

	// may contain the fence of another format
	[TranslationDataRow("+++\ntitle = \"Test\"\n---\n+++", "<frontmatter>title = &quot;Test&quot;&#xA;---</frontmatter>")]

	// empty
	[TranslationDataRow("+++\n+++", "<frontmatter></frontmatter>")]
	[TranslationDataRow("+++\n\n+++", "<frontmatter></frontmatter>")]

	// fence
	[TranslationDataRow("++++\ntitle = \"Test\"\n++++", "<p>&#x2B;&#x2B;&#x2B;&#x2B;</p><p>title = &quot;Test&quot;</p><p>&#x2B;&#x2B;&#x2B;&#x2B;</p>")]
	[TranslationDataRow("+++toml\ntitle = \"Test\"\n+++", "<p>&#x2B;&#x2B;&#x2B;toml</p><p>title = &quot;Test&quot;</p><p>&#x2B;&#x2B;&#x2B;</p>")]

	// not closed
	[TranslationDataRow("+++", "<p>&#x2B;&#x2B;&#x2B;</p>")]
	[TranslationDataRow("+++\ntitle = \"Test\"", "<p>&#x2B;&#x2B;&#x2B;</p><p>title = &quot;Test&quot;</p>")]

	// not closed by the fence of another format
	[TranslationDataRow("+++\ntitle = \"Test\"\n---", "<p>&#x2B;&#x2B;&#x2B;</p><p>title = &quot;Test&quot;</p><hr />")]
	[TranslationDataRow("---\ntitle: Test\n+++", "<hr /><p>title: Test</p><p>&#x2B;&#x2B;&#x2B;</p>")]

	// must be at the very beginning of the document
	[TranslationDataRow("\n+++\ntitle = \"Test\"\n+++", "<p>&#x2B;&#x2B;&#x2B;</p><p>title = &quot;Test&quot;</p><p>&#x2B;&#x2B;&#x2B;</p>")]
	[TranslationDataRow("text\n+++\ntitle = \"Test\"\n+++", "<p>text</p><p>&#x2B;&#x2B;&#x2B;</p><p>title = &quot;Test&quot;</p><p>&#x2B;&#x2B;&#x2B;</p>")]

	// following blocks
	[TranslationDataRow("+++\ntitle = \"Test\"\n+++\n\n# heading\n\ntext", "<frontmatter>title = &quot;Test&quot;</frontmatter><h1>heading</h1><p>text</p>")]

	public void TranslateToml(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// basic
	[TranslationDataRow(";;;\n{\n  \"title\": \"Test\"\n}\n;;;", "<frontmatter>{&#xA;  &quot;title&quot;: &quot;Test&quot;&#xA;}</frontmatter>")]
	[TranslationDataRow(";;;\n\"title\": \"Test\",\n\"tags\": [\"a\", \"b\"]\n;;;", "<frontmatter>&quot;title&quot;: &quot;Test&quot;,&#xA;&quot;tags&quot;: [&quot;a&quot;, &quot;b&quot;]</frontmatter>")]
	[TranslationDataRow("  ;;;\n{}\n;;;  ", "<frontmatter>{}</frontmatter>")]
	[TranslationDataRow(";;;  \n{}\n;;;", "<frontmatter>{}</frontmatter>")]

	// line endings
	[TranslationDataRow(";;;\r\n{}\r\n;;;\r\n# heading", "<frontmatter>{}</frontmatter><h1>heading</h1>")]

	// content is not parsed
	[TranslationDataRow(";;;\n{ \"title\": \"*italic* @user #tag :alias: ;) <b>\" }\n;;;", "<frontmatter>{ &quot;title&quot;: &quot;*italic* @user #tag :alias: ;) &lt;b&gt;&quot; }</frontmatter>")]

	// empty
	[TranslationDataRow(";;;\n;;;", "<frontmatter></frontmatter>")]
	[TranslationDataRow(";;;\n\n;;;", "<frontmatter></frontmatter>")]

	// fence
	[TranslationDataRow(";;;;\n{}\n;;;;", "<p>;;;;</p><p>{}</p><p>;;;;</p>")]
	[TranslationDataRow(";;;json\n{}\n;;;", "<p>;;;json</p><p>{}</p><p>;;;</p>")]

	// not closed
	[TranslationDataRow(";;;", "<p>;;;</p>")]
	[TranslationDataRow(";;;\n{}", "<p>;;;</p><p>{}</p>")]

	// not closed by the fence of another format
	[TranslationDataRow(";;;\n{}\n+++", "<p>;;;</p><p>{}</p><p>&#x2B;&#x2B;&#x2B;</p>")]

	// must be at the very beginning of the document
	[TranslationDataRow("text\n;;;\n{}\n;;;", "<p>text</p><p>;;;</p><p>{}</p><p>;;;</p>")]

	// following blocks
	[TranslationDataRow(";;;\n{}\n;;;\n\n# heading\n\ntext", "<frontmatter>{}</frontmatter><h1>heading</h1><p>text</p>")]

	public void TranslateJson(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// yaml
	[DataRow("---\ntitle: Test\n---", "title: Test", FrontMatterFormat.Yaml)]
	[DataRow("---\r\ntitle: Test\r\n---", "title: Test", FrontMatterFormat.Yaml)]
	[DataRow("---\ntitle: Test\nauthor: Peter\n---", "title: Test\nauthor: Peter", FrontMatterFormat.Yaml)]
	[DataRow("---\ntitle: Test\n\nauthor: Peter\n---", "title: Test\n\nauthor: Peter", FrontMatterFormat.Yaml)]
	[DataRow("---\nlist:\n  - a\n---", "list:\n  - a", FrontMatterFormat.Yaml)]
	[DataRow("---\n---", "", FrontMatterFormat.Yaml)]
	[DataRow("---\n\n---", "", FrontMatterFormat.Yaml)]

	// toml
	[DataRow("+++\ntitle = \"Test\"\n+++", "title = \"Test\"", FrontMatterFormat.Toml)]
	[DataRow("+++\r\ntitle = \"Test\"\r\n+++", "title = \"Test\"", FrontMatterFormat.Toml)]
	[DataRow("+++\ntitle = \"Test\"\n\n[author]\nname = \"Peter\"\n+++", "title = \"Test\"\n\n[author]\nname = \"Peter\"", FrontMatterFormat.Toml)]
	[DataRow("+++\n+++", "", FrontMatterFormat.Toml)]

	// json
	[DataRow(";;;\n{\n  \"title\": \"Test\"\n}\n;;;", "{\n  \"title\": \"Test\"\n}", FrontMatterFormat.Json)]
	[DataRow(";;;\r\n{ \"title\": \"Test\" }\r\n;;;", "{ \"title\": \"Test\" }", FrontMatterFormat.Json)]
	[DataRow(";;;\n;;;", "", FrontMatterFormat.Json)]

	public void ParseFrontMatter(string markdown, string expectedContent, FrontMatterFormat expectedFormat)
	{
		var document = Parser.Parse(markdown);
		var frontMatter = Assert.IsInstanceOfType<FrontMatterNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedContent, frontMatter.Content);
		Assert.AreEqual(expectedFormat, frontMatter.Format);
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
	[DataRow("+++toml\ntitle = \"Test\"\n+++")]
	[DataRow(";;;json\n{}\n;;;")]

	// closed by the fence of another format
	[DataRow("---\ntitle: Test\n+++")]
	[DataRow("+++\ntitle = \"Test\"\n;;;")]
	[DataRow(";;;\n{}\n---")]

	public void ParseWithoutFrontMatter(string markdown)
	{
		var document = Parser.Parse(markdown);
		Assert.IsFalse(document.Blocks.Any(b => b is FrontMatterNode));
	}
}
