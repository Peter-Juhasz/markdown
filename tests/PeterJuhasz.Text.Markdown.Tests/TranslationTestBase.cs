namespace System.Text.Markdown.Tests;

public abstract class TranslationTestBase
{
	// not static, because test classes run in parallel and the translator is stateful
	private readonly TestMarkdownStringTranslator _translator = new();

	protected void AssertTranslation(string markdown, string expectedHtml)
	{
		_translator.VisitDocument(markdown);
		var actualHtml = _translator.ToString();
		Assert.AreEqual(expectedHtml, actualHtml);
	}
}
