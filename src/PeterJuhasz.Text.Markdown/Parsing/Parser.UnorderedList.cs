using Microsoft.Extensions.Primitives;

namespace PeterJuhasz.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	public readonly ref struct UnorderedListParser(Segment document)
	{
		public Enumerator GetEnumerator() => new(document);

		public ref struct Enumerator(Segment document)
		{
			private Node _current;
			private int _processedIndex = 0;
			private char _dashOrStar = default;

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

				if (line.Length == 0)
				{
					_current = default;
					return false;
				}

				// set delimiter
				if (_dashOrStar == default)
				{
					_dashOrStar = line[0];

					if (_dashOrStar is not (SyntaxFacts.UnorderedListDash or SyntaxFacts.UnorderedListStar))
					{
						_current = default;
						return false;
					}
				}

				// read next line
				if (!line.StartsWith(_dashOrStar))
				{
					_current = default;
					return false;
				}

				_current = new(NodeType.UnorderedListItem, line);
				_processedIndex = wholeLine.ToRelativeOffset(document) + wholeLine.Length;
				return true;
			}
		}
	}
}