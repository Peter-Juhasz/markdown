using Microsoft.Extensions.Primitives;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	public readonly ref struct BlockParser(Segment document)
	{
		public Enumerator GetEnumerator() => new(document);

		public ref struct Enumerator(Segment document)
		{
			private Node _current;
			private int _processedIndex = 0;

			public readonly Node Current => _current;

			public bool MoveNext()
			{
				// reached end of document
				if (_processedIndex == document.Length)
				{
					_current = default;
					return false;
				}

				// find next line
				var nextLineEndingIndex = document.IndexOf('\n', _processedIndex);
				if (nextLineEndingIndex == -1)
				{
					nextLineEndingIndex = document.Length;
				}

				var line = document.Subsegment(_processedIndex..nextLineEndingIndex).Trim();
				var remaining = document.Subsegment(_processedIndex).Trim();

				// empty line
				if (line.Length == 0)
				{
					_current = new(NodeType.EmptyLine, line);
				}

				// heading
				else if (TryParseHeading(line, out var heading))
				{
					_current = heading;
				}

				// horizontal rule
				else if (TryParseHorizontalRule(line, out var hr))
				{
					_current = hr;
				}

				// unordered list
				else if (TryParseUnorderedList(remaining, out var ul))
				{
					_current = ul;
					_processedIndex = ul.FullSegment.Offset + ul.FullSegment.Length;
					return true;
				}

				// ordered list
				else if (TryParseOrderedList(remaining, out var ol))
				{
					_current = ol;
					_processedIndex = ol.FullSegment.Offset + ol.FullSegment.Length;
					return true;
				}

				// embed
				else if (TryParseEmbed(line, out var embed))
				{
					_current = embed;
				}

				// spoiler
				else if (TryParseSpoiler(remaining, out var s))
				{
					_current = s;
					_processedIndex = s.FullSegment.Offset + s.FullSegment.Length;
					return true;
				}

				// block quote
				else if (TryParseBlockQuote(remaining, out var bq))
				{
					_current = bq;
					_processedIndex = bq.FullSegment.Offset + bq.FullSegment.Length;
					return true;
				}

				// code block
				else if (TryParseCodeBlock(remaining, out var code))
				{
					_current = code;
					_processedIndex = code.FullSegment.Offset + code.FullSegment.Length;
					return true;
				}

				// paragraph
				else
				{
					_current = new(NodeType.Paragraph, line);
				}

				// move to next line
				if (nextLineEndingIndex == document.Length)
				{
					_processedIndex = document.Length;
				}
				else
				{
					_processedIndex = nextLineEndingIndex + 1;
				}
				return true;
			}
		}
	}

	internal static bool TryParseHeading(Segment line, out Node node)
	{
		if (line[0] != SyntaxFacts.Heading)
		{
			node = default;
			return false;
		}

		var depth = line.AsSpan().CommonPrefixLength("######");
		if (line.IndexSafe(depth) != ' ')
		{
			node = default;
			return false;
		}

		node = new(NodeType.Heading, line);
		return true;
	}

	internal static bool TryParseHorizontalRule(Segment line, out Node node)
	{
		if (!line.All(SyntaxFacts.HorizontalLine))
		{
			node = default;
			return false;
		}

		node = new(NodeType.HorizontalRule, line);
		return true;
	}

	internal static bool TryParseEmbed(Segment line, out Node node)
	{
		if (line[0] != SyntaxFacts.Embed)
		{
			node = default;
			return false;
		}

		if (line.PeekNextSafe() != SyntaxFacts.LinkUrlStartDelimiter)
		{
			node = default;
			return false;
		}

		var urlStartIndex = 1;
		var urlEndIndex = line.IndexOfNonEscaped(SyntaxFacts.LinkUrlEndDelimiter, urlStartIndex + 1);
		if (urlEndIndex == -1)
		{
			node = default;
			return false;
		}

		urlEndIndex += 1;
		var segment = line.Subsegment(..urlEndIndex);
		node = new(NodeType.Embed, segment);
		return true;
	}

	internal static bool TryParseUnorderedList(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.UnorderedListDash) && !document.StartsWith(SyntaxFacts.UnorderedListStar))
		{
			node = default;
			return false;
		}

		if (document.IndexSafe(1) != SyntaxFacts.Space)
		{
			node = default;
			return false;
		}

		var dashOrStar = document[0];
		var remaining = document;
		while (remaining.TryReadLine(out var nextLine))
		{
			var trimmed = nextLine.TrimStart();

			if (!trimmed.StartsWith(dashOrStar))
			{
				break;
			}

			if (trimmed.IndexSafe(1) != SyntaxFacts.Space)
			{
				break;
			}

			remaining = remaining.Subsegment(nextLine.Length);
		}

		var segment = document.Subsegment(..remaining.ToRelativeOffset(document));
		node = new(NodeType.UnorderedList, segment);
		return true;
	}

	internal static bool TryParseOrderedList(Segment document, out Node node)
	{
		if (!document.StartsWith("1. ", StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var remaining = document;
		var number = 0;
		while (remaining.TryReadLine(out var nextLine))
		{
			var trimmed = nextLine.TrimStart();

			var dotIndex = trimmed.IndexOf(SyntaxFacts.OrderedListDot);
			if (dotIndex == -1)
			{
				break;
			}

			if (!Int32.TryParse(trimmed.Subsegment(..dotIndex), out var n) || n != number + 1)
			{
				break;
			}

			number = n;
			var numberLength = Math.Max((int)Math.Ceiling(Math.Log10(number + 1)), 1);
			if (trimmed.IndexSafe(numberLength) != SyntaxFacts.OrderedListDot)
			{
				break;
			}

			remaining = remaining.Subsegment(nextLine.Length);
		}

		var segment = document.Subsegment(..remaining.ToRelativeOffset(document));
		node = new(NodeType.OrderedList, segment);
		return true;
	}

	internal static bool TryParseBlockQuote(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.BlockQuote))
		{
			node = default;
			return false;
		}

		var remaining = document;
		while (remaining.TryReadLine(out var nextLine))
		{
			var trimmed = nextLine.TrimStart();

			if (!trimmed.StartsWith(SyntaxFacts.BlockQuote))
			{
				break;
			}

			if (trimmed.IndexSafe(1) != SyntaxFacts.Space)
			{
				break;
			}

			remaining = remaining.Subsegment(nextLine.Length);
		}

		var segment = document.Subsegment(..remaining.ToRelativeOffset(document));
		node = new(NodeType.BlockQuote, segment);
		return true;
	}

	internal static bool TryParseSpoiler(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.Spoiler, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var remaining = document;
		while (remaining.TryReadLine(out var nextLine))
		{
			var trimmed = nextLine.TrimStart();

			if (!trimmed.StartsWith(SyntaxFacts.Spoiler, StringComparison.Ordinal))
			{
				break;
			}

			if (trimmed.IndexSafe(SyntaxFacts.Spoiler.Length) != SyntaxFacts.Space)
			{
				break;
			}

			remaining = remaining.Subsegment(nextLine.Length);
		}

		var segment = document.Subsegment(..remaining.ToRelativeOffset(document));
		node = new(NodeType.Spoiler, segment);
		return true;
	}

	internal static bool TryParseCodeBlock(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.CodeBlockDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var codeBlockEndIndex = document.IndexOf(SyntaxFacts.CodeBlockDelimiter, SyntaxFacts.CodeBlockDelimiter.Length + 1, StringComparison.Ordinal);
		if (codeBlockEndIndex == -1)
		{
			node = default;
			return false;
		}

		codeBlockEndIndex += SyntaxFacts.CodeBlockDelimiter.Length;
		var block = document.Subsegment(..codeBlockEndIndex);
		node = new(NodeType.CodeBlock, block);
		return true;
	}
}