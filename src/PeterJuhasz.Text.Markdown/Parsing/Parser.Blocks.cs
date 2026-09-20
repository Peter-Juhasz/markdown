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

			/// <summary>
			/// Everything which is not processed yet, which is what a block spanning multiple lines is measured from.
			/// </summary>
			private readonly Segment Remaining => document.Subsegment(_processedIndex).Trim();

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

				// empty line
				if (line.Length == 0)
				{
					_current = new(NodeType.EmptyLine, line);
				}

				// the first character of the line already rules out every block kind but a few,
				// and the blocks which may span multiple lines are measured from the whole remaining document
				else
				{
					switch (line[0])
					{
						// heading
						case SyntaxFacts.Heading when TryParseHeading(line, out var heading):
							_current = heading;
							break;

						// front matter, whose fence is built of the same character as a horizontal rule,
						// and which is only recognized at the very beginning of the document
						case SyntaxFacts.HorizontalLine when _processedIndex == 0 && TryParseFrontMatter(Remaining, out var frontMatter):
							return Consume(frontMatter);

						// horizontal rule
						case SyntaxFacts.HorizontalLine when TryParseHorizontalRule(line, out var hr):
							_current = hr;
							break;

						// unordered list
						case SyntaxFacts.UnorderedListDash or SyntaxFacts.UnorderedListStar when TryParseUnorderedList(Remaining, out var ul):
							return Consume(ul);

						// ordered list, which always starts at one
						case '1' when TryParseOrderedList(Remaining, out var ol):
							return Consume(ol);

						// embed
						case SyntaxFacts.Embed when TryParseEmbed(line, out var embed):
							_current = embed;
							break;

						// spoiler
						case SyntaxFacts.BlockQuote when TryParseSpoiler(Remaining, out var s):
							return Consume(s);

						// alert, which is a block quote whose first line declares its type
						case SyntaxFacts.BlockQuote when TryParseAlert(Remaining, out var alert):
							return Consume(alert);

						// block quote
						case SyntaxFacts.BlockQuote when TryParseBlockQuote(Remaining, out var bq):
							return Consume(bq);

						// comment, which may span multiple lines
						case SyntaxFacts.Comment when TryParseComment(Remaining, out var comment):
							return Consume(comment);

						// table
						case SyntaxFacts.TableCellDelimiter when TryParseTableBlock(Remaining, out var table):
							return Consume(table);

						// code block
						case SyntaxFacts.InlineCodeDelimiter when TryParseCodeBlock(Remaining, out var code):
							return Consume(code);

						// math block
						case SyntaxFacts.InlineMathDelimiter when TryParseMathBlock(Remaining, out var math):
							return Consume(math);

						// single line math block
						case SyntaxFacts.InlineMathDelimiter when TryParseSingleLineMathBlock(line, out var singleLineMath):
							_current = singleLineMath;
							break;

						// paragraph
						default:
							_current = new(NodeType.Paragraph, line);
							break;
					}
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

	/// <summary>
	/// Parses an alert, which is a block quote whose first line declares nothing but its type, like <c>&gt; [!NOTE]</c>.
	/// </summary>
	/// <remarks>
	/// Any type is accepted, so a document is not held to the set of types the renderer happens to know.
	/// </remarks>
	internal static bool TryParseAlert(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.AlertStartDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var firstLineEndIndex = document.IndexOf('\n');
		if (firstLineEndIndex == -1)
		{
			firstLineEndIndex = document.Length;
		}

		// the type is closed on the line it is opened on, and it is not empty
		var typeStartIndex = SyntaxFacts.AlertStartDelimiter.Length;
		var typeEndIndex = document.IndexOf(SyntaxFacts.AlertEndDelimiter, typeStartIndex);
		if (typeEndIndex is -1 || typeEndIndex >= firstLineEndIndex || typeEndIndex == typeStartIndex)
		{
			node = default;
			return false;
		}

		// the type is a single word, and the marker is the only thing on its line
		if (document.AsSpan(typeStartIndex, typeEndIndex - typeStartIndex).ContainsAny(SyntaxFacts.Space, SyntaxFacts.Tab) ||
			!document.AsSpan()[(typeEndIndex + 1)..firstLineEndIndex].IsWhiteSpace())
		{
			node = default;
			return false;
		}

		// the body is quoted line by line, just like a block quote
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
		node = new(NodeType.Alert, segment);
		return true;
	}

	/// <summary>
	/// Parses a comment, which carries nothing for the reader, like <c>&lt;!-- remark --&gt;</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A comment may span multiple lines, and whatever follows the line it is closed on is parsed as its own block.
	/// </para>
	/// <para>
	/// A comment which is never closed runs to the end of the document, just like an HTML one does,
	/// so that what an author meant to hide is never rendered.
	/// </para>
	/// </remarks>
	internal static bool TryParseComment(Segment document, out Node node)
	{
		if (!document.StartsWith(SyntaxFacts.CommentStartDelimiter, StringComparison.Ordinal))
		{
			node = default;
			return false;
		}

		var contentStartIndex = SyntaxFacts.CommentStartDelimiter.Length;
		var commentEndIndex = document.AsSpan(contentStartIndex).IndexOf(SyntaxFacts.CommentEndDelimiter, StringComparison.Ordinal);

		var segment = commentEndIndex == -1
			? document
			: document.Subsegment(..(contentStartIndex + commentEndIndex + SyntaxFacts.CommentEndDelimiter.Length));

		node = new(NodeType.Comment, segment);
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