using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using PeterJuhasz.Text.Markdown.Model;

namespace PeterJuhasz.Text.Markdown.Higlighting;

public class HighlighterTokenizer
{
	private static readonly CompareInfo _compareInfo = CultureInfo.CurrentCulture.CompareInfo;
	private static string? _lastMatch = default;

	public static ImmutableArray<InlineNode> Highlight(string text, string searchText)
	{
		if (text.IsNullOrWhiteSpace())
		{
			return [new TextNode(String.Empty)];
		}

		if (searchText.IsNullOrWhiteSpace())
		{
			return [new TextNode(text)];
		}

		var absoluteNextMatchIndex = text.IndexOfAtWordStart(searchText, _compareInfo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
		if (absoluteNextMatchIndex == -1)
		{
			return [new TextNode(text)];
		}

		return HighlightCore(text.AsSpan(), searchText, absoluteNextMatchIndex);
	}

	private static ImmutableArray<InlineNode> HighlightCore(ReadOnlySpan<char> text, string searchText, int absoluteNextMatchIndex)
	{
		using var results = new PooledArrayBuilder<InlineNode>();
		var absoluteProcessedIndex = 0;

		while (absoluteNextMatchIndex != -1)
		{
			// add skipped text
			if (absoluteProcessedIndex < absoluteNextMatchIndex)
			{
				results.Add(new TextNode(new string(text[absoluteProcessedIndex..absoluteNextMatchIndex])));
			}

			// get string slice
			var matchSpan = text.Slice(absoluteNextMatchIndex, searchText.Length);
			string? matchString = null;
			if (_lastMatch is string lm && matchSpan.SequenceEqual(lm))
			{
				matchString = lm;
			}
			else if (matchSpan.SequenceEqual(searchText))
			{
				matchString = searchText;
			}
			else
			{
				matchString = new string(matchSpan);
				_lastMatch = matchString;
			}

			// create node
			results.Add(new HighlightedNode(matchString));
			absoluteProcessedIndex = absoluteNextMatchIndex + searchText.Length;

			// find next match
			var remaining = text[absoluteProcessedIndex..];
			var relativeNextIndex = remaining.IndexOfAtWordStart(searchText, _compareInfo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);
			if (relativeNextIndex == -1)
			{
				// no more matches
				break;
			}
			absoluteNextMatchIndex = absoluteProcessedIndex + relativeNextIndex;
		}

		// add remaining text
		if (absoluteProcessedIndex < text.Length)
		{
			results.Add(new TextNode(new string(text[absoluteProcessedIndex..])));
		}

		return results.ToImmutableArray();
	}

	public static ImmutableArray<InlineNode> Highlight(string text, SearchQuery query)
	{
		if (text.IsNullOrWhiteSpace())
		{
			return [new TextNode(String.Empty)];
		}

		if (!query.HasMultiplePhrases)
		{
			return Highlight(text, query.Query);
		}

		using var results = new PooledArrayBuilder<InlineNode>();
		var absoluteProcessedIndex = 0;

		foreach (var range in query.Matches(text))
		{
			var (absoluteNextMatchIndex, length) = range.GetOffsetAndLength(text.Length);
			if (absoluteProcessedIndex < absoluteNextMatchIndex)
			{
				results.Add(new TextNode(text[absoluteProcessedIndex..absoluteNextMatchIndex]));
			}

			results.Add(new HighlightedNode(text[range]));
			absoluteProcessedIndex = range.End.GetOffset(text.Length);
		}

		if (absoluteProcessedIndex < text.Length)
		{
			results.Add(new TextNode(text[absoluteProcessedIndex..]));
		}

		return results.ToImmutableArray();
	}
}