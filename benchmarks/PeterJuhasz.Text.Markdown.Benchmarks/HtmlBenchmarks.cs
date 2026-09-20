using BenchmarkDotNet.Attributes;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Benchmarks;

using MarkdigMarkdown = Markdig.Markdown;

/// <summary>
/// Translates the sample document from markdown to an HTML string.
/// </summary>
/// <remarks>
/// Markdig pools its HTML renderer between calls, while a translator is allocated per call here,
/// which is how the API is meant to be used. The comparison is biased towards Markdig by whatever
/// that pooling saves.
/// </remarks>
[MemoryDiagnoser]
public class HtmlBenchmarks
{
	private string _markdown = null!;

	[GlobalSetup]
	public void Setup()
	{
		_markdown = SampleDocument.Read();
	}

	[Benchmark(Baseline = true, Description = "Markdig")]
	public string Markdig()
	{
		return MarkdigMarkdown.ToHtml(_markdown, MarkdigPipeline.Default);
	}

	[Benchmark(Description = "PeterJuhasz.Text.Markdown")]
	public string PeterJuhaszNewTranslator()
	{
		var translator = new HtmlStringMarkdownTranslator();
		translator.VisitDocument(_markdown);
		return translator.ToString();
	}
}
