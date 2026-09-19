using Microsoft.Extensions.Primitives;
using System.Buffers;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	private static readonly SearchValues<char> UserNameCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
	private static readonly SearchValues<char> HashtagCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-");

	private static readonly SearchValues<char> EmailUserCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.+_");
	private static readonly SearchValues<char> DomainCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.");

	private static readonly SearchValues<char> UriCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.+_?#%=&/");
	private static readonly SearchValues<char> EmojiAliasCharacters = SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789_");

	private static readonly SearchValues<char> WhitespaceOrOpenDelimiter = SearchValues.Create(" ([,\n\t\0");

	private static readonly SearchValues<char> Delimiters = SearchValues.Create("*_[:;@#`~$");

	public readonly ref struct InlineParser(Segment inline, NodeType parentNode, NodeType disallowedNodeTypes = default)
	{
		public Enumerator GetEnumerator() => new(inline, parentNode, disallowedNodeTypes);

		public ref struct Enumerator(Segment inline, NodeType parentNode, NodeType disallowedNodeTypes = default)
		{
			private Node _next;
			private int _processedIndex;
			private int _searchIndex;
			private int _lastReturnedIndex;

			public Node Current
			{
				get
				{
					var nextRelativeIndex = _next.FullSegment.ToRelativeOffset(inline);

					if (_lastReturnedIndex < nextRelativeIndex)
					{
						var precedingTextNode = new Node(NodeType.Text, inline.Subsegment(_lastReturnedIndex..nextRelativeIndex));
						_lastReturnedIndex = nextRelativeIndex;
						return precedingTextNode;
					}

					_lastReturnedIndex = nextRelativeIndex + _next.FullSegment.Length;
					return _next;
				}
			}

			public bool MoveNext()
			{
				// reached end of document
				if (_lastReturnedIndex == inline.Length)
				{
					_next = default;
					return false;
				}

				// preceding text not returned yet
				if (_next.FullSegment.HasValue && _lastReturnedIndex <= _next.FullSegment.ToRelativeOffset(inline))
				{
					return true;
				}

				_searchIndex = _processedIndex;

				while (_searchIndex < inline.Length)
				{
					// find next delimiter
					var nextRelativeDelimiterIndex = inline.AsSpan(_searchIndex).IndexOfAny(Delimiters);
					if (nextRelativeDelimiterIndex == -1)
					{
						var remaining2 = inline.Subsegment(_processedIndex);
						_next = new(NodeType.Text, remaining2);
						_processedIndex = inline.Length;
						return true;
					}

					var nextDelimiterIndex = _searchIndex + nextRelativeDelimiterIndex;
					if (SyntaxFacts.IsEscaped(inline, nextDelimiterIndex))
					{
						// an escaped '$' is a single character, so the character after it must still be checked (e.g. \$**100**)
						_searchIndex = nextDelimiterIndex + (inline[nextDelimiterIndex] == SyntaxFacts.InlineMathDelimiter ? 1 : 2);
						continue;
					}

					var delimiter = inline[nextDelimiterIndex];

					switch (delimiter)
					{
						// bold or italic
						case SyntaxFacts.BoldOrItalic:
							{
								var isBold = inline.PeekNextSafe(nextDelimiterIndex) == SyntaxFacts.BoldOrItalic;

								// resolve indeterminate
								var isIndeterminate = isBold && inline.PeekNextSafe(nextDelimiterIndex, 2) == SyntaxFacts.BoldOrItalic;
								if (isIndeterminate)
								{
									var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.BoldOrItalic, nextDelimiterIndex + 1 + SyntaxFacts.Bold.Length);
									if (endIndex == -1)
									{
										_searchIndex = nextDelimiterIndex + 1;
										continue;
									}

									isBold = inline.PeekNextSafe(endIndex) == SyntaxFacts.BoldOrItalic;
								}

								// bold
								if (isBold && TryParseBold(inline.Subsegment(nextDelimiterIndex), out var bold) && !disallowedNodeTypes.HasFlag(NodeType.Bold))
								{
									_next = bold;
									_processedIndex = bold.FullSegment.ToRelativeOffset(inline) + bold.FullSegment.Length;
									return true;
								}

								// italic
								else if (TryParseItalic(inline.Subsegment(nextDelimiterIndex), out var italic) && !disallowedNodeTypes.HasFlag(NodeType.Italic))
								{
									_next = italic;
									_processedIndex = italic.FullSegment.ToRelativeOffset(inline) + italic.FullSegment.Length;
									return true;
								}

								break;
							}

						// underlined
						case SyntaxFacts.Underlined when TryParseUnderline(inline.Subsegment(nextDelimiterIndex), out var node) && !disallowedNodeTypes.HasFlag(NodeType.Underline):
							{
								_next = node;
								_processedIndex = node.FullSegment.ToRelativeOffset(inline) + node.FullSegment.Length;
								return true;
							}

						// strikethrough
						case SyntaxFacts.Strikethrough when TryParseStrikethrough(inline.Subsegment(nextDelimiterIndex), out var node) && !disallowedNodeTypes.HasFlag(NodeType.Strikethrough):
							{
								_next = node;
								_processedIndex = node.FullSegment.ToRelativeOffset(inline) + node.FullSegment.Length;
								return true;
							}

						// link
						case SyntaxFacts.LinkTextStartDelimiter when TryParseLink(inline.Subsegment(nextDelimiterIndex), out var node) && !disallowedNodeTypes.HasFlag(NodeType.Link):
							{
								_next = node;
								_processedIndex = node.FullSegment.ToRelativeOffset(inline) + node.FullSegment.Length;
								return true;
							}

						// checkbox
						case SyntaxFacts.CheckboxStartDelimiter when TryParseCheckbox(inline.Subsegment(nextDelimiterIndex), out var node) && parentNode == NodeType.UnorderedListItem && !disallowedNodeTypes.HasFlag(NodeType.Checkbox):
							{
								_next = node;
								_processedIndex = node.FullSegment.ToRelativeOffset(inline) + node.FullSegment.Length;
								return true;
							}

						// e-mail
						case SyntaxFacts.MentionDelimiter when TryParseEmailAddressAtAt(inline.Subsegment(_processedIndex), nextDelimiterIndex - _processedIndex, out var node) && !disallowedNodeTypes.HasFlag(NodeType.EmailAddress):
							{
								_next = node;
								_processedIndex = node.FullSegment.ToRelativeOffset(inline) + node.FullSegment.Length;
								return true;
							}

						// mention
						case SyntaxFacts.MentionDelimiter when TryParseMention(inline.Subsegment(nextDelimiterIndex), out var code) && !disallowedNodeTypes.HasFlag(NodeType.Mention):
							{
								_next = code;
								_processedIndex = code.FullSegment.ToRelativeOffset(inline) + code.FullSegment.Length;
								return true;
							}

						// hashtag
						case SyntaxFacts.HashtagDelimiter when TryParseHashtag(inline.Subsegment(nextDelimiterIndex), out var code) && !disallowedNodeTypes.HasFlag(NodeType.Hashtag):
							{
								_next = code;
								_processedIndex = code.FullSegment.ToRelativeOffset(inline) + code.FullSegment.Length;
								return true;
							}

						// url
						case SyntaxFacts.UrlColon when TryParseUrlAtColon(inline.Subsegment(_processedIndex), nextDelimiterIndex - _processedIndex, out var url) && !disallowedNodeTypes.HasFlag(NodeType.Url):
							{
								_next = url;
								_processedIndex = url.FullSegment.ToRelativeOffset(inline) + url.FullSegment.Length;
								return true;
							}

						// emoji smiley
						case SyntaxFacts.EmojiSmileyDelimiter1 or
							 SyntaxFacts.EmojiSmileyDelimiter2 when
							 TryParseEmojiSmiley(inline.Subsegment(nextDelimiterIndex), out var alias) && !disallowedNodeTypes.HasFlag(NodeType.EmojiSmiley):
							{
								_next = alias;
								_processedIndex = alias.FullSegment.ToRelativeOffset(inline) + alias.FullSegment.Length;
								return true;
							}

						// emoji alias
						case SyntaxFacts.EmojiDelimiter when TryParseEmojiAlias(inline.Subsegment(nextDelimiterIndex), out var alias) && !disallowedNodeTypes.HasFlag(NodeType.EmojiAlias):
							{
								_next = alias;
								_processedIndex = alias.FullSegment.ToRelativeOffset(inline) + alias.FullSegment.Length;
								return true;
							}

						// code
						case SyntaxFacts.InlineCodeDelimiter when TryParseInlineCode(inline.Subsegment(nextDelimiterIndex), out var code) && !disallowedNodeTypes.HasFlag(NodeType.InlineCode):
							{
								_next = code;
								_processedIndex = code.FullSegment.ToRelativeOffset(inline) + code.FullSegment.Length;
								return true;
							}

						// math
						case SyntaxFacts.InlineMathDelimiter when TryParseInlineMath(inline.Subsegment(nextDelimiterIndex), out var math) && !disallowedNodeTypes.HasFlag(NodeType.InlineMath):
							{
								_next = math;
								_processedIndex = math.FullSegment.ToRelativeOffset(inline) + math.FullSegment.Length;
								return true;
							}
					}

					_searchIndex = nextDelimiterIndex + 1;
				}

				var remaining = inline.Subsegment(_processedIndex);
				_next = new(NodeType.Text, remaining);
				_processedIndex = inline.Length;
				return true;
			}

		}
	}

	internal static bool TryParseItalic(Segment inline, out Node node)
	{
		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.Italic, 1) + 1;
		if (endIndex == 0)
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Italic, segment);
		return true;
	}

	internal static bool TryParseBold(Segment inline, out Node node)
	{
		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.Bold, SyntaxFacts.Bold.Length);
		if (endIndex == -1)
		{
			node = default;
			return false;
		}

		endIndex += SyntaxFacts.Bold.Length;
		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Bold, segment);
		return true;
	}

	internal static bool TryParseUnderline(Segment inline, out Node node)
	{
		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.Underlined, 1) + 1;
		if (endIndex == 0)
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Underline, segment);
		return true;
	}

	internal static bool TryParseStrikethrough(Segment inline, out Node node)
	{
		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.Strikethrough, 1) + 1;
		if (endIndex == 0)
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Strikethrough, segment);
		return true;
	}

	internal static bool TryParseLink(Segment inline, out Node node)
	{
		var textEndIndex = inline.IndexOfNonEscaped(SyntaxFacts.LinkTextEndDelimiter, 1);
		if (textEndIndex == -1)
		{
			node = default;
			return false;
		}

		if (inline.PeekNextSafe(textEndIndex) != SyntaxFacts.LinkUrlStartDelimiter)
		{
			node = default;
			return false;
		}

		var urlStartIndex = textEndIndex + 1;
		var urlEndIndex = inline.IndexOfNonEscaped(SyntaxFacts.LinkUrlEndDelimiter, urlStartIndex + 1);
		if (urlEndIndex == -1)
		{
			node = default;
			return false;
		}

		urlEndIndex += 1;
		var segment = inline.Subsegment(..urlEndIndex);
		node = new(NodeType.Link, segment);
		return true;
	}

	internal static bool TryParseMention(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (!WhitespaceOrOpenDelimiter.Contains(previous))
		{
			node = default;
			return false;
		}

		if (!Char.IsLetter(inline.PeekNextSafe()))
		{
			node = default;
			return false;
		}

		var length = inline.AsSpan(1).CountWhile(UserNameCharacters);
		var endIndex = 1 + length;
		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Mention, segment);
		return true;
	}

	internal static bool TryParseHashtag(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (!WhitespaceOrOpenDelimiter.Contains(previous))
		{
			node = default;
			return false;
		}

		if (!Char.IsLetter(inline.PeekNextSafe()))
		{
			node = default;
			return false;
		}

		var length = inline.AsSpan(1).CountWhile(HashtagCharacters);
		var endIndex = 1 + length;
		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.Hashtag, segment);
		return true;
	}

	internal static bool TryParseInlineCode(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (!WhitespaceOrOpenDelimiter.Contains(previous))
		{
			node = default;
			return false;
		}

		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.InlineCodeDelimiter, 1) + 1;
		if (endIndex == 0)
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.InlineCode, segment);
		return true;
	}

	internal static bool TryParseInlineMath(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (!WhitespaceOrOpenDelimiter.Contains(previous))
		{
			node = default;
			return false;
		}

		// opening delimiter must be followed by a non-whitespace character
		if (inline.Length < 3 || Char.IsWhiteSpace(inline[1]) || inline[1] == SyntaxFacts.InlineMathDelimiter)
		{
			node = default;
			return false;
		}

		var endIndex = inline.IndexOfNonEscaped(SyntaxFacts.InlineMathDelimiter, 1);
		if (endIndex == -1)
		{
			node = default;
			return false;
		}

		// closing delimiter must be preceded by a non-whitespace character, and must not be followed by a digit (e.g. $5 and $10)
		if (Char.IsWhiteSpace(inline[endIndex - 1]) || Char.IsDigit(inline.PeekNextSafe(endIndex)))
		{
			node = default;
			return false;
		}

		endIndex += 1;
		var segment = inline.Subsegment(..endIndex);
		node = new(NodeType.InlineMath, segment);
		return true;
	}

	internal static bool TryParseEmojiAlias(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (!WhitespaceOrOpenDelimiter.Contains(previous))
		{
			node = default;
			return false;
		}

		var endIndex = inline.IndexOf(SyntaxFacts.EmojiDelimiter, 1) + 1;
		if (endIndex == 0)
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..endIndex);
		if (segment.AsSpan()[1..^1].ContainsAnyExcept(EmojiAliasCharacters))
		{
			node = default;
			return false;
		}

		node = new(NodeType.EmojiAlias, segment);
		return true;
	}

	internal static bool TryParseCheckbox(Segment inline, out Node node)
	{
		var closeDelimiter = inline.PeekNextSafe(relative: 2);
		if (closeDelimiter != SyntaxFacts.CheckboxEndDelimiter)
		{
			node = default;
			return false;
		}

		var content = inline.PeekNextSafe(relative: 1);
		if (content is not (SyntaxFacts.CheckboxEmptyDelimiter or SyntaxFacts.CheckboxCheckedDelimiter))
		{
			node = default;
			return false;
		}

		var segment = inline.Subsegment(..3);
		node = new(NodeType.Checkbox, segment);
		return true;
	}

	internal static bool TryParseEmailAddressAtAt(Segment inline, int atIndex, out Node node)
	{
		if (!Char.IsLetter(inline.PeekNextSafe(atIndex)))
		{
			node = default;
			return false;
		}

		if (!Char.IsLetterOrDigit(inline.PeekPreviousSafe(atIndex)))
		{
			node = default;
			return false;
		}

		// e-mail
		var startIndex = atIndex - inline.AsSpan(0, atIndex).CountWhileBackwards(EmailUserCharacters);
		var endIndex = atIndex + 1 + inline.AsSpan(atIndex + 1).CountWhile(DomainCharacters);

		var segment = inline.Subsegment(startIndex..endIndex);
		node = new(NodeType.EmailAddress, segment);
		return true;
	}

	internal static bool TryParseUrlAtColon(Segment inline, int colonIndex, out Node node)
	{
		if (inline.PeekPreviousSafe(colonIndex, 1) != 's' || inline.PeekNextSafe(colonIndex, 2) != SyntaxFacts.UrlSlash || inline.PeekNextSafe(colonIndex, 1) != SyntaxFacts.UrlSlash)
		{
			node = default;
			return false;
		}

		if (!inline.Subsegment(..colonIndex).EndsWith("https", StringComparison.OrdinalIgnoreCase))
		{
			node = default;
			return false;
		}

		var startIndex = colonIndex - "https".Length;
		var endIndex = colonIndex + 3 + inline.AsSpan(colonIndex + 3).CountWhile(UriCharacters);
		var segment = inline.Subsegment(startIndex..endIndex);
		node = new(NodeType.Url, segment);
		return true;
	}


	public static Segment GetBoldContent(this Node node) => node.FullSegment.Subsegment(SyntaxFacts.Bold.Length..^SyntaxFacts.Bold.Length);
	public static Segment GetItalicContent(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static Segment GetUnderlineContent(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static Segment GetStrikethroughContent(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static int GetHeadingLevel(this Node node) => node.FullSegment.AsSpan().TrimStart().CommonPrefixLength("######"); // TODO: better implementation
	public static Segment GetHeadingContent(this Node node) => node.FullSegment.Subsegment(node.GetHeadingLevel()..).Trim(); // TODO: better implementation
	public static Segment GetBlockQuoteContent(this Node node) => node.FullSegment.Subsegment(1..).Trim(); // HACK
	public static Segment GetLinkContent(this Node node) => node.FullSegment.Subsegment(1..node.FullSegment.IndexOf(']'));
	public static Segment GetLinkUrl(this Node node) => node.FullSegment.Subsegment((node.FullSegment.LastIndexOf('(') + 1)..^1);
	public static Segment GetEmbedScheme(this Node node) => node.FullSegment.Subsegment(2..node.FullSegment.IndexOf(':'));
	public static Segment GetEmbedId(this Node node) => node.FullSegment.Subsegment((node.FullSegment.IndexOf(':') + SyntaxFacts.EmbedSchemeDelimiter.Length)..^1);
	public static Segment GetMentionedUser(this Node node) => node.FullSegment.Subsegment(1);
	public static Segment GetHashtag(this Node node) => node.FullSegment.Subsegment(1);
	public static Segment GetUnorderedListItemContent(this Node node) => node.FullSegment.Subsegment(1).Trim();
	public static Segment GetOrderedListItemContent(this Node node) => node.FullSegment.Subsegment(node.FullSegment.IndexOf('.') + 1).Trim();
	public static Segment GetInlineCode(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static Segment GetCode(this Node node)
	{
		var tickCount = node.FullSegment.AsSpan().IndexOfAnyExcept(SyntaxFacts.InlineCodeDelimiter);
		var lineEnd = node.FullSegment.IndexOf('\n', tickCount) + 1;
		var endIndex = node.FullSegment.Length - (1 + tickCount);
		if (endIndex < lineEnd)
		{
			return Segment.Empty;
		}

		return node.FullSegment.Subsegment(lineEnd..endIndex).TrimEnd();
	}

	public static Segment GetCodeLanguage(this Node node)
	{
		var tickCount = node.FullSegment.AsSpan().IndexOfAnyExcept(SyntaxFacts.InlineCodeDelimiter);
		var lineEnd = node.FullSegment.IndexOf('\n', tickCount);

		return node.FullSegment.Subsegment(tickCount..lineEnd).Trim();
	}

	public static Segment GetInlineMath(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static Segment GetMath(this Node node)
	{
		var lineEnd = node.FullSegment.IndexOf('\n') + 1;

		// single line
		if (lineEnd == 0)
		{
			return node.FullSegment.Subsegment(SyntaxFacts.MathBlockDelimiter.Length..^SyntaxFacts.MathBlockDelimiter.Length).Trim();
		}

		// multiple lines
		var endIndex = node.FullSegment.Length - (1 + SyntaxFacts.MathBlockDelimiter.Length);
		if (endIndex < lineEnd)
		{
			return Segment.Empty;
		}

		return node.FullSegment.Subsegment(lineEnd..endIndex).TrimEnd();
	}

	public static Segment GetEmojiAlias(this Node node) => node.FullSegment.Subsegment(1..^1);
	public static bool GetCheckboxState(this Node node) => node.FullSegment[1] != SyntaxFacts.CheckboxEmptyDelimiter;
}