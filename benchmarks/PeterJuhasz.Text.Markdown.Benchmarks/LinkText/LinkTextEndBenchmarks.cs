using BenchmarkDotNet.Attributes;

namespace PeterJuhasz.Text.Markdown.Benchmarks.LinkText;

/// <summary>
/// Compares ways to find the end of the text of a link, which may contain matched pairs of brackets.
/// </summary>
/// <remarks>
/// Each iteration searches all samples of a scenario, so that the inputs vary in length like they do in a document,
/// and the reported time is the average of a single search.
/// </remarks>
public class LinkTextEndBenchmarks
{
	private string[] _inputs = null!;

	[ParamsAllValues]
	public LinkTextScenario Scenario { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var samples = LinkTextSamples.Create(Scenario);
		LinkTextSamples.Verify(Scenario, samples);
		_inputs = [.. samples.Select(sample => sample.Input)];
	}

	[Benchmark(Baseline = true, OperationsPerInvoke = LinkTextSamples.Count)]
	public int FirstClose()
	{
		var sum = 0;
		foreach (var input in _inputs)
		{
			sum += LinkTextEnd.FirstClose(input);
		}

		return sum;
	}

	[Benchmark(OperationsPerInvoke = LinkTextSamples.Count)]
	public int SearchValuesWalk()
	{
		var sum = 0;
		foreach (var input in _inputs)
		{
			sum += LinkTextEnd.SearchValuesWalk(input);
		}

		return sum;
	}

	[Benchmark(OperationsPerInvoke = LinkTextSamples.Count)]
	public int IndexOfAnyWalk()
	{
		var sum = 0;
		foreach (var input in _inputs)
		{
			sum += LinkTextEnd.IndexOfAnyWalk(input);
		}

		return sum;
	}

	[Benchmark(OperationsPerInvoke = LinkTextSamples.Count)]
	public int IndexOfThenLookBack()
	{
		var sum = 0;
		foreach (var input in _inputs)
		{
			sum += LinkTextEnd.IndexOfThenLookBack(input);
		}

		return sum;
	}

	[Benchmark(OperationsPerInvoke = LinkTextSamples.Count)]
	public int ScalarWalk()
	{
		var sum = 0;
		foreach (var input in _inputs)
		{
			sum += LinkTextEnd.ScalarWalk(input);
		}

		return sum;
	}
}
