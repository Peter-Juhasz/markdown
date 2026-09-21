namespace System.Text.Markdown.Tests;

[TestClass]
public class TaskListTests : TranslationTestBase
{
	[TestMethod]

	[TranslationDataRow("- [x] task", """<ul><li><input type="checkbox" disabled checked /> task</li></ul>""")]
	[TranslationDataRow("- [ ] task", """<ul><li><input type="checkbox" disabled /> task</li></ul>""")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);
}
