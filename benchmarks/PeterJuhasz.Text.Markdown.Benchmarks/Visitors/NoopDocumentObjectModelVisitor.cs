using System.Text.Markdown.Model;

namespace System.Text.Markdown.Benchmarks.Visitors;

/// <summary>
/// Traverses a Document Object Model without doing any work on the nodes, to measure the cost of
/// the traversal itself.
/// </summary>
/// <remarks>
/// Every node is counted, so that the traversal can neither be optimized away nor silently visit
/// nothing.
/// </remarks>
internal sealed class NoopDocumentObjectModelVisitor : DocumentObjectModelVisitor
{
	public int Count { get; private set; }

	protected override void Visit(Node node)
	{
		Count++;
		base.Visit(node);
	}
}
