using Microsoft.Extensions.Primitives;

namespace PeterJuhasz.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	internal static bool TryParseEmojiSmiley(Segment inline, out Node node)
	{
		var previous = inline.PeekPreviousOutOfBoundsSafe();
		if (previous != default && !Char.IsWhiteSpace(previous))
		{
			node = default;
			return false;
		}

		if (inline.Length < 2)
		{
			node = default;
			return false;
		}

		switch (inline[0])
		{
			case ':':
				switch (inline[1])
				{
					case ')':
					case '(':
					case '/':
					case 'P':
					case 'D':
					case 'O':
						node = new(NodeType.EmojiSmiley, inline.Subsegment(..2));
						return true;

					case '-' when inline.Length > 2:
						switch (inline[2])
						{
							case ')':
							case '(':
							case '/':
							case 'D':
							case 'P':
							case 'O':
								node = new(NodeType.EmojiSmiley, inline.Subsegment(..3));
								return true;

							default:
								node = default;
								return false;
						}

					case '\'' when inline.Length > 2:
						switch (inline[2])
						{
							case 'D':
								node = new(NodeType.EmojiSmiley, inline.Subsegment(..3));
								return true;

							default:
								node = default;
								return false;
						}

					default:
						node = default;
						return false;
				}

			case ';':
				switch (inline[1])
				{
					case ')':
						node = new(NodeType.EmojiSmiley, inline.Subsegment(..2));
						return true;

					case '-' when inline.Length > 2:
						switch (inline[2])
						{
							case ')':
								node = new(NodeType.EmojiSmiley, inline.Subsegment(..3));
								return true;

							default:
								node = default;
								return false;
						}

					default:
						node = default;
						return false;
				}

			default:
				node = default;
				return false;
		}
	}
}