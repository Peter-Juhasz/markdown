using BenchmarkDotNet.Attributes;
using Markdig.Syntax;
using System.Text.Markdown.Benchmarks.Visitors;
using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Benchmarks;

using MarkdigMarkdown = Markdig.Markdown;

/// <summary>
/// Traverses the sample document with a visitor which does nothing on each node.
/// </summary>
/// <remarks>
/// The high level benchmarks walk a Document Object Model which is built once in the global setup,
/// so they measure the traversal only. The low level benchmark has nothing to build up front: the
/// streaming API parses lazily while it visits, so its number includes the parse and is directly
/// comparable to <see cref="ParseBenchmarks"/> instead.
/// </remarks>
[MemoryDiagnoser]
public class VisitBenchmarks
{
	private string _markdown = null!;
	private DocumentNode _document = null!;
	private MarkdownDocument _markdigDocument = null!;

	[GlobalSetup]
	public void Setup()
	{
		_markdown = SampleDocument.Read();
		_document = Parser.Parse(_markdown);
		_markdigDocument = MarkdigMarkdown.Parse(_markdown, MarkdigPipeline.Default);
	}

	[Benchmark(Baseline = true, Description = "Markdig (walk)")]
	public int Markdig()
	{
		return new NoopMarkdigWalker().Walk(_markdigDocument);
	}

	[Benchmark(Description = "PeterJuhasz.Text.Markdown (high level)")]
	public int PeterJuhaszHighLevel()
	{
		var visitor = new NoopDocumentObjectModelVisitor();
		visitor.VisitDocument(_document);
		return visitor.Count;
	}

	[Benchmark(Description = "PeterJuhasz.Text.Markdown (low level, includes parsing)")]
	public int PeterJuhaszLowLevel()
	{
		var visitor = new NoopInplaceMarkdownVisitor();
		visitor.VisitDocument(_markdown);
		return visitor.Count;
	}
}
