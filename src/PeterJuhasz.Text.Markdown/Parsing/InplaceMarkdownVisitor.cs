using Microsoft.Extensions.Primitives;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public abstract partial class InplaceMarkdownVisitor
{
	public void VisitDocument(string document) => Visit(new Node(NodeType.Document, document));


	protected virtual void VisitDocument(Node node) => VisitInner(node);

	protected abstract void VisitHeading(Node node, int level);

	protected abstract void VisitParagraph(Node node);

	protected abstract void VisitBlockQuote(Node node);

	protected abstract void VisitSpoiler(Node node);

	protected abstract void VisitBold(Node node);

	protected abstract void VisitItalic(Node node);

	protected abstract void VisitUnderline(Node node);

	protected abstract void VisitStrikethrough(Node node);

	protected abstract void VisitEmailAddress(Node node, Segment emailAddress);

	protected abstract void VisitPhoneNumber(Node node, Segment phoneNumber);

	protected abstract void VisitUrl(Node node, Segment url);

	protected abstract void VisitLink(Node node, Segment url);

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

	protected abstract void VisitHorizontalRule(Node node);

	protected abstract void VisitEmptyLine(Node node);

	protected abstract void VisitUnorderedList(Node node);

	protected abstract void VisitUnorderedListItem(Node node);

	protected abstract void VisitOrderedList(Node node);

	protected abstract void VisitOrderedListItem(Node node);

	protected abstract void VisitCheckbox(Node node, bool isChecked);


	private NodeType _disallowedNodeTypes = default;

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

			case NodeType.Link:
				VisitLink(node, node.GetLinkUrl());
				break;

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
		}

		var disallow = node.Type switch
		{
			NodeType.Bold => NodeType.Bold,
			NodeType.Italic => NodeType.Italic,
			NodeType.Underline => NodeType.Underline,
			NodeType.Strikethrough => NodeType.Strikethrough,
			NodeType.Link => NodeType.Link | NodeType.Url | NodeType.EmailAddress | NodeType.PhoneNumber | NodeType.Mention | NodeType.Hashtag,
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

	protected static void Decode(ReadOnlySpan<char> encoded, Span<char> text, out int written)
	{
		// shortcut if there are no escapes
		if (!SyntaxFacts.HasAnyEscaped(encoded, out var firstEscapeIndex))
		{
			encoded.CopyTo(text);
			written = encoded.Length;
			return;
		}

		// decode escapes
		encoded[..firstEscapeIndex].CopyTo(text);
		written = firstEscapeIndex;
		text[written] = encoded[firstEscapeIndex + 1];
		written++;
		var processed = firstEscapeIndex + 2;

		while (processed < encoded.Length)
		{
			var remaining = encoded[processed..];
			var nextEscapeIndex = remaining.IndexOf(SyntaxFacts.Escape);
			if (nextEscapeIndex == -1)
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
		}
	}

	protected static int GetDecodedLength(ReadOnlySpan<char> encoded)
	{
		// shortcut if there are no escapes
		if (!SyntaxFacts.HasAnyEscaped(encoded, out var firstEscapeIndex))
		{
			return encoded.Length;
		}

		// count escapes
		var encodedCount = 1;
		var processed = firstEscapeIndex + 2;

		while (processed < encoded.Length)
		{
			var remaining = encoded[processed..];
			var nextEscapeIndex = remaining.IndexOf(SyntaxFacts.Escape);
			if (nextEscapeIndex == -1)
			{
				break;
			}

			encodedCount++;
			processed += nextEscapeIndex + 2;
		}

		return encoded.Length - encodedCount;
	}

	protected static string Decode(Segment segment)
	{
		var decodedLength = GetDecodedLength(segment);
		return string.Create<object?>(decodedLength, null, (buffer, state) => Decode(segment.AsSpan(), buffer, out _));
	}
}
