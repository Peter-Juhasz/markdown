using Microsoft.Extensions.Primitives;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public abstract partial class InplaceMarkdownVisitor
{
	public void VisitDocument(string document) => Visit(new Node(NodeType.Document, document));


	protected virtual void VisitDocument(Node node) => VisitInner(node);

	protected virtual void VisitHeading(Node node, int level) => VisitInner(node);

	protected virtual void VisitParagraph(Node node) => VisitInner(node);

	protected virtual void VisitBlockQuote(Node node) => VisitInner(node);

	protected virtual void VisitSpoiler(Node node) => VisitInner(node);

	protected virtual void VisitAlert(Node node, Segment type) => VisitInner(node);

	protected virtual void VisitDetails(Node node) => VisitInner(node);

	protected virtual void VisitDetailsSummary(Node node) => VisitInner(node);

	protected virtual void VisitFigure(Node node) => VisitInner(node);

	protected virtual void VisitFigureCaption(Node node) => VisitInner(node);

	protected virtual void VisitBold(Node node) => VisitInner(node);

	protected virtual void VisitItalic(Node node) => VisitInner(node);

	protected virtual void VisitUnderline(Node node) => VisitInner(node);

	protected virtual void VisitStrikethrough(Node node) => VisitInner(node);

	protected abstract void VisitEmailAddress(Node node, Segment emailAddress);

	protected abstract void VisitPhoneNumber(Node node, Segment phoneNumber);

	protected abstract void VisitUrl(Node node, Segment url);

	protected virtual void VisitAngleBracketUrl(Node node, Segment url) => VisitUrl(node, url);

	protected virtual void VisitLink(Node node, Segment url, Segment title) => VisitInner(node);

	protected abstract void VisitEmbed(Node node, Segment scheme, Segment id);

	protected abstract void VisitText(Node node, Segment text);

	protected abstract void VisitEmojiAlias(Node node, Segment alias);

	protected abstract void VisitEmojiSmiley(Node node, Segment smiley);

	protected abstract void VisitMention(Node node, Segment userName);

	protected abstract void VisitHashtag(Node node, Segment tag);

	protected abstract void VisitInlineCode(Node node, Segment code);

	protected abstract void VisitCodeBlock(Node node, Segment code, Segment language);

	protected abstract void VisitInlineMath(Node node, Segment math);

	protected abstract void VisitMathBlock(Node node, Segment math);

	protected abstract void VisitFrontMatter(Node node, Segment frontMatter);

	protected virtual void VisitComment(Node node, Segment comment) { }

	protected virtual void VisitInlineComment(Node node, Segment comment) { }

	protected virtual void VisitFootnoteContent(Node node, int number) => VisitInner(node);

	protected abstract void VisitFootnoteReference(Node node, int number);

	protected abstract void VisitHorizontalRule(Node node);

	protected abstract void VisitEmptyLine(Node node);

	protected virtual void VisitUnorderedList(Node node) => VisitInner(node);

	protected virtual void VisitUnorderedListItem(Node node) => VisitInner(node);

	protected virtual void VisitOrderedList(Node node) => VisitInner(node);

	protected virtual void VisitOrderedListItem(Node node) => VisitInner(node);

	protected abstract void VisitCheckbox(Node node, bool isChecked);

	protected virtual void VisitTable(Node node) => VisitInner(node);

	protected virtual void VisitTableRow(Node node) => VisitInner(node);

	protected virtual void VisitTableHeaderRow(Node node) => VisitTableRow(node);

	protected virtual void VisitTableFooterRow(Node node) => VisitTableRow(node);

	protected virtual void VisitTableCell(Node node, TableCellAlignment? alignment) => VisitInner(node);


	private NodeType _disallowedNodeTypes = default;
	private TableCellAlignment?[]? _tableColumnAlignments;

	private void Visit(Node node)
	{
		switch (node.Type)
		{
			case NodeType.Document:
				VisitDocument(node);
				break;

			case NodeType.Heading:
				VisitHeading(node, node.GetHeadingLevel());
				break;

			case NodeType.BlockQuote:
				VisitBlockQuote(node);
				break;

			case NodeType.Paragraph:
				VisitParagraph(node);
				break;

			case NodeType.EmptyLine:
				VisitEmptyLine(node);
				break;

			case NodeType.Bold:
				VisitBold(node);
				break;

			case NodeType.Italic:
				VisitItalic(node);
				break;

			case NodeType.Underline:
				VisitUnderline(node);
				break;

			case NodeType.Strikethrough:
				VisitStrikethrough(node);
				break;

			case NodeType.EmailAddress:
				VisitEmailAddress(node, node.FullSegment);
				break;

			case NodeType.PhoneNumber:
				VisitPhoneNumber(node, node.FullSegment);
				break;

			case NodeType.Url:
				VisitUrl(node, node.FullSegment);
				break;

			case NodeType.AngleBracketUrl:
				VisitAngleBracketUrl(node, node.GetAngleBracketUrl());
				break;

			case NodeType.Link:
				{
					node.GetLinkTarget(out var url, out var title);
					VisitLink(node, url, title);
					break;
				}

			case NodeType.Embed:
				VisitEmbed(node, node.GetEmbedScheme(), node.GetEmbedId());
				break;

			case NodeType.Mention:
				VisitMention(node, node.GetMentionedUser());
				break;

			case NodeType.Hashtag:
				VisitHashtag(node, node.GetHashtag());
				break;

			case NodeType.EmojiAlias:
				VisitEmojiAlias(node, node.GetEmojiAlias());
				break;

			case NodeType.Text:
				VisitText(node, node.FullSegment);
				break;

			case NodeType.InlineCode:
				VisitInlineCode(node, node.GetInlineCode());
				break;

			case NodeType.CodeBlock:
				VisitCodeBlock(node, node.GetCode(), node.GetCodeLanguage());
				break;

			case NodeType.InlineMath:
				VisitInlineMath(node, node.GetInlineMath());
				break;

			case NodeType.MathBlock:
				VisitMathBlock(node, node.GetMath());
				break;

			case NodeType.FrontMatter:
				VisitFrontMatter(node, node.GetFrontMatter());
				break;

			case NodeType.Comment:
				VisitComment(node, node.GetComment());
				break;

			case NodeType.InlineComment:
				VisitInlineComment(node, node.GetComment());
				break;

			case NodeType.FootnoteContent:
				VisitFootnoteContent(node, node.GetFootnoteNumber());
				break;

			case NodeType.FootnoteReference:
				VisitFootnoteReference(node, node.GetFootnoteNumber());
				break;

			case NodeType.HorizontalRule:
				VisitHorizontalRule(node);
				break;

			case NodeType.EmojiSmiley:
				VisitEmojiSmiley(node, node.FullSegment);
				break;

			case NodeType.UnorderedList:
				VisitUnorderedList(node);
				break;

			case NodeType.UnorderedListItem:
				VisitUnorderedListItem(node);
				break;

			case NodeType.OrderedList:
				VisitOrderedList(node);
				break;

			case NodeType.OrderedListItem:
				VisitOrderedListItem(node);
				break;

			case NodeType.Checkbox:
				VisitCheckbox(node, node.GetCheckboxState());
				break;

			case NodeType.Spoiler:
				VisitSpoiler(node);
				break;

			case NodeType.Alert:
				VisitAlert(node, node.GetAlertType());
				break;

			case NodeType.Details:
				VisitDetails(node);
				break;

			case NodeType.Figure:
				VisitFigure(node);
				break;

			case NodeType.TableBlock:
				VisitTable(node);
				break;

			default:
				throw new NotSupportedException();
		}
	}


	protected void VisitInner(Node node)
	{
		switch (node.Type)
		{
			case NodeType.Document:
				{
					foreach (var block in Parser.ParseDocument(node.FullSegment))
					{
						Visit(block);
					}
					return;
				}

			case NodeType.UnorderedList:
				{
					foreach (var listItem in Parser.ParseUnorderedListItems(node.FullSegment))
					{
						Visit(listItem);
					}
					return;
				}

			case NodeType.OrderedList:
				{
					foreach (var listItem in Parser.ParseOrderedListItems(node.FullSegment))
					{
						Visit(listItem);
					}
					return;
				}

			case NodeType.BlockQuote:
				{
					foreach (var block in Parser.ParseBlockQuote(node.FullSegment))
					{
						Visit(block);
					}
					return;
				}

			case NodeType.Spoiler:
				{
					foreach (var block in Parser.ParseSpoiler(node.FullSegment))
					{
						Visit(block);
					}
					return;
				}

			case NodeType.Alert:
				{
					// the body below the marker line is quoted just like a block quote is
					foreach (var block in Parser.ParseBlockQuote(node.GetAlertContent()))
					{
						Visit(block);
					}
					return;
				}

			case NodeType.Details:
				{
					// the summary comes first, and is the only part of the block which is not written as blocks
					if (node.TryGetDetailsSummary(out var summary))
					{
						VisitDetailsSummary(new Node(NodeType.DetailsSummary, summary));
					}

					foreach (var block in Parser.ParseDocument(node.GetDetailsContent()))
					{
						Visit(block);
					}
					return;
				}

			case NodeType.Figure:
				{
					// the caption comes last, and is the only part of the figure which is not written as blocks
					foreach (var block in Parser.ParseDocument(node.GetFigureContent()))
					{
						Visit(block);
					}

					if (node.TryGetFigureCaption(out var caption))
					{
						VisitFigureCaption(new Node(NodeType.FigureCaption, caption));
					}
					return;
				}

			case NodeType.TableBlock:
				{
					VisitTableRows(node.FullSegment);
					return;
				}

			case NodeType.TableRow:
				{
					var alignments = _tableColumnAlignments;
					var column = 0;
					foreach (var cell in Parser.ParseTableCells(node.FullSegment))
					{
						// a row may have more cells than the separator row declared alignments for
						VisitTableCell(cell, alignments is not null && column < alignments.Length ? alignments[column] : null);
						column++;
					}
					return;
				}
		}

		var disallow = node.Type switch
		{
			NodeType.Bold => NodeType.Bold,
			NodeType.Italic => NodeType.Italic,
			NodeType.Underline => NodeType.Underline,
			NodeType.Strikethrough => NodeType.Strikethrough,
			NodeType.Link => NodeType.Link | NodeType.Url | NodeType.AngleBracketUrl | NodeType.EmailAddress | NodeType.PhoneNumber | NodeType.Mention | NodeType.Hashtag,
			_ => default
		};
		_disallowedNodeTypes |= disallow;
		foreach (var block in Parser.ParseInline(
			inline: node.Type switch
			{
				NodeType.Paragraph => node.FullSegment,
				NodeType.Heading => node.GetHeadingContent(),
				NodeType.Bold => node.GetBoldContent(),
				NodeType.Italic => node.GetItalicContent(),
				NodeType.Underline => node.GetUnderlineContent(),
				NodeType.Strikethrough => node.GetStrikethroughContent(),
				NodeType.Link => node.GetLinkContent(),
				NodeType.UnorderedListItem => node.GetUnorderedListItemContent(),
				NodeType.OrderedListItem => node.GetOrderedListItemContent(),
				NodeType.FootnoteContent => node.GetFootnoteContent(),
				NodeType.DetailsSummary => node.FullSegment,
				NodeType.FigureCaption => node.FullSegment,
				NodeType.TableCell => node.FullSegment,
				_ => throw new NotSupportedException(),
			},
			parentNode: node.Type,
			disallowedNodeTypes: _disallowedNodeTypes
		))
		{
			Visit(block);
		}
		_disallowedNodeTypes &= ~disallow;
	}

	private void VisitTableRows(Segment table)
	{
		// the first row is the header if it is followed by a separator row,
		// and the last row is the footer if it is preceded by one, which is only known
		// once the whole table is read, so the rows are collected instead of parsed twice
		using var rows = new PooledArrayBuilder<Segment>();
		var secondIsSeparator = false;
		var lastIsSeparator = false;
		var beforeLastIsSeparator = false;

		foreach (var row in Parser.ParseTableRows(table))
		{
			var isSeparator = Parser.IsTableSeparatorRow(row.FullSegment);

			if (rows.Count == 1)
			{
				secondIsSeparator = isSeparator;
			}

			beforeLastIsSeparator = lastIsSeparator;
			lastIsSeparator = isSeparator;
			rows.Add(row.FullSegment);
		}

		var count = rows.Count;
		var hasHeader = count >= 2 && secondIsSeparator;
		var footerSeparatorIndex = count - 2;
		var hasFooter = count >= 3 && beforeLastIsSeparator && !(hasHeader && footerSeparatorIndex == 1);

		// columns are aligned by the separator row which follows the header
		var previousAlignments = _tableColumnAlignments;
		_tableColumnAlignments = hasHeader ? ParseTableColumnAlignments(rows[1]) : null;

		for (var index = 0; index < count; index++)
		{
			// skip separator rows
			if ((hasHeader && index == 1) || (hasFooter && index == footerSeparatorIndex))
			{
				continue;
			}

			var row = new Node(NodeType.TableRow, rows[index]);

			if (hasHeader && index == 0)
			{
				VisitTableHeaderRow(row);
			}
			else if (hasFooter && index == count - 1)
			{
				VisitTableFooterRow(row);
			}
			else
			{
				VisitTableRow(row);
			}
		}

		_tableColumnAlignments = previousAlignments;
	}

	private static TableCellAlignment?[]? ParseTableColumnAlignments(Segment separatorRow)
	{
		using var alignments = new PooledArrayBuilder<TableCellAlignment?>();

		var hasAlignment = false;
		
		foreach (var cell in Parser.ParseTableCells(separatorRow))
		{
			var alignment = cell.GetTableCellAlignment();
			hasAlignment |= alignment is not null;
			alignments.Add(alignment);
		}

		// most separator rows declare no alignment at all
		if (!hasAlignment)
		{
			return null;
		}

		return alignments.ToArray();
	}

	protected static void Decode(ReadOnlySpan<char> encoded, Span<char> text, out int written)
	{
		// shortcut if there are no escapes
		if (!SyntaxFacts.HasAnyEscaped(encoded, out var firstEscapeIndex))
		{
			encoded.CopyTo(text);
			written = encoded.Length;
			return;
		}

		Decode(encoded, firstEscapeIndex, text, out written);
	}

	/// <summary>
	/// Decodes escapes, starting from the first one, which the caller has already located.
	/// </summary>
	private static void Decode(ReadOnlySpan<char> encoded, int firstEscapeIndex, Span<char> text, out int written)
	{
		written = 0;
		var processed = 0;
		var nextEscapeIndex = firstEscapeIndex;

		while (true)
		{
			var remaining = encoded[processed..];

			// no more escapes, or a trailing escape character which is kept as literal
			if (nextEscapeIndex == -1 || nextEscapeIndex == remaining.Length - 1)
			{
				remaining.CopyTo(text[written..]);
				written += remaining.Length;
				return;
			}

			remaining[..nextEscapeIndex].CopyTo(text[written..]);
			written += nextEscapeIndex;
			text[written] = remaining[nextEscapeIndex + 1];
			written++;
			processed += nextEscapeIndex + 2;

			if (processed == encoded.Length)
			{
				return;
			}

			nextEscapeIndex = encoded[processed..].IndexOf(SyntaxFacts.Escape);
		}
	}

	protected static int GetDecodedLength(ReadOnlySpan<char> encoded)
	{
		// shortcut if there are no escapes
		if (!SyntaxFacts.HasAnyEscaped(encoded, out var firstEscapeIndex))
		{
			return encoded.Length;
		}

		return GetDecodedLength(encoded, firstEscapeIndex);
	}

	/// <summary>
	/// Counts the decoded length, starting from the first escape, which the caller has already located.
	/// </summary>
	private static int GetDecodedLength(ReadOnlySpan<char> encoded, int firstEscapeIndex)
	{
		var encodedCount = 0;
		var processed = 0;
		var nextEscapeIndex = firstEscapeIndex;

		while (true)
		{
			var remaining = encoded[processed..];

			// no more escapes, or a trailing escape character which is kept as literal
			if (nextEscapeIndex == -1 || nextEscapeIndex == remaining.Length - 1)
			{
				break;
			}

			encodedCount++;
			processed += nextEscapeIndex + 2;

			if (processed == encoded.Length)
			{
				break;
			}

			nextEscapeIndex = encoded[processed..].IndexOf(SyntaxFacts.Escape);
		}

		return encoded.Length - encodedCount;
	}

	protected static string Decode(Segment segment)
	{
		// most runs carry no escape at all, and are handed over as they are written
		if (!SyntaxFacts.HasAnyEscaped(segment.AsSpan(), out var firstEscapeIndex))
		{
			return segment.HasValue ? segment.Value : String.Empty;
		}

		// the state is passed to the callback, so that it captures nothing
		var decodedLength = GetDecodedLength(segment.AsSpan(), firstEscapeIndex);
		return String.Create(
			decodedLength,
			(segment, firstEscapeIndex),
			static (buffer, state) => Decode(state.segment.AsSpan(), state.firstEscapeIndex, buffer, out _)
		);
	}
}
