using System.Buffers;

namespace PeterJuhasz.Text.Markdown.Benchmarks.LinkText;

/// <summary>
/// Candidate implementations which find the bracket closing the text of a link, like <c>[text](url)</c>.
/// </summary>
/// <remarks>
/// Each of them receives the text starting at its opening bracket, and returns the index of the bracket which closes it,
/// or -1 if there is none. Brackets may appear in the text of a link in matched pairs, or when they are escaped.
/// </remarks>
internal static class LinkTextEnd
{
	private const char Open = '[';
	private const char Close = ']';
	private const char Escape = '\\';

	private static readonly SearchValues<char> Brackets = SearchValues.Create("[]");

	/// <summary>
	/// The current implementation, which stops at the first closing bracket, so it does not support nested pairs.
	/// </summary>
	public static int FirstClose(ReadOnlySpan<char> text) => IndexOfNonEscaped(text, Close, 1);

	/// <summary>
	/// Searches for both kinds of brackets using <see cref="SearchValues{T}"/>, and counts the pairs.
	/// </summary>
	public static int SearchValuesWalk(ReadOnlySpan<char> text) => WalkPairs(text, 1);

	/// <summary>
	/// Searches for both kinds of brackets using the overload of <c>IndexOfAny</c> for two values, and counts the pairs.
	/// </summary>
	public static int IndexOfAnyWalk(ReadOnlySpan<char> text)
	{
		var depth = 0;

		for (var index = 1; index < text.Length; index++)
		{
			var relativeIndex = text[index..].IndexOfAny(Open, Close);
			if (relativeIndex == -1)
			{
				return -1;
			}

			index += relativeIndex;
			if (IsEscaped(text, index))
			{
				continue;
			}

			if (text[index] == Open)
			{
				depth++;
				continue;
			}

			if (depth == 0)
			{
				return index;
			}

			depth--;
		}

		return -1;
	}

	/// <summary>
	/// Searches for the first closing bracket like the current implementation does, and counts the pairs only when
	/// there is an opening bracket before it.
	/// </summary>
	public static int IndexOfThenLookBack(ReadOnlySpan<char> text)
	{
		var closeIndex = IndexOfNonEscaped(text, Close, 1);
		if (closeIndex == -1)
		{
			return -1;
		}

		// an opening bracket before the first closing one may be closed by it, so the pairs have to be counted
		var openIndex = text[1..closeIndex].IndexOf(Open);
		if (openIndex == -1)
		{
			return closeIndex;
		}

		return WalkPairs(text, 1 + openIndex);
	}

	/// <summary>
	/// Visits each character one by one, which avoids the setup cost of a vectorized search for short texts.
	/// </summary>
	/// <remarks>
	/// Escapes are handled while walking forward, so there is no need to look back for them.
	/// </remarks>
	public static int ScalarWalk(ReadOnlySpan<char> text)
	{
		var depth = 0;

		for (var index = 1; index < text.Length; index++)
		{
			switch (text[index])
			{
				// the escaped character is skipped
				case Escape:
					index++;
					break;

				case Open:
					depth++;
					break;

				case Close when depth == 0:
					return index;

				case Close:
					depth--;
					break;
			}
		}

		return -1;
	}

	private static int WalkPairs(ReadOnlySpan<char> text, int start)
	{
		var depth = 0;

		for (var index = start; index < text.Length; index++)
		{
			var relativeIndex = text[index..].IndexOfAny(Brackets);
			if (relativeIndex == -1)
			{
				return -1;
			}

			index += relativeIndex;
			if (IsEscaped(text, index))
			{
				continue;
			}

			if (text[index] == Open)
			{
				depth++;
				continue;
			}

			if (depth == 0)
			{
				return index;
			}

			depth--;
		}

		return -1;
	}

	private static int IndexOfNonEscaped(ReadOnlySpan<char> text, char ch, int start)
	{
		for (var index = start; index < text.Length; index++)
		{
			var relativeIndex = text[index..].IndexOf(ch);
			if (relativeIndex == -1)
			{
				return -1;
			}

			index += relativeIndex;
			if (!IsEscaped(text, index))
			{
				return index;
			}
		}

		return -1;
	}

	/// <summary>
	/// Determines whether a character is escaped, which is the case when it is preceded by an odd number of escapes.
	/// </summary>
	private static bool IsEscaped(ReadOnlySpan<char> text, int index)
	{
		var escapeCount = 0;
		while (escapeCount < index && text[index - escapeCount - 1] == Escape)
		{
			escapeCount++;
		}

		return escapeCount % 2 == 1;
	}
}
