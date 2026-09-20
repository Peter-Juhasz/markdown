using Microsoft.Extensions.Primitives;
using System.Buffers;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	public readonly ref struct TableParser(Segment document)
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
				if (!document.Subsegment(_processedIndex).TryReadLine(out var wholeLine))
				{
					_current = default;
					return false;
				}

				var line = wholeLine.Trim();
				if (!line.StartsWith(SyntaxFacts.TableCellDelimiter))
				{
					_current = default;
					return false;
				}

				_current = new(NodeType.TableRow, line);
				_processedIndex = wholeLine.ToRelativeOffset(document) + wholeLine.Length;
				return true;
			}
		}
	}

	public readonly ref struct TableRowParser(Segment row)
	{
		public Enumerator GetEnumerator() => new(row);

		public ref struct Enumerator(Segment row)
		{
			private Node _current;
			private int _processedIndex = 1; // skip the opening delimiter

			public readonly Node Current => _current;

			public bool MoveNext()
			{
				// reached end of row
				if (_processedIndex >= row.Length)
				{
					_current = default;
					return false;
				}

				// find the closing delimiter, which may be missing for the last cell
				var endIndex = row.IndexOfNonEscaped(SyntaxFacts.TableCellDelimiter, _processedIndex);
				var isClosed = endIndex != -1;
				if (!isClosed)
				{
					endIndex = row.Length;
				}

				var segment = row.Subsegment(_processedIndex..endIndex).Trim();
				_processedIndex = endIndex + 1;

				// trailing whitespace after the last cell is not a cell on its own
				if (!isClosed && segment.Length == 0)
				{
					_current = default;
					return false;
				}

				_current = new(NodeType.TableCell, segment);
				return true;
			}
		}
	}

	private static readonly SearchValues<char> TableSeparatorCharacters = SearchValues.Create([
		SyntaxFacts.TableSeparator,
		SyntaxFacts.TableCellDelimiter,
		SyntaxFacts.TableAlignment,

		SyntaxFacts.Space,
		SyntaxFacts.Tab,
	]);

	/// <summary>
	/// Determines whether a row separates the header or the footer from the body, like <c>| --- | --- |</c>.
	/// </summary>
	internal static bool IsTableSeparatorRow(Segment row)
	{
		var span = row.AsSpan();
		return !span.ContainsAnyExcept(TableSeparatorCharacters) && span.Contains(SyntaxFacts.TableSeparator);
	}
}
