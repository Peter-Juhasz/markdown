using System.Buffers;
using PeterJuhasz.Text.Markdown.Model;

namespace PeterJuhasz.Text.Markdown.Writer;

public static partial class Extensions
{
	private static string[]? CodeBlockTickPairs;
	private const int MaxTickLength = 10;


	extension<TWriter>(MarkdownWriter<TWriter> writer) where TWriter : IBufferWriter<char>
	{
		public void OpenHeading(int level)
		{
			if (level < 1 || level > 6)
			{
				throw new ArgumentOutOfRangeException(nameof(level), "Heading level must be between 1 and 6.");
			}

			Span<char> headingPrefix = writer.Writer.GetSpan(level + 1);
			headingPrefix.Fill('#');
			headingPrefix[level] = ' ';
			writer.Writer.Advance(level + 1);
			writer.PushClosePair(writer.FormattingOptions.NewLine);
		}

		public void WriteHeading(int level, ReadOnlySpan<char> text)
		{
			writer.OpenHeading(level);
			writer.WriteText(text);
			writer.Close();
		}


		public void WriteBold(ReadOnlySpan<char> text) => writer.WriteBetweenPair("**", text, "**");

		public void OpenBold() => writer.OpenPair("**", "**");


		public void WriteItalic(ReadOnlySpan<char> text) => writer.WriteBetweenPair("*", text, "*");

		public void OpenItalic() => writer.OpenPair("*", "*");


		public void WriteUnderline(ReadOnlySpan<char> text) => writer.WriteBetweenPair("_", text, "_");

		public void OpenUnderline() => writer.OpenPair("_", "_");


		public void WriteStrikethrough(ReadOnlySpan<char> text) => writer.WriteBetweenPair("~", text, "~");

		public void OpenStrikethrough() => writer.OpenPair("~", "~");


		public void WriteInlineCode(ReadOnlySpan<char> code)
		{
			writer.WriteMarkdown("`");
			writer.WriteMarkdown(code);
			writer.WriteMarkdown("`");
		}

		public void WriteCodeBlock(ReadOnlySpan<char> code, int tickLength = 3, ReadOnlySpan<char> language = default)
		{
			if (tickLength < 3 || tickLength > MaxTickLength)
			{
				throw new ArgumentOutOfRangeException(nameof(tickLength), "Tick length must be between 3 and " + MaxTickLength + ".");
			}

			if (tickLength == 3)
			{
				writer.WriteMarkdown("```");

				if (!language.IsEmpty)
				{
					writer.WriteMarkdown(language);
				}

				writer.WriteLine();
				writer.WriteMarkdown(code);
				writer.WriteLine();
				writer.WriteMarkdown("```");
				writer.WriteLine();
				return;
			}

			if (CodeBlockTickPairs == null)
			{
				CodeBlockTickPairs = new string[MaxTickLength + 1];
			}

			var ticks = CodeBlockTickPairs[tickLength];
			if (ticks == null)
			{
				ticks = new string('`', tickLength);
				CodeBlockTickPairs[tickLength] = ticks;
			}

			writer.WriteMarkdown(ticks);

			if (!language.IsEmpty)
			{
				writer.WriteMarkdown(language);
			}

			writer.WriteLine();
			writer.WriteMarkdown(code);
			writer.WriteLine();
			writer.WriteMarkdown(ticks);
			writer.WriteLine();
		}


		public void WriteInlineMath(ReadOnlySpan<char> math)
		{
			writer.WriteMarkdown("$");
			writer.WriteMarkdown(math);
			writer.WriteMarkdown("$");
		}

		public void WriteMathBlock(ReadOnlySpan<char> math)
		{
			writer.WriteMarkdown("$$");
			writer.WriteLine();
			writer.WriteMarkdown(math);
			writer.WriteLine();
			writer.WriteMarkdown("$$");
			writer.WriteLine();
		}

		public void WriteYamlFrontMatter(ReadOnlySpan<char> yaml)
		{
			writer.WriteMarkdown("---");
			writer.WriteLine();
			writer.WriteMarkdown(yaml);
			writer.WriteLine();
			writer.WriteMarkdown("---");
			writer.WriteLine();
		}


		public void WriteLink(ReadOnlySpan<char> text, ReadOnlySpan<char> url)
		{
			writer.WriteBetweenPair("[", text, "]");
			writer.WriteMarkdownBetweenPair("(", url, ")");
		}

