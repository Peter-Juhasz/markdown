using Microsoft.Extensions.Primitives;
using System.Buffers;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public static class SyntaxFacts
{
	public const char EmojiDelimiter = ':';

	public const char EmojiSmileyDelimiter1 = ':';

	public const char EmojiSmileyDelimiter2 = ';';

	public const char MentionDelimiter = '@';

	public const char HashtagDelimiter = '#';

	public const char LinkTextStartDelimiter = '[';

	public const char LinkTextEndDelimiter = ']';

	public const char LinkUrlStartDelimiter = '(';

	public const char LinkUrlEndDelimiter = ')';

	public const char LinkTitleDoubleQuoteDelimiter = '"';

	public const char LinkTitleSingleQuoteDelimiter = '\'';

	public const char UrlColon = ':';

	public const char UrlSlash = '/';

	public const char AngleBracketUrlStartDelimiter = '<';

	public const char AngleBracketUrlEndDelimiter = '>';

	public const char Escape = '\\';

	public const string Bold = "**";

	public const char Italic = '*';

	public const char BoldOrItalic = '*';

	public const char Underlined = '_';

	public const char Strikethrough = '~';

	public const char Heading = '#';

	public const char BlockQuote = '>';

	public const char InlineCodeDelimiter = '`';

	public const string CodeBlockDelimiter = "```";

	public const char InlineMathDelimiter = '$';

	public const string MathBlockDelimiter = "$$";

	public const char EmailUserDomainDelimiter = '@';

	public const char Embed = '!';

	public const string EmbedSchemeDelimiter = "://";

	public const char HorizontalLine = '-';

	public const string FrontMatterDelimiter = "---";

	public const char UnorderedListDash = '-';

	public const char UnorderedListStar = '*';

	public const char OrderedListDot = '.';

	public const char Space = ' ';

	public const char Tab = '\t';

	public const char CheckboxStartDelimiter = '[';

	public const char CheckboxEndDelimiter = ']';

	public const char CheckboxEmptyDelimiter = ' ';

	public const char CheckboxCheckedDelimiter = 'x';

	public const string Spoiler = ">!";

	/// <summary>
	/// The marker which opens an alert, which is a block quote whose first line declares its type, like <c>&gt; [!NOTE]</c>.
	/// </summary>
	public const string AlertStartDelimiter = "> [!";

	public const char AlertEndDelimiter = ']';

	public const char TableCellDelimiter = '|';

	public const char TableSeparator = '-';

	public const char TableAlignment = ':';


	private static readonly SearchValues<char> Delimiters = SearchValues.Create("\n*_[!:;@#`~$");
	private static readonly SearchValues<char> EmojiChars = SearchValues.Create(":;");

	public static bool IsPlaintext(Segment segment) => !segment.AsSpan().ContainsAny(Delimiters);

	public static bool IsEscaped(Segment segment, int index)
	{
		if (segment.PeekPreviousSafe(index) != Escape)
		{
			return false;
		}

		var escapeCount = 1;
		for (var i = 2; i >= 0; i--)
		{
			if (segment.PeekPreviousSafe(index, i) == Escape)
			{
				escapeCount++;
			}
			else
			{
				break;
			}
		}

		return escapeCount % 2 == 1;
	}

	public static bool HasAnyEscaped(ReadOnlySpan<char> chars) => chars.Contains(Escape);

	public static bool HasAnyEscaped(ReadOnlySpan<char> chars, out int firstEscapeIndex)
	{
		firstEscapeIndex = chars.IndexOf(Escape);
		return firstEscapeIndex != -1;
	}

	public static bool MayContainEmojis(ReadOnlySpan<char> chars) => chars.ContainsAny(EmojiChars);

	public static bool MayContainEmojiAlias(Segment document)
	{
		var index = document.IndexOf(EmojiDelimiter);
		while (index != -1)
		{
			if (!IsEscaped(document, index) && Parser.TryParseEmojiAlias(document.Subsegment(index), out _))
			{
				return true;
			}

			index = document.IndexOf(EmojiDelimiter, index + 1);
		}

		return false;
	}

	public static bool MayContainMention(Segment document)
	{
		var index = document.IndexOf(MentionDelimiter);
		while (index != -1)
		{
			if (!IsEscaped(document, index) && Parser.TryParseMention(document.Subsegment(index), out _))
			{
				return true;
			}

			index = document.IndexOf(MentionDelimiter, index + 1);
		}

		return false;
	}
}