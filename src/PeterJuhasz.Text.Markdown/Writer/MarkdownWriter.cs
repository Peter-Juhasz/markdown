using System.Buffers;
using System.Runtime.CompilerServices;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Writer;

public partial class MarkdownWriter<TWriter>(TWriter writer) where TWriter : IBufferWriter<char>
{
	private readonly Stack<string> closePairs = new();

	public void WriteText(string text)
	{
		var span = text.AsSpan();
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
		writer.Write(Environment.NewLine);
	}

	public void WriteLine(string text)
	{
		WriteText(text);
		WriteLine();
	}

	public void WriteMarkdown(string markdown)
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
	internal void WriteBetweenPair(string openPair, string text, string closePair)
	{
		WriteMarkdown(openPair);
		WriteText(text);
		WriteMarkdown(closePair);
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

public static partial class Extensions
{
	extension<TWriter>(MarkdownWriter<TWriter> writer) where TWriter : IBufferWriter<char>
	{
		public void WriteBold(string text) => writer.WriteBetweenPair("**", text, "**");

		public void OpenBold() => writer.OpenPair("**", "**");


		public void WriteLink(string text, string url)
		{
			writer.WriteBetweenPair("[", text, $"]");
			writer.WriteBetweenPair("(", url, ")");
		}

		public void OpenLinkText() => writer.OpenPair("[", "]");

		public void OpenLinkUrl() => writer.OpenPair("(", ")");
	}
}