using System.Diagnostics.CodeAnalysis;

namespace PeterJuhasz.Text.Markdown.Tests;

internal sealed class TranslationDataRowAttribute(string markdown, [StringSyntax(StringSyntaxAttribute.Xml)] string html)
	: DataRowAttribute(markdown, html)
{
}
