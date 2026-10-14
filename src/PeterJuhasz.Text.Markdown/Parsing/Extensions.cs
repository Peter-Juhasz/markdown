using Microsoft.Extensions.Primitives;

namespace PeterJuhasz.Text.Markdown.Parsing;

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

	/// <summary>
	/// Gets how far into its line a segment starts, which is measured on the underlying string, so that it is known
	/// even when the segment is cut out of a block which was trimmed, or which starts in the middle of a line.
	/// </summary>
	public static int GetColumn(this Segment segment)
	{
		var lineStartIndex = segment.Buffer.AsSpan(0, segment.Offset).LastIndexOf('\n') + 1;
		return segment.Offset - lineStartIndex;
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