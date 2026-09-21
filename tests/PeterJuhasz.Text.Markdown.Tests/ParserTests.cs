namespace System.Text.Markdown.Tests;

[TestClass]
public class ParserTests : TranslationTestBase
{
	[TestMethod]

	// text
	[TranslationDataRow("abc", "<p>abc</p>")]
	[TranslationDataRow(@"abc \* star", "<p>abc * star</p>")]

	// italic
	[TranslationDataRow("test *italic*", "<p>test <i>italic</i></p>")]
	[TranslationDataRow("test *italic* snippet", "<p>test <i>italic</i> snippet</p>")]
	[TranslationDataRow("*italic* snippet", "<p><i>italic</i> snippet</p>")]
	[TranslationDataRow("test not *italic snippet", "<p>test not *italic snippet</p>")]
	[TranslationDataRow(@"test *ita\*lic* snippet", "<p>test <i>ita*lic</i> snippet</p>")]

	// bold
	[TranslationDataRow("test **bold**", "<p>test <b>bold</b></p>")]
	[TranslationDataRow("test **bold** snippet", "<p>test <b>bold</b> snippet</p>")]
	[TranslationDataRow("**bold** snippet", "<p><b>bold</b> snippet</p>")]
	[TranslationDataRow(@"test **bo\**ld** snippet", "<p>test <b>bo**ld</b> snippet</p>")]

	// underline
	[TranslationDataRow("test _underlined_", "<p>test <u>underlined</u></p>")]
	[TranslationDataRow("test _underlined_ snippet", "<p>test <u>underlined</u> snippet</p>")]
	[TranslationDataRow("_underlined_ snippet", "<p><u>underlined</u> snippet</p>")]
	[TranslationDataRow("test not _underlined snippet", "<p>test not _underlined snippet</p>")]
	[TranslationDataRow(@"test _under\_lined_ snippet", "<p>test <u>under_lined</u> snippet</p>")]

	// strikethrough
	[TranslationDataRow("test ~underlined~", "<p>test <s>underlined</s></p>")]
	[TranslationDataRow("test ~underlined~ snippet", "<p>test <s>underlined</s> snippet</p>")]
	[TranslationDataRow("~underlined~ snippet", "<p><s>underlined</s> snippet</p>")]

	// inline code
	[TranslationDataRow("this is `code` snippet", "<p>this is <code>code</code> snippet</p>")]
	[TranslationDataRow("this is `c_od_e` snippet", "<p>this is <code>c_od_e</code> snippet</p>")]

	// line
	[TranslationDataRow("this is [link](https://example.org) snippet", """<p>this is <a href="https://example.org">link</a> snippet</p>""")]

	// heading
	[TranslationDataRow("# abc", "<h1>abc</h1>")]
	[TranslationDataRow("## abc", "<h2>abc</h2>")]
	[TranslationDataRow("### abc", "<h3>abc</h3>")]
	[TranslationDataRow("#### abc", "<h4>abc</h4>")]
	[TranslationDataRow("##### abc", "<h5>abc</h5>")]
	[TranslationDataRow("###### abc", "<h6>abc</h6>")]

	// blockquote
	[TranslationDataRow("> abc", "<blockquote><p>abc</p></blockquote>")]

	// e-mail address
	[TranslationDataRow("this isan@email.com address", """<p>this <a href="mailto:isan@email.com">isan@email.com</a> address</p>""")]

	// mention
	[TranslationDataRow("this @user works", "<p>this <mention>@user</mention> works</p>")]

	// hashtag
	[TranslationDataRow("this #tag works", "<p>this <hashtag>#tag</hashtag> works</p>")]
	[TranslationDataRow("#not heading", "<p><hashtag>#not</hashtag> heading</p>")]

	// emoji alias
	[TranslationDataRow("this :alias: works", "<p>this <emoji>alias</emoji> works</p>")]
	[TranslationDataRow("not: alias: works", "<p>not: alias: works</p>")]

	// emoji smiley
	[TranslationDataRow("this is a :) smiley", "<p>this is a <smiley>:)</smiley> smiley</p>")]

	// url
	[TranslationDataRow("this https://example.org works", """<p>this <a href="https://example.org">https://example.org</a> works</p>""")]

	// unordered list
	[TranslationDataRow("- first item\n- second", """<ul><li>first item</li><li>second</li></ul>""")]
	[TranslationDataRow("* first item\n* second", """<ul><li>first item</li><li>second</li></ul>""")]

	// ordered list
	[TranslationDataRow("1. first item\n2. second", """<ol><li>first item</li><li>second</li></ol>""")]

	// horizontal rule
	[TranslationDataRow("---", "<hr />")]
	[TranslationDataRow("-----", "<hr />")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	
	[TranslationDataRow("this is [**bold** and *italic* link](https://example.org) snippet", """<p>this is <a href="https://example.org"><b>bold</b> and <i>italic</i> link</a> snippet</p>""")]
	[TranslationDataRow("this is [@mention #hash link](https://example.org) snippet", """<p>this is <a href="https://example.org">@mention #hash link</a> snippet</p>""")]
	[TranslationDataRow("test\n\n- first\n- second\n\nokay", """<p>test</p><ul><li>first</li><li>second</li></ul><p>okay</p>""")]

	public void TranslateComplex(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]

	[TranslationDataRow("What a cute little dyno #dyno", """<p>What a cute little dyno <hashtag>#dyno</hashtag></p>""")]
	[TranslationDataRow("ha megjottel: :D\r\nhttps://youtu.be/fpSE8ESmPbY", """<p>ha megjottel: <smiley>:D</smiley></p><p><a href="https://youtu.be/fpSE8ESmPbY">https://youtu.be/fpSE8ESmPbY</a></p>""")]
	[TranslationDataRow(
		"""
		There can be fragments in the text of a post or a chat message which have special meaning, like referring to a tag, or mentioning another user. To make them easier to use, the text editor now offers applicable completions automatically for these fragments:  
		 - usernames  
		 - topics  
		 - smileys  
		 - embed schemes

		(Navigation among the completions with keyboard is coming soon.)
		""",
		"""<p>There can be fragments in the text of a post or a chat message which have special meaning, like referring to a tag, or mentioning another user. To make them easier to use, the text editor now offers applicable completions automatically for these fragments:</p><ul><li>usernames</li><li>topics</li><li>smileys</li><li>embed schemes</li></ul><p>(Navigation among the completions with keyboard is coming soon.)</p>""")]

	public void TranslateReal(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);
}
