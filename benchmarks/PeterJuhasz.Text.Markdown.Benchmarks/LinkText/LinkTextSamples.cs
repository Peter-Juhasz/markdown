namespace PeterJuhasz.Text.Markdown.Benchmarks.LinkText;

/// <summary>
/// The shapes of link text the candidates are measured on.
/// </summary>
public enum LinkTextScenario
{
	/// <summary>
	/// Short texts without brackets, which is the most common case, like <c>[LICENSE](url)</c>.
	/// </summary>
	Short,

	/// <summary>
	/// Texts without brackets which are as long as a sentence.
	/// </summary>
	Long,

	/// <summary>
	/// Texts with matched pairs of brackets in them, like <c>[link [foo] bar](url)</c>.
	/// </summary>
	Nested,

	/// <summary>
	/// Texts with escaped brackets in them, like <c>[link \[bar](url)</c>.
	/// </summary>
	Escaped,

	/// <summary>
	/// An opening bracket which is never closed, so the rest of the paragraph is searched.
	/// </summary>
	Unclosed,
}

/// <summary>
/// An input which starts at the opening bracket of a link text, and continues until the end of its paragraph.
/// </summary>
internal sealed record LinkTextSample(string Input, int ExpectedEndIndex);

/// <summary>
/// Creates the inputs of each scenario, and checks that the candidates find the end of them.
/// </summary>
internal static class LinkTextSamples
{
	/// <summary>
	/// The number of samples in each scenario, so that the time of a single search can be reported.
	/// </summary>
	public const int Count = 8;

	private const string Url = "(https://example.org/docs/page)";

	private const string Tail = " for the details, and then the paragraph goes on with some more prose, like most paragraphs in a document do.";

	public static LinkTextSample[] Create(LinkTextScenario scenario) => scenario switch
	{
		LinkTextScenario.Short => Closed(
			"LICENSE",
			"docs",
			"code of conduct",
			"contributing guide",
			"the API reference",
			"issue tracker",
			"release notes",
			"Markdig"
		),
		LinkTextScenario.Long => Closed(
			"the detailed specification of the orbit propagation model used by the simulator",
			"a step by step guide which explains how to set up the development environment",
			"the original paper describing the algorithm, published in the proceedings of the conference",
			"the list of known issues and the workarounds for each of them, updated with every release",
			"everything you need to know about configuring the parser, including all of its extensions",
			"this comment on the pull request which explains why the previous approach did not work",
			"the benchmark results which compare the performance of the parser to other libraries",
			"a long link text which keeps going and going, because some authors link whole sentences"
		),
		LinkTextScenario.Nested => Closed(
			"link [foo] bar",
			"see [1] and [2]",
			"a [nested [deeply] pair] here",
			"[x] done",
			"array [index] access",
			"the [RFC 3986] grammar",
			"[2024] annual report",
			"foo [bar]"
		),
		LinkTextScenario.Escaped => Closed(
			@"link \[bar",
			@"array\[0\] access",
			@"a \] literal",
			@"\[draft\] proposal",
			@"backslash \\",
			@"x\]y",
			@"open \[ only",
			@"close \] only"
		),
		LinkTextScenario.Unclosed => Unclosed(
			"sic",
			"note: this bracket is never closed",
			"1",
			"citation needed",
			"draft",
			"TODO: link",
			"see also",
			"x"
		),
		_ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null),
	};

	/// <summary>
	/// Checks that each candidate finds the expected end of each sample, so that none of them is measured while being wrong.
	/// </summary>
	public static void Verify(LinkTextScenario scenario, LinkTextSample[] samples)
	{
		if (samples.Length != Count)
		{
			throw new InvalidOperationException($"Scenario '{scenario}' has {samples.Length} samples instead of {Count}.");
		}

		foreach (var sample in samples)
		{
			Verify(sample, nameof(LinkTextEnd.SearchValuesWalk), LinkTextEnd.SearchValuesWalk(sample.Input));
			Verify(sample, nameof(LinkTextEnd.IndexOfAnyWalk), LinkTextEnd.IndexOfAnyWalk(sample.Input));
			Verify(sample, nameof(LinkTextEnd.IndexOfThenLookBack), LinkTextEnd.IndexOfThenLookBack(sample.Input));
			Verify(sample, nameof(LinkTextEnd.ScalarWalk), LinkTextEnd.ScalarWalk(sample.Input));

			// the current implementation does not support nested pairs, which is what the others are meant to fix
			if (scenario is not LinkTextScenario.Nested)
			{
				Verify(sample, nameof(LinkTextEnd.FirstClose), LinkTextEnd.FirstClose(sample.Input));
			}
		}
	}

	private static void Verify(LinkTextSample sample, string candidate, int actualEndIndex)
	{
		if (actualEndIndex != sample.ExpectedEndIndex)
		{
			throw new InvalidOperationException($"{candidate} found {actualEndIndex} instead of {sample.ExpectedEndIndex} in '{sample.Input}'.");
		}
	}

	private static LinkTextSample[] Closed(params string[] texts) =>
		[.. texts.Select(text => new LinkTextSample(Input: $"[{text}]{Url}{Tail}", ExpectedEndIndex: 1 + text.Length))];

	private static LinkTextSample[] Unclosed(params string[] texts) =>
		[.. texts.Select(text => new LinkTextSample(Input: $"[{text}{Tail}", ExpectedEndIndex: -1))];
}
