using Microsoft.Extensions.Primitives;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public record class MarkdownTranslationOptions(
	Func<Segment, string>? ResolveSmiley = null,
	Func<Segment, string>? ResolveEmojiByAlias = null
)
{
	public static readonly MarkdownTranslationOptions Default = new();
}
