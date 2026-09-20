namespace System.Text.Markdown.Tests;

[TestClass]
public class TableTests : TranslationTestBase
{
	[TestMethod]

	// basic
	[TranslationDataRow("| a | b |", "<table><tr><td>a</td><td>b</td></tr></table>")]
	[TranslationDataRow("| a | b |\n| c | d |", "<table><tr><td>a</td><td>b</td></tr><tr><td>c</td><td>d</td></tr></table>")]

	// no padding, unclosed last cell
	[TranslationDataRow("|a|b|", "<table><tr><td>a</td><td>b</td></tr></table>")]
	[TranslationDataRow("|a|b", "<table><tr><td>a</td><td>b</td></tr></table>")]

	// different number of cells, empty cells
	[TranslationDataRow("| a | b |\n| c |", "<table><tr><td>a</td><td>b</td></tr><tr><td>c</td></tr></table>")]
	[TranslationDataRow("| a || b |", "<table><tr><td>a</td><td></td><td>b</td></tr></table>")]

	// header
	[TranslationDataRow("| a | b |\n| --- | --- |\n| c | d |", "<table><thead><tr><th>a</th><th>b</th></tr></thead><tr><td>c</td><td>d</td></tr></table>")]
	[TranslationDataRow("| a |\n| --- |", "<table><thead><tr><th>a</th></tr></thead></table>")]

	// alignment
	[TranslationDataRow("| a | b | c |\n| :-- | :-: | --: |\n| d | e | f |", """<table><thead><tr><th align="left">a</th><th align="center">b</th><th align="right">c</th></tr></thead><tr><td align="left">d</td><td align="center">e</td><td align="right">f</td></tr></table>""")]
	[TranslationDataRow("| a | b |\n| --- | --: |\n| c | d |", """<table><thead><tr><th>a</th><th align="right">b</th></tr></thead><tr><td>c</td><td align="right">d</td></tr></table>""")]
	[TranslationDataRow("| a |\n| :---: |\n| b |\n| :---: |\n| c |", """<table><thead><tr><th align="center">a</th></tr></thead><tr><td align="center">b</td></tr><tfoot><tr><td align="center">c</td></tr></tfoot></table>""")]

	// alignment only applies to the columns the separator row declares
	[TranslationDataRow("| a |\n| --: |\n| b | c |", """<table><thead><tr><th align="right">a</th></tr></thead><tr><td align="right">b</td><td>c</td></tr></table>""")]

	// a separator row which is not the second one does not align
	[TranslationDataRow("| a |\n| b |\n| --: |\n| c |", "<table><tr><td>a</td></tr><tr><td>b</td></tr><tfoot><tr><td>c</td></tr></tfoot></table>")]

	// footer
	[TranslationDataRow("| a | b |\n| --- | --- |\n| c | d |\n| --- | --- |\n| e | f |", "<table><thead><tr><th>a</th><th>b</th></tr></thead><tr><td>c</td><td>d</td></tr><tfoot><tr><td>e</td><td>f</td></tr></tfoot></table>")]
	[TranslationDataRow("| a | b |\n| --- | --- |\n| c | d |", "<table><thead><tr><th>a</th><th>b</th></tr></thead><tr><td>c</td><td>d</td></tr></table>")]

	// inline content
	[TranslationDataRow("| *a* | **b** |", "<table><tr><td><i>a</i></td><td><b>b</b></td></tr></table>")]

	// escaped delimiter
	[TranslationDataRow(@"| a \| b |", "<table><tr><td>a | b</td></tr></table>")]

	// encoding
	[TranslationDataRow("| <b> |", "<table><tr><td>&lt;b&gt;</td></tr></table>")]

	// surrounding blocks
	[TranslationDataRow("# h\n| a |\n\nafter", "<h1>h</h1><table><tr><td>a</td></tr></table><p>after</p>")]

	// not a table
	[TranslationDataRow("|", "<p>|</p>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);
}
