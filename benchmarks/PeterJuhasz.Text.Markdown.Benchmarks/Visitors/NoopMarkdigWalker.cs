using Markdig.Syntax;

namespace System.Text.Markdown.Benchmarks.Visitors;

/// <summary>
/// Walks a Markdig syntax tree without doing any work on the nodes.
/// </summary>
/// <remarks>
/// Markdig has no visitor of its own, so its <c>Descendants</c> extension is the closest thing to
/// a full traversal. It flattens the tree into an iterator instead of dispatching per node, which
/// is a different shape of work than a visitor does, and it shows up in the allocation column.
/// </remarks>
internal sealed class NoopMarkdigWalker
{
	public int Count { get; private set; }

	public int Walk(MarkdownDocument document)
	{
		Count = 0;

		foreach (var _ in document.Descendants())
		{
			Count++;
		}

		return Count;
	}
}
