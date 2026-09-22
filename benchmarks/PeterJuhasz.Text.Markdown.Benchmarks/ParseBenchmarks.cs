using BenchmarkDotNet.Attributes;
using Markdig.Syntax;
using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Benchmarks;

using MarkdigMarkdown = Markdig.Markdown;

/// <summary>
/// Parses the sample document into a Document Object Model.
/// </summary>
[MemoryDiagnoser]
public class ParseBenchmarks
{
	private string _markdown = null!;

	[GlobalSetup]
	public void Setup() => _markdown = SampleDocument.Read();

	[Benchmark(Baseline = true, Description = "Markdig")]
	public MarkdownDocument Markdig()
	{
		return MarkdigMarkdown.Parse(_markdown, MarkdigPipeline.Default);
	}

	[Benchmark(Description = "PeterJuhasz.Text.Markdown")]
	public DocumentNode PeterJuhasz()
	{
		return Parser.Parse(_markdown);
	}
}
