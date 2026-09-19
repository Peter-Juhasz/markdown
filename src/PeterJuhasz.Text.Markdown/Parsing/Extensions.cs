using Microsoft.Extensions.Primitives;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

internal static partial class Extensions
{
	public static bool TryReadLine(this Segment segment, out Segment line)
	{
		if (segment.Length == 0)
		{
			line = default;
			return false;
		}

		var lineEndIndex = segment.IndexOf('\n') + 1;
		if (lineEndIndex == 0)
		{
			line = segment;
			return true;
		}

		line = segment.Subsegment(..lineEndIndex);
		return true;
	}

	public static int IndexOfNonEscaped(this Segment segment, char ch, int start)
	{
		while (start < segment.Length)
		{
			var index = segment.IndexOf(ch, start);
			if (index == -1)
			{
				return -1;
			}

			if (SyntaxFacts.IsEscaped(segment, index))
			{
				start = index + 1;
				continue;
			}

			return index;
		}

		return -1;
	}

	public static int IndexOfNonEscaped(this Segment segment, string ch, int start)
	{
		while (start < segment.Length)
		{
			var index = segment.IndexOf(ch, start, StringComparison.Ordinal);
			if (index == -1)
			{
				return -1;
			}

			if (SyntaxFacts.IsEscaped(segment, index))
			{
				start = index + 1;
				continue;
			}

			return index;
		}

		return -1;
	}
}