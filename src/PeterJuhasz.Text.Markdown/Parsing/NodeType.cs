namespace System.Text.Markdown.Parsing;

[Flags]
public enum NodeType : long
{
	Document = 0,

	Paragraph = 1,
	Heading = 1 << 1,
	UnorderedList = 1 << 2,
	UnorderedListItem = 1 << 3,
	OrderedList = 1 << 4,
	OrderedListItem = 1 << 5,
	BlockQuote = 1 << 6,
	CodeBlock = 1 << 7,
	HorizontalRule = 1 << 8,
	EmptyLine = 1 << 9,
	Spoiler = 1 << 27,
	MathBlock = 1 << 28,
	TableBlock = 1 << 30,
	TableRow = 1L << 31,
	FrontMatter = 1L << 34,
	Alert = 1L << 35,

	Text = 1 << 10,
	Bold = 1 << 11,
	Italic = 1 << 12,
	Underline = 1 << 13,
	Strikethrough = 1 << 14,
	EmailAddress = 1 << 15,
	PhoneNumber = 1 << 16,
	Url = 1 << 17,
	AngleBracketUrl = 1L << 33,
	Link = 1 << 18,
	Image = 1 << 19,
	Embed = 1 << 20,
	InlineCode = 1 << 21,
	InlineMath = 1 << 29,
	TableCell = 1L << 32,

	EmojiSmiley = 1 << 22,
	EmojiAlias = 1 << 23,
	Mention = 1 << 24,
	Hashtag = 1 << 25,
	Checkbox = 1 << 26,
};
