using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

[TestClass]
public class AngleBracketLinkTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("<https://example.org>", """<p><a href="https://example.org">https://example.org</a></p>""")]
	[TranslationDataRow("this <https://example.org> works", """<p>this <a href="https://example.org">https://example.org</a> works</p>""")]
	[TranslationDataRow("<https://example.org> and <https://example.com>", """<p><a href="https://example.org">https://example.org</a> and <a href="https://example.com">https://example.com</a></p>""")]

	// path, query and fragment
	[TranslationDataRow("<https://example.org/a/b>", """<p><a href="https://example.org/a/b">https://example.org/a/b</a></p>""")]
	[TranslationDataRow("<https://example.org/a?b=c>", """<p><a href="https://example.org/a?b=c">https://example.org/a?b=c</a></p>""")]
	[TranslationDataRow("<https://example.org/a#b>", """<p><a href="https://example.org/a#b">https://example.org/a#b</a></p>""")]
	[TranslationDataRow("<https://example.org:8443/a>", """<p><a href="https://example.org:8443/a">https://example.org:8443/a</a></p>""")]

	// the punctuation which follows is not a part of the URL anymore
	[TranslationDataRow("see <https://example.org>.", """<p>see <a href="https://example.org">https://example.org</a>.</p>""")]
	[TranslationDataRow("(<https://example.org>)", """<p>(<a href="https://example.org">https://example.org</a>)</p>""")]

	// not closed, so only the bare URL is recognized
	[TranslationDataRow("<https://example.org", """<p>&lt;<a href="https://example.org">https://example.org</a></p>""")]
	[TranslationDataRow("<https://example.org x>", """<p>&lt;<a href="https://example.org">https://example.org</a> x&gt;</p>""")]

	// the innermost bracket opens the link
	[TranslationDataRow("<<https://example.org>", """<p>&lt;<a href="https://example.org">https://example.org</a></p>""")]

	// a quote may not be a part of the URL, so that it can't break out of the attribute it is rendered into
	[TranslationDataRow("""<https://example.org/a"b>""", """<p>&lt;<a href="https://example.org/a">https://example.org/a</a>&quot;b&gt;</p>""")]
	[TranslationDataRow("<https://example.org/a'b>", """<p>&lt;<a href="https://example.org/a">https://example.org/a</a>&#x27;b&gt;</p>""")]

	// escaped
	[TranslationDataRow(@"\<https://example.org>", """<p>&lt;<a href="https://example.org">https://example.org</a>&gt;</p>""")]

	// not a URL
	[TranslationDataRow("<example.org>", "<p>&lt;example.org&gt;</p>")]
	[TranslationDataRow("<b>", "<p>&lt;b&gt;</p>")]
	[TranslationDataRow("<>", "<p>&lt;&gt;</p>")]
	[TranslationDataRow("<http://example.org>", "<p>&lt;http://example.org&gt;</p>")]

	// a bare URL is still recognized on its own
	[TranslationDataRow("https://example.org", """<p><a href="https://example.org">https://example.org</a></p>""")]
	[TranslationDataRow("this https://example.org/a(b) ends", """<p>this <a href="https://example.org/a">https://example.org/a</a>(b) ends</p>""")]

	// nested in other blocks
	[TranslationDataRow("# <https://example.org>", """<h1><a href="https://example.org">https://example.org</a></h1>""")]
	[TranslationDataRow("- <https://example.org>", """<ul><li><a href="https://example.org">https://example.org</a></li></ul>""")]
	[TranslationDataRow("1. <https://example.org>", """<ol><li><a href="https://example.org">https://example.org</a></li></ol>""")]
	[TranslationDataRow("> <https://example.org>", """<blockquote><p><a href="https://example.org">https://example.org</a></p></blockquote>""")]
	[TranslationDataRow("| <https://example.org> |", """<table><tr><td><a href="https://example.org">https://example.org</a></td></tr></table>""")]
	[TranslationDataRow("**<https://example.org>**", """<p><b><a href="https://example.org">https://example.org</a></b></p>""")]

	// links are not nested into each other
	[TranslationDataRow("[<https://example.org>](b)", """<p><a href="b">&lt;https://example.org&gt;</a></p>""")]

	// not parsed in code or math
	[TranslationDataRow("`<https://example.org>`", "<p><code>&lt;https://example.org&gt;</code></p>")]
	[TranslationDataRow("$<https://example.org>$", "<p><math>&lt;https://example.org&gt;</math></p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[DataRow("<https://example.org>", "https://example.org")]
	[DataRow("<https://example.org/a/b?c=d#e>", "https://example.org/a/b?c=d#e")]
	[DataRow("<https://>", "https://")]

	public void ParseUrl(string markdown, string expectedUrl)
	{
		var url = ParseSingleUrl<AngleBracketUrlNode>(markdown);
		Assert.AreEqual(expectedUrl, url.Url);
	}


	[TestMethod]
	public void ParseBareUrlIsNotAngleBracketUrl()
	{
		var url = ParseSingleUrl<UrlNode>("https://example.org");
		Assert.AreEqual("https://example.org", url.Url);
		Assert.IsNotInstanceOfType<AngleBracketUrlNode>(url);
	}


	[TestMethod]
	public void ParseAngleBracketUrlIsUrl()
	{
		// so that visitors which only handle URLs keep working
		var url = ParseSingleUrl<AngleBracketUrlNode>("<https://example.org>");
		Assert.IsInstanceOfType<UrlNode>(url);
	}


	[TestMethod]
	public void ParseSurroundedByText()
	{
		var document = Parser.Parse("see <https://example.org> now");
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(3, paragraph.Runs);
		Assert.AreEqual("see ", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[0]).Text);
		Assert.AreEqual("https://example.org", Assert.IsInstanceOfType<AngleBracketUrlNode>(paragraph.Runs[1]).Url);
		Assert.AreEqual(" now", Assert.IsInstanceOfType<TextNode>(paragraph.Runs[2]).Text);
	}


	private static T ParseSingleUrl<T>(string markdown) where T : UrlNode
	{
		var document = Parser.Parse(markdown);
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(document.Blocks));
		return Assert.IsInstanceOfType<T>(Assert.ContainsSingle(paragraph.Runs));
	}
}
