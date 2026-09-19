using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class CodeBlockTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("```\ncode\n```", "<code>code</code>")]
	[TranslationDataRow("```csharp\nvar x = 1;\n```", "<code>var x = 1;</code>")]
	[TranslationDataRow("```\nfirst\nsecond\n```", "<code>first&#xA;second</code>")]
	[TranslationDataRow("```\nfirst\n\nsecond\n```", "<code>first&#xA;&#xA;second</code>")]
	[TranslationDataRow("```\n  indented\n    more\n```", "<code>  indented&#xA;    more</code>")]

	// line endings
	[TranslationDataRow("```\r\ncode\r\n```", "<code>code</code>")]
	[TranslationDataRow("before\r\n```\r\ncode\r\n```\r\nafter", "<p>before</p><code>code</code><p>after</p>")]

	// content is not parsed
	[TranslationDataRow("```\n*italic* **bold** _underline_ ~strikethrough~ `code`\n```", "<code>*italic* **bold** _underline_ ~strikethrough~ `code`</code>")]
	[TranslationDataRow("```\n@user #tag :alias: :) https://example.org [link](https://example.org)\n```", "<code>@user #tag :alias: :) https://example.org [link](https://example.org)</code>")]
	[TranslationDataRow("```\n# heading\n- item\n1. item\n> quote\n---\n```", "<code># heading&#xA;- item&#xA;1. item&#xA;&gt; quote&#xA;---</code>")]
	[TranslationDataRow("```\nnot \\*escaped\\*\n```", "<code>not \\*escaped\\*</code>")]
	[TranslationDataRow("```\n<b class=\"x\">&</b>\n```", "<code>&lt;b class=&quot;x&quot;&gt;&amp;&lt;/b&gt;</code>")]

	// fence length
	[TranslationDataRow("````\n```\ncode\n```\n````", "<code>```&#xA;code&#xA;```</code>")]
	[TranslationDataRow("```\na ``` b\n```", "<code>a ``` b</code>")]
	[TranslationDataRow("```\ncode\n````", "<code>code</code><p>`</p>")]

	// empty
	[TranslationDataRow("```\n```", "<code></code>")]

	// not closed
	[TranslationDataRow("```", "<p><code></code>`</p>")]
	[TranslationDataRow("```\ncode", "<p><code></code>`</p><p>code</p>")]

	// surrounding blocks
	[TranslationDataRow("before\n```\ncode\n```\nafter", "<p>before</p><code>code</code><p>after</p>")]
	[TranslationDataRow("before\n\n```\ncode\n```\n\nafter", "<p>before</p><code>code</code><p>after</p>")]
	[TranslationDataRow("# heading\n```\ncode\n```", "<h1>heading</h1><code>code</code>")]
	[TranslationDataRow("- item\n```\ncode\n```", "<ul><li>item</li></ul><code>code</code>")]
	[TranslationDataRow("```\nfirst\n```\n```\nsecond\n```", "<code>first</code><code>second</code>")]
	[TranslationDataRow("```\ncode\n```\n", "<code>code</code>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("```\ncode\n```", "code", "")]
	[DataRow("```csharp\nvar x = 1;\n```", "var x = 1;", "csharp")]
	[DataRow("``` csharp \nvar x = 1;\n```", "var x = 1;", "csharp")]
	[DataRow("```csharp\r\nvar x = 1;\r\n```", "var x = 1;", "csharp")]
	[DataRow("````markdown\n```\ncode\n```\n````", "```\ncode\n```", "markdown")]

	public void ParseCodeAndLanguage(string markdown, string expectedCode, string expectedLanguage)
	{
		var document = Parser.Parse(markdown);
		var codeBlock = Assert.IsInstanceOfType<CodeBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedCode, codeBlock.Code);
		Assert.AreEqual(expectedLanguage, codeBlock.Language);
	}
}
