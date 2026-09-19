using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class MathTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("$x$", "<p><math>x</math></p>")]
	[TranslationDataRow("this is $x^2$ snippet", "<p>this is <math>x^2</math> snippet</p>")]
	[TranslationDataRow("$a$ and $b$", "<p><math>a</math> and <math>b</math></p>")]
	[TranslationDataRow("($x$)", "<p>(<math>x</math>)</p>")]
	[TranslationDataRow("$x$, $y$.", "<p><math>x</math>, <math>y</math>.</p>")]
	[TranslationDataRow("$e^{i\\pi} = -1$", "<p><math>e^{i\\pi} = -1</math></p>")]

	// content is not parsed
	[TranslationDataRow("$a*b*c$", "<p><math>a*b*c</math></p>")]
	[TranslationDataRow("$x_1 - y_2$", "<p><math>x_1 - y_2</math></p>")]
	[TranslationDataRow("$\\frac{a}{b}$", "<p><math>\\frac{a}{b}</math></p>")]
	[TranslationDataRow("$`code` ~s~ [x](y)$", "<p><math>`code` ~s~ [x](y)</math></p>")]
	[TranslationDataRow("$a < b & c > d$", "<p><math>a &lt; b &amp; c &gt; d</math></p>")]
	[TranslationDataRow("$a @user #tag :alias: :) https://example.org$", "<p><math>a @user #tag :alias: :) https://example.org</math></p>")]

	// escaped
	[TranslationDataRow("\\$x\\$", "<p>$x$</p>")]
	[TranslationDataRow("$a \\$ b$", "<p><math>a \\$ b</math></p>")]
	[TranslationDataRow("\\$$x$", "<p>$$x$</p>")]
	[TranslationDataRow("\\$ $x$", "<p>$ <math>x</math></p>")]

	// escaped does not affect following syntax
	[TranslationDataRow("\\$*a*", "<p>$<i>a</i></p>")]
	[TranslationDataRow("only \\$**100** today", "<p>only $<b>100</b> today</p>")]
	[TranslationDataRow("\\$_x_ and _y_", "<p>$<u>x</u> and <u>y</u></p>")]
	[TranslationDataRow("\\$~a~", "<p>$<s>a</s></p>")]
	[TranslationDataRow("\\$[a](https://example.org)", """<p>$<a href="https://example.org">a</a></p>""")]
	[TranslationDataRow("\\$\\$*a*", "<p>$$<i>a</i></p>")]

	// currency
	[TranslationDataRow("$5 and $10", "<p>$5 and $10</p>")]
	[TranslationDataRow("$20,000 and $30,000", "<p>$20,000 and $30,000</p>")]
	[TranslationDataRow("5$ and 10$", "<p>5$ and 10$</p>")]
	[TranslationDataRow("$x$5", "<p>$x$5</p>")]
	[TranslationDataRow("costs $5 and $x$ is", "<p>costs $5 and <math>x</math> is</p>")]
	[TranslationDataRow("between $5-$10 and $x$", "<p>between $5-$10 and <math>x</math></p>")]

	// whitespace next to delimiters
	[TranslationDataRow("$ x$", "<p>$ x$</p>")]
	[TranslationDataRow("$x $", "<p>$x $</p>")]
	[TranslationDataRow("$ $", "<p>$ $</p>")]

	// not closed
	[TranslationDataRow("$", "<p>$</p>")]
	[TranslationDataRow("$x", "<p>$x</p>")]
	[TranslationDataRow("a $x", "<p>a $x</p>")]

	// must start at word boundary
	[TranslationDataRow("a$x$", "<p>a$x$</p>")]

	// double delimiter within text is not math
	[TranslationDataRow("a $$x$$ b", "<p>a $$x$$ b</p>")]
	[TranslationDataRow("$$", "<p>$$</p>")]

	// nested in other elements
	[TranslationDataRow("# $x$", "<h1><math>x</math></h1>")]
	[TranslationDataRow("- $x$", "<ul><li><math>x</math></li></ul>")]
	[TranslationDataRow("1. $x$", "<ol><li><math>x</math></li></ol>")]
	[TranslationDataRow("> $x$", "<blockquote><p><math>x</math></p></blockquote>")]
	[TranslationDataRow("**bold $x$**", "<p><b>bold <math>x</math></b></p>")]
	[TranslationDataRow("[$x$](https://example.org)", """<p><a href="https://example.org"><math>x</math></a></p>""")]
	[TranslationDataRow("`$x$`", "<p><code>$x$</code></p>")]

	public void TranslateInline(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	// single line
	[TranslationDataRow("$$x^2$$", "<math>x^2</math>")]
	[TranslationDataRow("$$ x^2 $$", "<math>x^2</math>")]
	[TranslationDataRow("  $$x$$  ", "<math>x</math>")]
	[TranslationDataRow("$$\\sum_{i=1}^{n} i$$", "<math>\\sum_{i=1}^{n} i</math>")]
	[TranslationDataRow("$$a*b*c$$", "<math>a*b*c</math>")]
	[TranslationDataRow("$$a < b$$", "<math>a &lt; b</math>")]

	// multiple lines
	[TranslationDataRow("$$\nx^2\n$$", "<math>x^2</math>")]
	[TranslationDataRow("$$  \nx^2\n$$", "<math>x^2</math>")]
	[TranslationDataRow("$$\nfirst\nsecond\n$$", "<math>first&#xA;second</math>")]
	[TranslationDataRow("$$\nfirst\n\nsecond\n$$", "<math>first&#xA;&#xA;second</math>")]
	[TranslationDataRow("$$\n  indented\n    more\n$$", "<math>  indented&#xA;    more</math>")]

	// line endings
	[TranslationDataRow("$$\r\nx\r\n$$", "<math>x</math>")]
	[TranslationDataRow("before\r\n$$\r\nx\r\n$$\r\nafter", "<p>before</p><math>x</math><p>after</p>")]
	[TranslationDataRow("before\r\n$$x$$\r\nafter", "<p>before</p><math>x</math><p>after</p>")]

	// content is not parsed
	[TranslationDataRow("$$\n# heading\n- item\n*italic*\n> quote\n$x$\n$$", "<math># heading&#xA;- item&#xA;*italic*&#xA;&gt; quote&#xA;$x$</math>")]
	[TranslationDataRow("$$\n```\ncode\n```\n$$", "<math>```&#xA;code&#xA;```</math>")]

	// fence
	[TranslationDataRow("$$\nx\n$$$", "<math>x</math><p>$</p>")]

	// empty
	[TranslationDataRow("$$\n$$", "<math></math>")]
	[TranslationDataRow("$$$$", "<p>$$$$</p>")]
	[TranslationDataRow("$$ $$", "<p>$$ $$</p>")]

	// not closed
	[TranslationDataRow("$$\nx", "<p>$$</p><p>x</p>")]
	[TranslationDataRow("$$x", "<p>$$x</p>")]
	[TranslationDataRow("$$x$", "<p>$$x$</p>")]

	// not a whole line
	[TranslationDataRow("$$a$$ b", "<p>$$a$$ b</p>")]
	[TranslationDataRow("$$a$$ b $$c$$", "<p>$$a$$ b $$c$$</p>")]
	[TranslationDataRow("$$x\ny$$", "<p>$$x</p><p>y$$</p>")]
	[TranslationDataRow("$$ \\$$", "<p>$$ $$</p>")]

	// surrounding blocks
	[TranslationDataRow("before\n$$x$$\nafter", "<p>before</p><math>x</math><p>after</p>")]
	[TranslationDataRow("before\n$$\nx\n$$\nafter", "<p>before</p><math>x</math><p>after</p>")]
	[TranslationDataRow("before\n\n$$\nx\n$$\n\nafter", "<p>before</p><math>x</math><p>after</p>")]
	[TranslationDataRow("# heading\n$$x$$", "<h1>heading</h1><math>x</math>")]
	[TranslationDataRow("- item\n$$\nx\n$$", "<ul><li>item</li></ul><math>x</math>")]
	[TranslationDataRow("$$\nx\n$$\n$$y$$", "<math>x</math><math>y</math>")]
	[TranslationDataRow("$$\nx\n$$\n", "<math>x</math>")]

	public void TranslateBlock(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("$x^2$", "x^2")]
	[DataRow("$\\frac{a}{b}$", "\\frac{a}{b}")]
	[DataRow("$a \\$ b$", "a \\$ b")]
	[DataRow("$a*b_c$", "a*b_c")]

	public void ParseInlineMath(string markdown, string expectedExpression)
	{
		var document = Parser.Parse(markdown);
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		var math = Assert.IsInstanceOfType<InlineMathNode>(Assert.ContainsSingle(paragraph.Runs));
		Assert.AreEqual(expectedExpression, math.Expression);
	}


	[TestMethod]
	public void ParseInlineMathWithText()
	{
		var document = Parser.Parse("area is $\\pi r^2$ here");
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(3, paragraph.Runs);
		Assert.AreEqual("area is ", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual("\\pi r^2", Assert.IsInstanceOfType<InlineMathNode>(paragraph.Runs[1]).Expression);
		Assert.AreEqual(" here", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[2]).Text);
	}


	[TestMethod]

	// single line
	[DataRow("$$x^2$$", "x^2")]
	[DataRow("$$ x^2 $$", "x^2")]
	[DataRow("$$\\sum_{i=1}^{n} i$$", "\\sum_{i=1}^{n} i")]

	// multiple lines
	[DataRow("$$\nx^2\n$$", "x^2")]
	[DataRow("$$\r\nx^2\r\n$$", "x^2")]
	[DataRow("$$\nfirst\nsecond\n$$", "first\nsecond")]
	[DataRow("$$\nfirst\n\nsecond\n$$", "first\n\nsecond")]
	[DataRow("$$\n  indented\n$$", "  indented")]
	[DataRow("$$\n$$", "")]

	public void ParseMathBlock(string markdown, string expectedExpression)
	{
		var document = Parser.Parse(markdown);
		var math = Assert.IsInstanceOfType<MathBlockNode>(Assert.ContainsSingle(document.Blocks));
		Assert.AreEqual(expectedExpression, math.Expression);
	}
}
