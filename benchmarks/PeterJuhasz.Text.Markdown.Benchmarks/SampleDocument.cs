using System.Runtime.CompilerServices;

namespace System.Text.Markdown.Benchmarks;

/// <summary>
/// Locates and reads the markdown document all benchmarks run on.
/// </summary>
/// <remarks>
/// The file is read from disk in the global setup phase of each benchmark, so that reading it
/// is never part of a measured iteration.
/// </remarks>
internal static class SampleDocument
{
	public const string FileName = "Sample.md";

	/// <summary>
	/// Reads the sample document from disk.
	/// </summary>
	public static string Read() => File.ReadAllText(ResolvePath());

	private static string ResolvePath()
	{
		foreach (var candidate in GetCandidatePaths())
		{
			if (File.Exists(candidate))
			{
				return candidate;
			}
		}

		throw new FileNotFoundException(
			$"Could not find '{FileName}'. Searched: {String.Join(", ", GetCandidatePaths())}",
			FileName
		);
	}

	private static IEnumerable<string> GetCandidatePaths()
	{
		// next to the source file, which is where BenchmarkDotNet's generated host project
		// runs from a different output directory than the one the file was copied to
		yield return Path.Combine(GetSourceDirectory(), FileName);

		// next to the benchmark assembly, for a published or copied build
		yield return Path.Combine(AppContext.BaseDirectory, FileName);

		// relative to the current working directory
		yield return Path.GetFullPath(FileName);
	}

	private static string GetSourceDirectory([CallerFilePath] string sourceFilePath = "") =>
		Path.GetDirectoryName(sourceFilePath) ?? AppContext.BaseDirectory;
}
