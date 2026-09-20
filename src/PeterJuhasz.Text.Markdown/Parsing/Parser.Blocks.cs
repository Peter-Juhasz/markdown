using Microsoft.Extensions.Primitives;
using System.Diagnostics;

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

				// front matter, which is only recognized at the very beginning of the document
				else if (_processedIndex == 0 && TryParseFrontMatter(remaining, out var frontMatter))
				{
					return Consume(frontMatter);
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
					return Consume(ul);
				}

				// ordered list
				else if (TryParseOrderedList(remaining, out var ol))
				{
					return Consume(ol);
				}

				// embed
				else if (TryParseEmbed(line, out var embed))
				{
					_current = embed;
				}

				// spoiler
				else if (TryParseSpoiler(remaining, out var s))
				{
					return Consume(s);
				}

				// block quote
				else if (TryParseBlockQuote(remaining, out var bq))
				{
					return Consume(bq);
				}

				// table
				else if (TryParseTableBlock(remaining, out var table))
				{
					return Consume(table);
				}

				// code block
				else if (TryParseCodeBlock(remaining, out var code))
				{
					return Consume(code);
				}

				// math block
				else if (TryParseMathBlock(remaining, out var math))
				{
					return Consume(math);
				}

				// single line math block
				else if (TryParseSingleLineMathBlock(line, out var singleLineMath))
				{
					_current = singleLineMath;
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

			/// <summary>
			/// Accepts a block which may span multiple lines, and continues the enumeration right after it.
			/// </summary>
			private bool Consume(Node node)
			{
				// the block is measured relative to the document, which may be a segment of a larger string
				var endIndex = node.FullSegment.ToRelativeOffset(document) + node.FullSegment.Length;

				// a block always consumes at least one character, otherwise the enumeration would never terminate
				Debug.Assert(endIndex > _processedIndex);

				_current = node;
				_processedIndex = endIndex;
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

	private const string FrontMatterEndDelimiter = "\n" + SyntaxFacts.FrontMatterDelimiter;

	/// <summary>
	/// Parses a front matter block, which is a fenced block of metadata at the very beginning of a document.
	/// </summary>
	/// <remarks>
	/// The contents are not parsed at all, they are handed over as they are written.
	/// </remarks>
	internal static bool TryParseFrontMatter(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.FrontMatterDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		// opening delimiter must be on its own line
		var firstLineEndIndex = document.IndexOf('\n');
		if (firstLineEndIndex == -1 || !document.AsSpan()[SyntaxFacts.FrontMatterDelimiter.Length..firstLineEndIndex].IsWhiteSpace())
		{
			node = default;
			return false;
		}

		var frontMatterEndIndex = document.AsSpan(firstLineEndIndex).IndexOf(FrontMatterEndDelimiter, StringComparison.Ordinal);
		if (frontMatterEndIndex == -1)
		{
			node = default;
			return false;
		}
		frontMatterEndIndex += firstLineEndIndex; // account for opening line

		var block = document.Subsegment(..(frontMatterEndIndex + FrontMatterEndDelimiter.Length));
		node = new(NodeType.FrontMatter, block);
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

		if (segment.AsSpan(2).IndexOf(SyntaxFacts.EmbedSchemeDelimiter, StringComparison.Ordinal) == -1)
		{
			node = default;
			return false;
		}

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

		if (document.IndexSafe(1) != SyntaxFacts.Space)
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

		if (document.IndexSafe(SyntaxFacts.Spoiler.Length) != SyntaxFacts.Space)
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

	internal static bool TryParseTableBlock(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.TableCellDelimiter))
		{
			node = default;
			return false;
		}

		// the first row must have content, so that a lone delimiter is not a table
		var firstLineEndIndex = document.IndexOf('\n');
		if (firstLineEndIndex == -1)
		{
			firstLineEndIndex = document.Length;
		}

		if (document.AsSpan()[1..firstLineEndIndex].IsWhiteSpace())
		{
			node = default;
			return false;
		}

		var remaining = document;
		while (remaining.TryReadLine(out var nextLine))
		{
			var trimmed = nextLine.TrimStart();

			if (!trimmed.StartsWith(SyntaxFacts.TableCellDelimiter))
			{
				break;
			}

			remaining = remaining.Subsegment(nextLine.Length);
		}

		var segment = document.Subsegment(..remaining.ToRelativeOffset(document));
		node = new(NodeType.TableBlock, segment);
		return true;
	}

	private const int MaxCodeBlockTickCount = 10;

	internal static bool TryParseCodeBlock(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.CodeBlockDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var tickCount = document.AsSpan().IndexOfAnyExcept('`');
		if (tickCount is -1 or > MaxCodeBlockTickCount)
		{
			node = default;
			return false;
		}

		Span<char> endDelimiter = stackalloc char[1 + tickCount];
		endDelimiter.Fill(SyntaxFacts.InlineCodeDelimiter);
		endDelimiter[0] = '\n';

		var codeBlockEndIndex = document.AsSpan(tickCount).IndexOf(endDelimiter, StringComparison.Ordinal);
		if (codeBlockEndIndex == -1)
		{
			node = default;
			return false;
		}
		codeBlockEndIndex += tickCount; // account for start ticks

		var block = document.Subsegment(..(codeBlockEndIndex + 1 + tickCount));
		node = new(NodeType.CodeBlock, block);
		return true;
	}

	private const string MathBlockEndDelimiter = "\n" + SyntaxFacts.MathBlockDelimiter;

	internal static bool TryParseMathBlock(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.MathBlockDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		// opening delimiter must be on its own line
		var firstLineEndIndex = document.IndexOf('\n');
		if (firstLineEndIndex == -1 || !document.AsSpan()[SyntaxFacts.MathBlockDelimiter.Length..firstLineEndIndex].IsWhiteSpace())
		{
			node = default;
			return false;
		}

		var mathBlockEndIndex = document.AsSpan(firstLineEndIndex).IndexOf(MathBlockEndDelimiter, StringComparison.Ordinal);
		if (mathBlockEndIndex == -1)
		{
			node = default;
			return false;
		}
		mathBlockEndIndex += firstLineEndIndex; // account for opening line

		var block = document.Subsegment(..(mathBlockEndIndex + MathBlockEndDelimiter.Length));
		node = new(NodeType.MathBlock, block);
		return true;
	}

	internal static bool TryParseSingleLineMathBlock(Segment line, out Node node)
	{
		if (!line.StartsWith(SyntaxFacts.MathBlockDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		// closing delimiter must be at the end of the line
		var endIndex = line.IndexOfNonEscaped(SyntaxFacts.MathBlockDelimiter, SyntaxFacts.MathBlockDelimiter.Length);
		if (endIndex != line.Length - SyntaxFacts.MathBlockDelimiter.Length)
		{
			node = default;
			return false;
		}

		// must not be empty
		if (line.AsSpan()[SyntaxFacts.MathBlockDelimiter.Length..endIndex].IsWhiteSpace())
		{
			node = default;
			return false;
		}

		node = new(NodeType.MathBlock, line);
		return true;
	}
}