		public void OpenLinkText() => writer.OpenPair("[", "]");

		public void OpenLinkUrl() => writer.OpenPair("(", ")");


		public void WriteImage(ReadOnlySpan<char> altText, ReadOnlySpan<char> url)
		{
			writer.WriteMarkdown("!");
			writer.WriteBetweenPair("[", altText, "]");
			writer.WriteMarkdownBetweenPair("(", url, ")");
		}


		public void WriteAngleBracketUrl(ReadOnlySpan<char> url) => writer.WriteMarkdownBetweenPair("<", url, ">");


		public void WriteEmailAddress(ReadOnlySpan<char> email) => writer.WriteMarkdown(email);

		public void WritePhoneNumber(ReadOnlySpan<char> phoneNumber) => writer.WriteMarkdown(phoneNumber);

		public void WriteUrl(ReadOnlySpan<char> url) => writer.WriteMarkdown(url);


		public void WriteSmileyAlias(ReadOnlySpan<char> alias) => writer.WriteBetweenPair(":", alias, ":");

		public void WriteSmileyEmoji(ReadOnlySpan<char> emoji) => writer.WriteMarkdown(emoji);

		public void WriteMention(ReadOnlySpan<char> username)
		{
			writer.WriteMarkdown("@");
			writer.WriteMarkdown(username);
		}

		public void WriteHashtag(ReadOnlySpan<char> hashtag)
		{
			writer.WriteMarkdown("#");
			writer.WriteMarkdown(hashtag);
		}


		public void WriteFootnoteReference(int number) => writer.WriteBetweenPair("[^", number.ToString(), "]");

		public void OpenFootnoteContent(int number)
		{
			writer.WriteFootnoteReference(number);
			writer.OpenPair(": ", writer.FormattingOptions.NewLine);
		}


		public void OpenUnorderedListItem() => writer.OpenPair("- ", writer.FormattingOptions.NewLine);

		public void OpenOrderedListItem(int number)
		{
			if (number < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(number), "List item number must not be negative.");
			}

			Span<char> prefix = writer.Writer.GetSpan(12);
			number.TryFormat(prefix, out int length);
			prefix[length++] = '.';
			prefix[length++] = ' ';
			writer.Writer.Advance(length);
			writer.PushClosePair(writer.FormattingOptions.NewLine);
		}


		public void OpenBlockQuote() => writer.OpenPair("> ", writer.FormattingOptions.NewLine);

		public void OpenAlert(ReadOnlySpan<char> type)
		{
			writer.WriteMarkdownBetweenPair("> [!", type, "]");
			writer.WriteLine();
			writer.OpenPair("> ", writer.FormattingOptions.NewLine);
		}

		public void OpenSpoiler() => writer.OpenPair(">! ", writer.FormattingOptions.NewLine);


		public void OpenDetailsBlock()
		{
			writer.WriteMarkdown("<details>");
			writer.WriteLine();
			writer.PushClosePair("</details>" + writer.FormattingOptions.NewLine);
		}

		public void OpenSummaryBlock() => writer.OpenPair("<summary>", "</summary>" + writer.FormattingOptions.NewLine);


		public void WriteHorizontalRule()
		{
			writer.WriteMarkdown("---");
			writer.WriteLine();
		}

		public void WriteCheckbox(bool isChecked) => writer.WriteMarkdown(isChecked ? "[x]" : "[ ]");

		public void WriteComment(ReadOnlySpan<char> comment) => writer.WriteBetweenPair("<!--", comment, "-->");


		public void OpenTableRow() => writer.OpenPair("|", writer.FormattingOptions.NewLine);

		public void OpenTableCell() => writer.OpenPair(" ", " |");

		public void WriteTableCell(ReadOnlySpan<char> text) => writer.WriteBetweenPair(" ", text, " |");

		public void WriteTableCellAlignment(TableCellAlignment alignment) => writer.WriteMarkdown(alignment switch
		{
			TableCellAlignment.Left => ":---",
			TableCellAlignment.Center => ":---:",
			TableCellAlignment.Right => "---:",
			_ => throw new ArgumentOutOfRangeException(nameof(alignment), "Invalid table cell alignment.")
		});
	}
}