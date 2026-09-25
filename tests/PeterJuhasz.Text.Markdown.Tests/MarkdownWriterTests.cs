using System.Buffers;
using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Writer;

namespace PeterJuhasz.Text.Markdown.Tests;

[TestClass]
public class MarkdownWriterTests
{
	[TestMethod]
	public void WriteText() => AssertWritten("hello world", w => w.WriteText("hello world"));

	[TestMethod]
	public void WriteTextEscapesDelimiters() => AssertWritten("\\*hello\\*", w => w.WriteText("*hello*"));

	[TestMethod]
	public void WriteLine() => AssertWritten("hello\nworld", w =>
	{
		w.WriteText("hello");
		w.WriteLine();
		w.WriteText("world");
	});

	[TestMethod]
	public void WriteLineUsesNewLineOption()
	{
		var buffer = new ArrayBufferWriter<char>();
		var writer = new MarkdownWriter<ArrayBufferWriter<char>>(buffer, new MarkdownFormattingOptions(NewLine: "\r\n"));

		writer.WriteLine();

		Assert.AreEqual("\r\n", buffer.WrittenSpan.ToString());
	}


	[TestMethod]
	[DataRow(1, "# heading\n")]
	[DataRow(2, "## heading\n")]
	[DataRow(6, "###### heading\n")]
	public void WriteHeading(int level, string expected) => AssertWritten(expected, w => w.WriteHeading(level, "heading"));

	[TestMethod]
	public void OpenHeading() => AssertWritten("## hello **world**\n", w =>
	{
		w.OpenHeading(2);
		w.WriteText("hello ");
		w.WriteBold("world");
		w.Close();
	});


	[TestMethod]
	public void WriteBold() => AssertWritten("**bold**", w => w.WriteBold("bold"));

	[TestMethod]
	public void OpenBold() => AssertWritten("**bold**", w =>
	{
		w.OpenBold();
		w.WriteText("bold");
		w.Close();
	});

	[TestMethod]
	public void WriteItalic() => AssertWritten("*italic*", w => w.WriteItalic("italic"));

	[TestMethod]
	public void OpenItalic() => AssertWritten("*italic*", w =>
	{
		w.OpenItalic();
		w.WriteText("italic");
		w.Close();
	});

	[TestMethod]
	public void WriteUnderline() => AssertWritten("_underline_", w => w.WriteUnderline("underline"));

	[TestMethod]
	public void OpenUnderline() => AssertWritten("_underline_", w =>
	{
		w.OpenUnderline();
		w.WriteText("underline");
		w.Close();
	});

	[TestMethod]
	public void WriteStrikethrough() => AssertWritten("~strike~", w => w.WriteStrikethrough("strike"));

	[TestMethod]
	public void OpenStrikethrough() => AssertWritten("~strike~", w =>
	{
		w.OpenStrikethrough();
		w.WriteText("strike");
		w.Close();
	});

	[TestMethod]
	public void NestedFormatting() => AssertWritten("***bold italic***", w =>
	{
		w.OpenItalic();
		w.OpenBold();
		w.WriteText("bold italic");
		w.Close();
		w.Close();
	});


	[TestMethod]
	public void WriteInlineCode() => AssertWritten("`var x = 1;`", w => w.WriteInlineCode("var x = 1;"));

	[TestMethod]
	public void WriteCodeBlock() => AssertWritten("```\ncode\n```\n", w => w.WriteCodeBlock("code"));

	[TestMethod]
	public void WriteCodeBlockWithLanguage() => AssertWritten("```csharp\nvar x = 1;\n```\n", w => w.WriteCodeBlock("var x = 1;", language: "csharp"));

	[TestMethod]
	public void WriteCodeBlockWithTickLength() => AssertWritten("````\ncode\n````\n", w => w.WriteCodeBlock("code", tickLength: 4));


	[TestMethod]
	public void WriteInlineMath() => AssertWritten("$x^2$", w => w.WriteInlineMath("x^2"));

	[TestMethod]
	public void WriteMathBlock() => AssertWritten("$$\nx^2\n$$\n", w => w.WriteMathBlock("x^2"));

	[TestMethod]
	public void WriteYamlFrontMatter() => AssertWritten("---\ntitle: hello\n---\n", w => w.WriteYamlFrontMatter("title: hello"));


	[TestMethod]
	public void WriteLink() => AssertWritten("[text](https://example.org)", w => w.WriteLink("text", "https://example.org"));

	[TestMethod]
	public void OpenLink() => AssertWritten("[**text**](https://example.org)", w =>
	{
		w.OpenLinkText();
		w.WriteBold("text");
		w.Close();
		w.OpenLinkUrl();
		w.WriteMarkdown("https://example.org");
		w.Close();
	});

	[TestMethod]
	public void WriteImage() => AssertWritten("![alt](https://example.org/image.png)", w => w.WriteImage("alt", "https://example.org/image.png"));

