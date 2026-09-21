using Microsoft.Extensions.Primitives;
using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public class HtmlStringMarkdownTranslator<TWriter> : InplaceMarkdownVisitor where TWriter : IBufferWriter<char>
{
	public HtmlStringMarkdownTranslator(
		HtmlWriter<TWriter> writer,
		UrlEncoder? urlEncoder = null
	)
	{
		Writer = writer;
		this.urlEncoder = urlEncoder ?? UrlEncoder.Default;
	}

	private readonly UrlEncoder urlEncoder;
	private bool isTableHeaderRow;

	private const int StackLimit = 1024;

	/// <summary>
	/// An Int32 is never written with more characters than this, sign included.
	/// </summary>
	private const int MaxInt32Length = 11;

	protected HtmlWriter<TWriter> Writer { get; }

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
		Writer.OpenElement(tag);
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitParagraph(Node node)
	{
		Writer.OpenElement("p");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitBlockQuote(Node node)
	{
		Writer.OpenElement("blockquote");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitSpoiler(Node node)
	{

	}

	protected override void VisitAlert(Node node, Segment type)
	{
		Writer.OpenElement("blockquote");
		Writer.WriteAttribute("class", type);
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitDetails(Node node)
	{
		Writer.OpenElement("details");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitDetailsSummary(Node node)
	{
		Writer.OpenElement("summary");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitFigure(Node node)
	{
		Writer.OpenElement("figure");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitFigureCaption(Node node)
	{
		Writer.OpenElement("figcaption");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitBold(Node node)
	{
		Writer.OpenElement("b");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitItalic(Node node)
	{
		Writer.OpenElement("i");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitUnderline(Node node)
	{
		Writer.OpenElement("u");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitStrikethrough(Node node)
	{
		Writer.OpenElement("s");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitEmailAddress(Node node, Segment emailAddress)
	{
		Writer.OpenElement("a");
		WriteUriAttribute("href", "mailto:", emailAddress);
		Writer.WriteText(emailAddress);
		Writer.CloseElement();
	}

	protected override void VisitPhoneNumber(Node node, Segment phoneNumber)
	{
		Writer.OpenElement("a");
		WriteUriAttribute("href", "tel:", phoneNumber);
		Writer.WriteText(phoneNumber);
		Writer.CloseElement();
	}

	protected override void VisitUrl(Node node, Segment url)
	{
		Writer.OpenElement("a");
		Writer.WriteAttribute("href", url);
		Writer.WriteText(url);
		Writer.CloseElement();
	}

	protected override void VisitLink(Node node, Segment url, Segment title)
	{
		Writer.OpenElement("a");
		Writer.WriteAttribute("href", url);
		if (title.Length > 0)
		{
			WriteAttributeFromMarkdown("title", title);
		}
		VisitInner(node);
		Writer.CloseElement();
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
		Writer.WriteText(":");
		Writer.WriteText(alias);
		Writer.WriteText(":");
	}

	protected override void VisitEmojiSmiley(Node node, Segment smiley)
	{
		Writer.WriteText(smiley);
	}

	protected override void VisitHorizontalRule(Node node)
	{
		Writer.OpenElement("hr");
		Writer.CloseElement();
	}

	protected override void VisitInlineCode(Node node, Segment code)
	{
		Writer.OpenElement("code");
		Writer.WriteText(code);
		Writer.CloseElement();
	}

	protected override void VisitCodeBlock(Node node, Segment code, Segment language)
	{
		Writer.OpenElement("code");
		Writer.WriteText(code);
		Writer.CloseElement();
	}

	protected override void VisitInlineMath(Node node, Segment math)
	{
		Writer.WriteText(math);
	}

	protected override void VisitMathBlock(Node node, Segment math)
	{
		Writer.OpenElement("p");
		Writer.WriteText(math);
		Writer.CloseElement();
	}

	protected override void VisitFrontMatter(Node node, Segment frontMatter)
	{
		// Front matter carries metadata about the document, so it is not rendered.
	}

	/// <summary>
	/// The prefix the anchor of every footnote is built from, so that a reference and the content
	/// written for it find each other by the number alone.
	/// </summary>
	private const string FootnoteAnchorPrefix = "footnote-";

	protected override void VisitFootnoteContent(Node node, int number)
	{
		Span<char> id = stackalloc char[FootnoteAnchorPrefix.Length + MaxInt32Length];
		FootnoteAnchorPrefix.CopyTo(id);
		number.TryFormat(id[FootnoteAnchorPrefix.Length..], out var formatted);

		Writer.OpenElement("p");
		Writer.WriteAttribute("id", id[..(FootnoteAnchorPrefix.Length + formatted)]);
		Writer.OpenElement("sup");
		WriteNumber(number);
		Writer.CloseElement();
		Writer.WriteText(" ");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitFootnoteReference(Node node, int number)
	{
		Span<char> href = stackalloc char[1 + FootnoteAnchorPrefix.Length + MaxInt32Length];
		href[0] = '#';
		FootnoteAnchorPrefix.CopyTo(href[1..]);
		number.TryFormat(href[(1 + FootnoteAnchorPrefix.Length)..], out var formatted);

		Writer.OpenElement("sup");
		Writer.OpenElement("a");
		Writer.WriteAttribute("href", href[..(1 + FootnoteAnchorPrefix.Length + formatted)]);
		WriteNumber(number);
		Writer.CloseElement();
		Writer.CloseElement();
	}

	protected override void VisitEmptyLine(Node node)
	{
	}

	protected override void VisitUnorderedList(Node node)
	{
		Writer.OpenElement("ul");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitUnorderedListItem(Node node)
	{
		Writer.OpenElement("li");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitOrderedList(Node node)
	{
		Writer.OpenElement("ol");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitOrderedListItem(Node node)
	{
		Writer.OpenElement("li");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitTable(Node node)
	{
		Writer.OpenElement("table");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitTableRow(Node node)
	{
		Writer.OpenElement("tr");
		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitTableHeaderRow(Node node)
	{
		Writer.OpenElement("thead");
		isTableHeaderRow = true;
		base.VisitTableHeaderRow(node);
		isTableHeaderRow = false;
		Writer.CloseElement();
	}

	protected override void VisitTableFooterRow(Node node)
	{
		Writer.OpenElement("tfoot");
		base.VisitTableFooterRow(node);
		Writer.CloseElement();
	}

	protected override void VisitTableCell(Node node, TableCellAlignment? alignment)
	{
		Writer.OpenElement(isTableHeaderRow ? "th" : "td");

		if (alignment is not null)
		{
			Writer.WriteAttribute("align", alignment.Value switch
			{
				TableCellAlignment.Center => "center",
				TableCellAlignment.Right => "right",
				_ => "left"
			});
		}

		VisitInner(node);
		Writer.CloseElement();
	}

	protected override void VisitCheckbox(Node node, bool isChecked)
	{
		Writer.OpenElement("input");
		Writer.WriteAttribute("type", "checkbox");
		Writer.WriteAttribute("disabled");
		if (isChecked)
		{
			Writer.WriteAttribute("checked");
		}
		Writer.CloseElement();
	}


	/// <summary>
	/// Writes a number as text, which the encoder passes through unchanged.
	/// </summary>
	protected void WriteNumber(int value)
	{
		Span<char> formatted = stackalloc char[MaxInt32Length];
		value.TryFormat(formatted, out var length);
		Writer.WriteText(formatted[..length]);
	}

	protected void WriteTextFromMarkdown(ReadOnlySpan<char> span)
	{
		Span<char> markdownDecoded = span.Length > StackLimit ? new char[span.Length] : stackalloc char[span.Length];
		Decode(span, markdownDecoded, out var decodedLength);
		Writer.WriteText(markdownDecoded[..decodedLength]);
	}

	protected void WriteAttributeFromMarkdown(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
	{
		Span<char> markdownDecoded = value.Length > StackLimit ? new char[value.Length] : stackalloc char[value.Length];
		Decode(value, markdownDecoded, out var decodedLength);
		Writer.WriteAttribute(name, markdownDecoded[..decodedLength]);
	}

	/// <summary>
	/// Writes an attribute whose value is <paramref name="prefix"/> followed by <paramref name="span"/>
	/// decoded from markdown and encoded as a component of a URI.
	/// </summary>
	protected void WriteUriAttribute(ReadOnlySpan<char> name, ReadOnlySpan<char> prefix, ReadOnlySpan<char> span)
	{
		Span<char> markdownDecoded = span.Length > StackLimit ? new char[span.Length] : stackalloc char[span.Length];
		Decode(span, markdownDecoded, out var decodedLength);

		var requiredLength = prefix.Length + decodedLength * urlEncoder.MaxOutputCharactersPerInputCharacter;
		Span<char> value = requiredLength > StackLimit ? new char[requiredLength] : stackalloc char[requiredLength];
		prefix.CopyTo(value);
		urlEncoder.Encode(markdownDecoded[..decodedLength], value[prefix.Length..], out _, out var uriEncodedLength);

		Writer.WriteAttribute(name, value[..(prefix.Length + uriEncodedLength)]);
	}
}
