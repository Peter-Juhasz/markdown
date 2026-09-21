using BenchmarkDotNet.Attributes;
using PeterJuhasz.Text.Html.Writer;
using System.Buffers;
using System.Text.Encodings.Web;
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
		var buffer = new ArrayBufferWriter<char>();
		var translator = new HtmlStringMarkdownTranslator<ArrayBufferWriter<char>>(new HtmlWriter<ArrayBufferWriter<char>>(buffer, HtmlEncoder.Default));
		translator.VisitDocument(_markdown);
		return new string(buffer.WrittenSpan);
	}
}
