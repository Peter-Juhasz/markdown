using Microsoft.Extensions.Primitives;
using PeterJuhasz.Text.Markdown.Model;

namespace PeterJuhasz.Text.Markdown.Parsing;

using Segment = StringSegment;

public static partial class Parser
{
	/// <summary>
	/// The translator the calling thread parsed with last, kept so that its buffers and its interned
	/// strings serve the next document too. It is held per thread, so parsing on several threads is safe.
	/// </summary>
	[ThreadStatic]
	private static ModelMarkdownTranslator? _translator;

	public static DocumentNode Parse(string document)
	{
		// the translator is taken for the duration of the parse, so a nested parse builds its own
		// rather than writing over the state of the one which is running
		var visitor = _translator ?? new ModelMarkdownTranslator();
		_translator = null;

		visitor.VisitDocument(document);
		var model = visitor.ToModel();

		// only a translator which finished is handed back, so a document abandoned
		// part way through by an exception leaves nothing behind for the next one
		_translator = visitor;
		return model;
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
