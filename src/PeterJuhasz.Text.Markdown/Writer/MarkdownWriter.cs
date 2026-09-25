using System.Buffers;
using System.Runtime.CompilerServices;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Writer;

public partial class MarkdownWriter<TWriter>(TWriter writer, MarkdownFormattingOptions? options = null) where TWriter : IBufferWriter<char>
{
	private readonly Stack<string> closePairs = new();
	private readonly TWriter writer = writer ?? throw new ArgumentNullException(nameof(writer));
	private readonly MarkdownFormattingOptions options = options ?? MarkdownFormattingOptions.Default;

	public MarkdownFormattingOptions FormattingOptions => options;
	internal TWriter Writer => writer;

	public void WriteText(ReadOnlySpan<char> text)
	{
		var span = text;
		int index;
		while ((index = span.IndexOfAny(Parser.Delimiters)) != -1)
		{
			writer.Write(span[..index]);
			writer.Write(['\\', span[index]]);
			span = span[(index + 1)..];
		}

		writer.Write(span);
	}

	public void WriteLine()
	{
		writer.Write(options.NewLine);
	}

	public void WriteMarkdown(ReadOnlySpan<char> markdown)
	{
		writer.Write(markdown);
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void OpenPair(string openPair, string closePair)
	{
		WriteMarkdown(openPair);
		closePairs.Push(closePair);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteBetweenPair(string openPair, ReadOnlySpan<char> text, string closePair)
	{
		WriteMarkdown(openPair);
		WriteText(text);
		WriteMarkdown(closePair);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void WriteMarkdownBetweenPair(string openPair, ReadOnlySpan<char> markdown, string closePair)
	{
		WriteMarkdown(openPair);
		WriteMarkdown(markdown);
		WriteMarkdown(closePair);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PushClosePair(string closePair)
	{
		closePairs.Push(closePair);
	}


	public void Close()
	{
		if (!closePairs.TryPop(out var pair))
		{
			throw new InvalidOperationException("No open pair to close.");
		}

		WriteMarkdown(pair);
	}
}

public class MarkdownWriter(IBufferWriter<char> writer) : MarkdownWriter<IBufferWriter<char>>(writer)
{
	public static MarkdownWriter<TWriter> Create<TWriter>(TWriter writer) where TWriter : IBufferWriter<char> => new(writer);
}
