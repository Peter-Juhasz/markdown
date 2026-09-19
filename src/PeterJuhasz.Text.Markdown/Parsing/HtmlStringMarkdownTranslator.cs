using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Text.Encodings.Web;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public class HtmlStringMarkdownTranslator : InplaceMarkdownVisitor
{
	public HtmlStringMarkdownTranslator(
		HtmlEncoder? htmlEncoder = null,
		UrlEncoder? urlEncoder = null
	)
	{
		this.htmlEncoder = htmlEncoder ?? HtmlEncoder.Default;
		this.urlEncoder = urlEncoder ?? UrlEncoder.Default;
	}

	private char[] buffer = new char[1024];
	private int written;
	private readonly HtmlEncoder htmlEncoder;
	private readonly UrlEncoder urlEncoder;

	private const int StackLimit = 1024;

	protected override void VisitDocument(Node node)
	{
		written = 0;
		base.VisitDocument(node);
	}

	protected override void VisitHeading(Node node, int level)
	{
		var tag = level switch
		{
			1 => "h1",
			2 => "h2",
			3 => "h3",
			4 => "h4",
			5 => "h5",
			6 => "h6",
			_ => "h6"
		};
		OpenElement(tag);
		VisitInner(node);
		CloseElement(tag);
	}

	protected override void VisitParagraph(Node node)
	{
		OpenElement("p");
		VisitInner(node);
		CloseElement("p");
	}

	protected override void VisitBlockQuote(Node node)
	{
		OpenElement("blockquote");
		VisitInner(node);
		CloseElement("blockquote");
	}

	protected override void VisitSpoiler(Node node)
	{

	}

	protected override void VisitBold(Node node)
	{
		OpenElement("b");
		VisitInner(node);
		CloseElement("b");
	}

	protected override void VisitItalic(Node node)
	{
		OpenElement("i");
		VisitInner(node);
		CloseElement("i");
	}

	protected override void VisitUnderline(Node node)
	{
		OpenElement("u");
		VisitInner(node);
		CloseElement("u");
	}

	protected override void VisitStrikethrough(Node node)
	{
		OpenElement("s");
		VisitInner(node);
		CloseElement("s");
	}

	protected override void VisitEmailAddress(Node node, Segment emailAddress)
	{
		OpenOpenElement("a");
		OpenAttribute("href");
		WriteHtml("mailto:");
		WriteUri(emailAddress);
		CloseAttribute();
		CloseOpenElement();
		WriteText(emailAddress);
		CloseElement("a");
	}

	protected override void VisitPhoneNumber(Node node, Segment emailAddress)
	{
		OpenOpenElement("a");
		OpenAttribute("href");
		WriteHtml("tel:");
		WriteUri(emailAddress);
		CloseAttribute();
		CloseOpenElement();
		WriteText(emailAddress);
		CloseElement("a");
	}

	protected override void VisitUrl(Node node, Segment url)
	{
		OpenOpenElement("a");
		OpenAttribute("href");
		WriteHtml(url);
		CloseAttribute();
		CloseOpenElement();
		WriteText(url);
		CloseElement("a");
	}

	protected override void VisitLink(Node node, Segment url)
	{
		OpenOpenElement("a");
		OpenAttribute("href");
		WriteHtml(url);
		CloseAttribute();
		CloseOpenElement();
		VisitInner(node);
		CloseElement("a");
	}

	protected override void VisitEmbed(Node node, Segment type, Segment id)
	{

	}

	protected override void VisitMention(Node node, Segment userName)
	{

	}

	protected override void VisitHashtag(Node node, Segment tag)
	{

	}

	protected override void VisitText(Node node, Segment text)
	{
		WriteTextFromMarkdown(text);
	}

	protected override void VisitEmojiAlias(Node node, Segment alias)
	{
		WriteHtml(':');
		WriteText(alias);
		WriteHtml(':');
	}

	protected override void VisitEmojiSmiley(Node node, Segment smiley)
	{
		WriteText(smiley);
	}

	protected override void VisitHorizontalRule(Node node)
	{
		OpenOpenElement("hr");
		CloseElement();
	}

	protected override void VisitInlineCode(Node node, Segment code)
	{
		OpenElement("code");
		WriteText(code);
		CloseElement("code");
	}

	protected override void VisitCodeBlock(Node node, Segment code, Segment language)
	{
		OpenElement("code");
		WriteText(code);
		CloseElement("code");
	}

	protected override void VisitInlineMath(Node node, Segment math)
	{
		WriteText(math);
	}

	protected override void VisitMathBlock(Node node, Segment math)
	{
		OpenElement("p");
		WriteText(math);
		CloseElement("p");
	}

	protected override void VisitEmptyLine(Node node)
	{
	}

	protected override void VisitUnorderedList(Node node)
	{
		OpenElement("ul");
		VisitInner(node);
		CloseElement("ul");
	}

	protected override void VisitUnorderedListItem(Node node)
	{
		OpenElement("li");
		VisitInner(node);
		CloseElement("li");
	}

	protected override void VisitOrderedList(Node node)
	{
		OpenElement("ol");
		VisitInner(node);
		CloseElement("ol");
	}

	protected override void VisitOrderedListItem(Node node)
	{
		OpenElement("li");
		VisitInner(node);
		CloseElement("li");
	}

	protected override void VisitCheckbox(Node node, bool isChecked)
	{
		OpenElement("input");
		WriteAttribute("type", "checkbox");
		WriteAttribute("disabled");
		if (isChecked)
		{
			WriteAttribute("checked");
		}
		CloseElement();
	}


	private void EnsureCapacity(int additionalCapacity)
	{
		if (written + additionalCapacity > buffer.Length)
		{
			var newCapacity = Math.Max(2 * buffer.Length, written + additionalCapacity);
			Array.Resize(ref buffer, newCapacity);
		}
	}

	protected void WriteHtml(char ch)
	{
		EnsureCapacity(1);

		buffer[written] = ch;
		written++;
	}

	protected void WriteHtml(ReadOnlySpan<char> span)
	{
		EnsureCapacity(span.Length);

		var target = buffer.AsSpan(written);
		span.CopyTo(target);
		written += span.Length;
	}

	protected void WriteTextFromMarkdown(ReadOnlySpan<char> span)
	{
		Span<char> markdownDecoded = span.Length > StackLimit ? new char[span.Length] : stackalloc char[span.Length];
		Decode(span, markdownDecoded, out var decodedLength);
		WriteText(markdownDecoded[..decodedLength]);
	}

	protected void WriteText(ReadOnlySpan<char> span)
	{
		EnsureCapacity(span.Length);
		var target = buffer.AsSpan(written);
		var firstTry = htmlEncoder.Encode(span, target, out _, out var htmlEncodedLength);
		if (firstTry == OperationStatus.Done)
		{
			written += htmlEncodedLength;
			return;
		}

		EnsureCapacity(span.Length * htmlEncoder.MaxOutputCharactersPerInputCharacter);
		target = buffer.AsSpan(written);
		htmlEncoder.Encode(span, target, out _, out htmlEncodedLength);
		written += htmlEncodedLength;
	}

	protected void WriteUri(ReadOnlySpan<char> span)
	{
		Span<char> markdownDecoded = span.Length > StackLimit ? new char[span.Length] : stackalloc char[span.Length];
		Decode(span, markdownDecoded, out var decodedLength);

		var requiredUriEncodeLength = markdownDecoded.Length * urlEncoder.MaxOutputCharactersPerInputCharacter;
		Span<char> uriEncoded = requiredUriEncodeLength > StackLimit ? new char[requiredUriEncodeLength] : stackalloc char[requiredUriEncodeLength];
		urlEncoder.Encode(markdownDecoded[..decodedLength], uriEncoded, out _, out var uriEncodedLength);

		WriteText(uriEncoded[..uriEncodedLength]);
	}


	protected void OpenAttribute(string name)
	{
		WriteHtml(' ');
		WriteHtml(name);
		WriteHtml('=');
		WriteHtml('"');
	}

	protected void CloseAttribute() => WriteHtml('"');

	protected void WriteAttribute(string name, ReadOnlySpan<char> value)
	{
		OpenAttribute(name);
		WriteText(value);
		CloseAttribute();
	}

	protected void WriteAttribute(string name)
	{
		WriteHtml(' ');
		WriteHtml(name);
	}

	protected void OpenElement(string name)
	{
		WriteHtml('<');
		WriteHtml(name);
		WriteHtml('>');
	}

	protected void OpenOpenElement(string name)
	{
		WriteHtml('<');
		WriteHtml(name);
	}

	protected void CloseOpenElement() => WriteHtml('>');

	protected void CloseElement(string name)
	{
		WriteHtml('<');
		WriteHtml('/');
		WriteHtml(name);
		WriteHtml('>');
	}

	protected void CloseElement() => WriteHtml("/>");


	public override string ToString() => new(buffer[..written]);
}
