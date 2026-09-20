using Microsoft.Extensions.Primitives;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	public static DocumentNode Parse(string document)
	{
		var visitor = new ModelMarkdownTranslator();
		visitor.VisitDocument(document);
		return visitor.ToModel();
	}


	internal static BlockParser ParseDocument(Segment document) => new(document);

	internal static UnorderedListParser ParseUnorderedListItems(Segment list) => new(list);

	internal static OrderedListParser ParseOrderedListItems(Segment list) => new(list);

	internal static BlockQuoteParser ParseBlockQuote(Segment list) => new(list);

	internal static SpoilerParser ParseSpoiler(Segment list) => new(list);

	internal static TableParser ParseTableRows(Segment table) => new(table);

	internal static TableRowParser ParseTableCells(Segment row) => new(row);

	internal static InlineParser ParseInline(Segment inline, NodeType parentNode, NodeType disallowedNodeTypes = default) => new(inline, parentNode, disallowedNodeTypes);
}
