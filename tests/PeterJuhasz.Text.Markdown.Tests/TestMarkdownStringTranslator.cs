using Microsoft.Extensions.Primitives;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Tests;

internal sealed class TestMarkdownStringTranslator : HtmlStringMarkdownTranslator
{
	protected override void VisitMention(Node node, StringSegment userName)
	{
		base.OpenElement("mention");
		base.WriteHtml('@');
		base.WriteText(userName);
		base.CloseElement("mention");
	}

	protected override void VisitHashtag(Node node, StringSegment userName)
	{
		base.OpenElement("hashtag");
		base.WriteHtml('#');
		base.WriteText(userName);
		base.CloseElement("hashtag");
	}

	protected override void VisitEmojiSmiley(Node node, StringSegment smiley)
	{
		base.OpenElement("smiley");
		base.WriteText(smiley);
		base.CloseElement("smiley");
	}

	protected override void VisitEmojiAlias(Node node, StringSegment alias)
	{
		base.OpenElement("emoji");
		base.WriteText(alias);
		base.CloseElement("emoji");
	}

	protected override void VisitInlineMath(Node node, StringSegment math)
	{
		base.OpenElement("math");
		base.WriteText(math);
		base.CloseElement("math");
	}

	protected override void VisitMathBlock(Node node, StringSegment math)
	{
		base.OpenElement("math");
		base.WriteText(math);
		base.CloseElement("math");
	}

	protected override void VisitFrontMatter(Node node, StringSegment frontMatter)
	{
		base.OpenElement("frontmatter");
		base.WriteText(frontMatter);
		base.CloseElement("frontmatter");
	}

	protected override void VisitComment(Node node, StringSegment comment)
	{
		base.OpenElement("comment");
		base.WriteText(comment);
		base.CloseElement("comment");
	}

	protected override void VisitInlineComment(Node node, StringSegment comment)
	{
		base.OpenElement("comment");
		base.WriteText(comment);
		base.CloseElement("comment");
	}
}
