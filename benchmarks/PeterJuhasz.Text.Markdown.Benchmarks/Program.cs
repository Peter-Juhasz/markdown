using BenchmarkDotNet.Running;
using PeterJuhasz.Text.Markdown.Benchmarks;

if (args is ["verify", ..])
{
	SampleDocumentReport.Print();
	return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

/// <summary>
/// The benchmark host.
/// </summary>
public partial class Program;
