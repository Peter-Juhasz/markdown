using Microsoft.Extensions.Primitives;

namespace PeterJuhasz.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	public readonly ref struct OrderedListParser(Segment document)
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

				// read next line
				var dotIndex = line.IndexOf(SyntaxFacts.OrderedListDot);
				if (dotIndex == -1)
				{
					_current = default;
					return false;
				}

				if (!Int32.TryParse(line.Subsegment(..dotIndex), out _))
				{
					_current = default;
					return false;
				}

				_current = new(NodeType.OrderedListItem, line);
				_processedIndex = wholeLine.ToRelativeOffset(document) + wholeLine.Length;
				return true;
			}
		}
	}
}