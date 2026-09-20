using Markdig;
using Markdig.Extensions.EmphasisExtras;

namespace System.Text.Markdown.Benchmarks;

internal static class MarkdigPipeline
{
	/// <summary>
	/// A pipeline configured to cover roughly the same syntax as this library, so that both parsers
	/// do comparable work on the sample document.
	/// </summary>
	/// <remarks>
	/// <c>UseAdvancedExtensions</c> is deliberately not used, because it enables a dozen extensions
	/// this library does not implement, which would make the comparison meaningless.
	/// </remarks>
	public static readonly MarkdownPipeline Default = new MarkdownPipelineBuilder()
		.UsePipeTables()
		.UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
		.UseTaskLists()
		.UseAutoLinks()
		.UseMathematics()
		.UseEmojiAndSmiley()
		.Build();
}
