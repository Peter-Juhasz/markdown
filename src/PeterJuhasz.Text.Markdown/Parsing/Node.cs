using Microsoft.Extensions.Primitives;

namespace PeterJuhasz.Text.Markdown.Parsing;

using Segment = StringSegment;

public readonly ref struct Node(NodeType type, Segment raw)
{
	public readonly NodeType Type => type;

	public readonly Segment FullSegment => raw;
}