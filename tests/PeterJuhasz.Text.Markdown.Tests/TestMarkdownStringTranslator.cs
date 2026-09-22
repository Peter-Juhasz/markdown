using Microsoft.Extensions.Primitives;
using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

internal sealed class TestMarkdownStringTranslator(HtmlWriter<ArrayBufferWriter<char>> writer)
	: HtmlStringMarkdownTranslator<ArrayBufferWriter<char>>(writer)
{
	protected override void VisitMention(Node node, StringSegment userName)
	{
		Writer.OpenElement("mention");
		Writer.WriteText("@");
		Writer.WriteText(userName);
		Writer.CloseElement();
	}

	protected override void VisitHashtag(Node node, StringSegment userName)
	{
		Writer.OpenElement("hashtag");
		Writer.WriteText("#");
		Writer.WriteText(userName);
		Writer.CloseElement();
	}

	protected override void VisitEmojiSmiley(Node node, StringSegment smiley)
	{
		Writer.OpenElement("smiley");
		Writer.WriteText(smiley);
		Writer.CloseElement();
	}

	protected override void VisitEmojiAlias(Node node, StringSegment alias)
	{
		Writer.OpenElement("emoji");
		Writer.WriteText(alias);
		Writer.CloseElement();
	}

	protected override void VisitInlineMath(Node node, StringSegment math)
	{
		Writer.OpenElement("math");
		Writer.WriteText(math);
		Writer.CloseElement();
	}

	protected override void VisitMathBlock(Node node, StringSegment math)
	{
		Writer.OpenElement("math");
		Writer.WriteText(math);
		Writer.CloseElement();
	}

	protected override void VisitFrontMatter(Node node, StringSegment frontMatter)
	{
		Writer.OpenElement("frontmatter");
		Writer.WriteText(frontMatter);
		Writer.CloseElement();
	}

	protected override void VisitComment(Node node, StringSegment comment)
	{
		Writer.OpenElement("comment");
		Writer.WriteText(comment);
		Writer.CloseElement();
	}

	protected override void VisitInlineComment(Node node, StringSegment comment)
	{
		Writer.OpenElement("comment");
		Writer.WriteText(comment);
		Writer.CloseElement();
	}

	protected override void VisitFootnoteContent(Node node, int number)
	{
		Span<char> id = stackalloc char[11];
		number.TryFormat(id, out var formatted);

		Writer.OpenElement("footnote");
		Writer.WriteAttribute("id", id[..formatted]);
		base.VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitFootnoteReference(Node node, int number)
	{
		Writer.OpenElement("footnoteref");
		base.WriteNumber(number);
		Writer.CloseElement();
	}
}