	[TestMethod]
	public void WriteAngleBracketUrl() => AssertWritten("<https://example.org>", w => w.WriteAngleBracketUrl("https://example.org"));

	[TestMethod]
	public void WriteUrl() => AssertWritten("https://example.org", w => w.WriteUrl("https://example.org"));

	[TestMethod]
	public void WriteEmailAddress() => AssertWritten("user@example.org", w => w.WriteEmailAddress("user@example.org"));

	[TestMethod]
	public void WritePhoneNumber() => AssertWritten("+36 1 234 5678", w => w.WritePhoneNumber("+36 1 234 5678"));


	[TestMethod]
	public void WriteSmileyAlias() => AssertWritten(":smile:", w => w.WriteSmileyAlias("smile"));

	[TestMethod]
	public void WriteSmileyEmoji() => AssertWritten(":)", w => w.WriteSmileyEmoji(":)"));

	[TestMethod]
	public void WriteMention() => AssertWritten("@user", w => w.WriteMention("user"));

	[TestMethod]
	public void WriteHashtag() => AssertWritten("#topic", w => w.WriteHashtag("topic"));


	[TestMethod]
	public void WriteFootnoteReference() => AssertWritten("[^1]", w => w.WriteFootnoteReference(1));

	[TestMethod]
	public void OpenFootnoteContent() => AssertWritten("[^1]: note\n", w =>
	{
		w.OpenFootnoteContent(1);
		w.WriteText("note");
		w.Close();
	});


	[TestMethod]
	public void OpenUnorderedListItem() => AssertWritten("- a\n- b\n", w =>
	{
		w.OpenUnorderedListItem();
		w.WriteText("a");
		w.Close();
		w.OpenUnorderedListItem();
		w.WriteText("b");
		w.Close();
	});

	[TestMethod]
	public void OpenOrderedListItem() => AssertWritten("1. a\n12. b\n", w =>
	{
		w.OpenOrderedListItem(1);
		w.WriteText("a");
		w.Close();
		w.OpenOrderedListItem(12);
		w.WriteText("b");
		w.Close();
	});

	[TestMethod]
	public void OpenTaskListItem() => AssertWritten("- [x] done\n", w =>
	{
		w.OpenUnorderedListItem();
		w.WriteCheckbox(true);
		w.WriteText(" done");
		w.Close();
	});


	[TestMethod]
	public void OpenBlockQuote() => AssertWritten("> quote\n", w =>
	{
		w.OpenBlockQuote();
		w.WriteText("quote");
		w.Close();
	});

	[TestMethod]
	[DataRow("NOTE")]
	[DataRow("WARNING")]
	public void OpenAlert(string type) => AssertWritten($"> [!{type}]\n> hello\n", w =>
	{
		w.OpenAlert(type);
		w.WriteText("hello");
		w.Close();
	});

	[TestMethod]
	public void OpenSpoiler() => AssertWritten(">! secret\n", w =>
	{
		w.OpenSpoiler();
		w.WriteText("secret");
		w.Close();
	});


	[TestMethod]
	public void OpenDetailsBlock() => AssertWritten("<details>\n<summary>title</summary>\nhello\n</details>\n", w =>
	{
		w.OpenDetailsBlock();
		w.OpenSummaryBlock();
		w.WriteText("title");
		w.Close();
		w.WriteText("hello");
		w.WriteLine();
		w.Close();
	});


	[TestMethod]
	public void WriteHorizontalRule() => AssertWritten("---\n", w => w.WriteHorizontalRule());

	[TestMethod]
	[DataRow(false, "[ ]")]
	[DataRow(true, "[x]")]
	public void WriteCheckbox(bool isChecked, string expected) => AssertWritten(expected, w => w.WriteCheckbox(isChecked));

	[TestMethod]
	public void WriteComment() => AssertWritten("<!-- note -->", w => w.WriteComment(" note "));


	[TestMethod]
	public void WriteTableRow() => AssertWritten("| a | b |\n", w =>
	{
		w.OpenTableRow();
		w.WriteTableCell("a");
		w.WriteTableCell("b");
		w.Close();
	});

	[TestMethod]
	public void OpenTableCell() => AssertWritten("| **a** |\n", w =>
	{
		w.OpenTableRow();
		w.OpenTableCell();
		w.WriteBold("a");
		w.Close();
		w.Close();
	});

	[TestMethod]
	[DataRow(TableCellAlignment.Left, ":---")]
	[DataRow(TableCellAlignment.Center, ":---:")]
	[DataRow(TableCellAlignment.Right, "---:")]
	public void WriteTableCellAlignment(TableCellAlignment alignment, string expected) => AssertWritten(expected, w => w.WriteTableCellAlignment(alignment));


	private static void AssertWritten(string expected, Action<MarkdownWriter<ArrayBufferWriter<char>>> write)
	{
		var buffer = new ArrayBufferWriter<char>();
		var writer = MarkdownWriter.Create(buffer);

		write(writer);

		Assert.AreEqual(expected, buffer.WrittenSpan.ToString());
	}
}
