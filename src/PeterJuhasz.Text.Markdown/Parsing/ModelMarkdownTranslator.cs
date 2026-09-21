using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public class ModelMarkdownTranslator : InplaceMarkdownVisitor
{
	private Scratch<BlockNode> _blockParent;
	private Scratch<InlineNode> _inlineParent;
	private Scratch<UnorderedListItemNode> _unorderedListParent;
	private Scratch<OrderedListItemNode> _orderedListParent;
	private Scratch<TableRowNode> _tableParent;
	private Scratch<TableCellNode> _tableRowParent;
	private TableRowNode? _tableHeader;
	private TableRowNode? _tableFooter;
	private ImmutableArray<TableCellAlignment>.Builder? _tableColumnAlignments;

	// the caches are keyed by the text which was already cut out of the document, looked up by span,
	// so that they hold no reference to the document they were filled from
	private Dictionary<string, MentionNode>? _mentionNodeCache;
	private Dictionary<string, HashtagNode>? _hashtagNodeCache;
	private Dictionary<string, EmojiAliasNode>? _emojiAliasNodeCache;
	private Dictionary<string, EmojiSmileyNode>? _emojiSmileyNodeCache;
	private HashSet<string>? _stringInternCache;
	private static CheckboxNode? _checkedNode;
	private static CheckboxNode? _uncheckedNode;

	protected override void VisitDocument(Node node)
	{
		// a translator may be reused, and a previous document may have been abandoned part way
		// through by an exception, so every buffer starts empty rather than where it was left
		ClearBuffers();

		// user names, tags and emojis are as unbounded as the documents they come from,
		// so those caches only ever serve the document they were filled from.
		// The interned strings are a small, closed vocabulary, and are kept.
		_mentionNodeCache?.Clear();
		_hashtagNodeCache?.Clear();
		_emojiAliasNodeCache?.Clear();
		_emojiSmileyNodeCache?.Clear();

		base.VisitDocument(node);
	}

	protected override void VisitHeading(Node node, int depth)
	{
		var heading = new HeadingNode(depth, DrainInner(node, ref _inlineParent));
		_blockParent.Add(heading);
	}

	protected override void VisitParagraph(Node node)
	{
		var paragraph = new ParagraphNode(DrainInner(node, ref _inlineParent));
		_blockParent.Add(paragraph);
	}

	protected override void VisitBlockQuote(Node node)
	{
		var blockQuote = new BlockQuoteNode(DrainInner(node, ref _blockParent));
		_blockParent.Add(blockQuote);
	}

	protected override void VisitSpoiler(Node node)
	{
		var spoiler = new SpoilerNode(DrainInner(node, ref _blockParent));
		_blockParent.Add(spoiler);
	}

	protected override void VisitAlert(Node node, Segment type)
	{
		// alert types are drawn from a small vocabulary which repeats across documents
		var alert = new AlertNode(Intern(type), DrainInner(node, ref _blockParent));
		_blockParent.Add(alert);
	}

	protected override void VisitDetails(Node node)
	{
		// the summary is collected as inline runs, and the content as blocks, so the two
		// stacks the visits inside push onto are drained one by one
		var summaryStart = _inlineParent.Count;
		var contentStart = _blockParent.Count;
		VisitInner(node);

		var details = new DetailsBlockNode(_inlineParent.DrainFrom(summaryStart), _blockParent.DrainFrom(contentStart));
		_blockParent.Add(details);
	}

	protected override void VisitBold(Node node)
	{
		var bold = new BoldNode(DrainInner(node, ref _inlineParent));
		_inlineParent.Add(bold);
	}

	protected override void VisitItalic(Node node)
	{
		var italic = new ItalicNode(DrainInner(node, ref _inlineParent));
		_inlineParent.Add(italic);
	}

	protected override void VisitUnderline(Node node)
	{
		var underline = new UnderlineNode(DrainInner(node, ref _inlineParent));
		_inlineParent.Add(underline);
	}

	protected override void VisitStrikethrough(Node node)
	{
		var strikethrough = new StrikethroughNode(DrainInner(node, ref _inlineParent));
		_inlineParent.Add(strikethrough);
	}

	protected override void VisitEmailAddress(Node node, Segment emailAddress) => _inlineParent.Add(new EmailAddressNode(emailAddress.Value!));

	protected override void VisitPhoneNumber(Node node, Segment phoneNumber) => _inlineParent.Add(new PhoneNumberNode(phoneNumber.Value!));

	protected override void VisitUrl(Node node, Segment url) => _inlineParent.Add(new UrlNode(url.Value!));

	protected override void VisitAngleBracketUrl(Node node, Segment url) => _inlineParent.Add(new AngleBracketUrlNode(url.Value!));

	protected override void VisitLink(Node node, Segment url, Segment title)
	{
		var link = new LinkNode(
			Runs: DrainInner(node, ref _inlineParent),
			Url: url.Value!,
			Title: title.Length > 0 ? Decode(title) : null
		);
		_inlineParent.Add(link);
	}

	protected override void VisitEmbed(Node node, Segment scheme, Segment id)
	{
		var heading = new EmbedNode(Intern(scheme), id.Value!);
		_blockParent.Add(heading);
	}

	protected override void VisitMention(Node node, Segment userName)
	{
		_mentionNodeCache ??= new(StringComparer.Ordinal);
		var lookup = _mentionNodeCache.GetAlternateLookup<ReadOnlySpan<char>>();
		if (!lookup.TryGetValue(userName.AsSpan(), out var model))
		{
			// the node holds the very string the cache is keyed by, so the key costs nothing of its own
			var name = userName.Value!;
			model = new MentionNode(name);
			_mentionNodeCache.Add(name, model);
		}

		_inlineParent.Add(model);
	}

	protected override void VisitHashtag(Node node, Segment tag)
	{
		_hashtagNodeCache ??= new(StringComparer.Ordinal);
		var lookup = _hashtagNodeCache.GetAlternateLookup<ReadOnlySpan<char>>();
		if (!lookup.TryGetValue(tag.AsSpan(), out var model))
		{
			var text = tag.Value!;
			model = new HashtagNode(text);
			_hashtagNodeCache.Add(text, model);
		}

		_inlineParent.Add(model);
	}

	protected override void VisitText(Node node, Segment text) => _inlineParent.Add(new TextNode(Decode(text)));

	protected override void VisitEmojiAlias(Node node, Segment alias)
	{
		_emojiAliasNodeCache ??= new(StringComparer.Ordinal);
		var lookup = _emojiAliasNodeCache.GetAlternateLookup<ReadOnlySpan<char>>();
		if (!lookup.TryGetValue(alias.AsSpan(), out var model))
		{
			var text = alias.Value!;
			model = new EmojiAliasNode(text);
			_emojiAliasNodeCache.Add(text, model);
		}

		_inlineParent.Add(model);
	}

	protected override void VisitEmojiSmiley(Node node, Segment smiley)
	{
		_emojiSmileyNodeCache ??= new(StringComparer.Ordinal);
		var lookup = _emojiSmileyNodeCache.GetAlternateLookup<ReadOnlySpan<char>>();
		if (!lookup.TryGetValue(smiley.AsSpan(), out var model))
		{
			var text = smiley.Value!;
			model = new EmojiSmileyNode(text);
			_emojiSmileyNodeCache.Add(text, model);
		}

		_inlineParent.Add(model);
	}

	protected override void VisitHorizontalRule(Node node) => _blockParent.Add(HorizontalRuleNode.Instance);

	protected override void VisitInlineCode(Node node, Segment code) => _inlineParent.Add(new InlineCodeNode(code.Value!));

	protected override void VisitCodeBlock(Node node, Segment code, Segment language)
	{
		_blockParent.Add(new CodeBlockNode(code.Value!, Intern(language)));
	}

	protected override void VisitInlineMath(Node node, Segment math) => _inlineParent.Add(new InlineMathNode(math.Value!));

	protected override void VisitMathBlock(Node node, Segment math) => _blockParent.Add(new MathBlockNode(math.Value!));

	protected override void VisitFrontMatter(Node node, Segment frontMatter) => _blockParent.Add(new FrontMatterNode(frontMatter.Value!));

	protected override void VisitComment(Node node, Segment comment) => _blockParent.Add(new CommentNode(comment.Value!));

	protected override void VisitInlineComment(Node node, Segment comment) => _inlineParent.Add(new InlineCommentNode(comment.Value!));

	protected override void VisitFootnoteContent(Node node, int number)
	{
		var footnote = new FootnoteContentNode(number, DrainInner(node, ref _inlineParent));
		_blockParent.Add(footnote);
	}

	protected override void VisitFootnoteReference(Node node, int number) => _inlineParent.Add(new FootnoteReferenceNode(number));

	protected override void VisitEmptyLine(Node node) => _blockParent.Add(EmptyLineNode.Instance);

	protected override void VisitUnorderedList(Node node)
	{
		var list = new UnorderedListNode(DrainInner(node, ref _unorderedListParent));
		_blockParent.Add(list);
	}

	protected override void VisitUnorderedListItem(Node node)
	{
		var listItem = new UnorderedListItemNode(DrainInner(node, ref _inlineParent));
		_unorderedListParent.Add(listItem);
	}

	protected override void VisitOrderedList(Node node)
	{
		var list = new OrderedListNode(DrainInner(node, ref _orderedListParent));
		_blockParent.Add(list);
	}

	protected override void VisitOrderedListItem(Node node)
	{
		var listItem = new OrderedListItemNode(DrainInner(node, ref _inlineParent));
		_orderedListParent.Add(listItem);
	}

	protected override void VisitTable(Node node)
	{
		var previousHeader = _tableHeader;
		var previousFooter = _tableFooter;
		var previousAlignments = _tableColumnAlignments;
		_tableHeader = null;
		_tableFooter = null;
		_tableColumnAlignments = null;

		var rows = DrainInner(node, ref _tableParent);
		var table = new TableBlockNode(_tableHeader, rows, _tableFooter, _tableColumnAlignments?.DrainToImmutable());

		_tableHeader = previousHeader;
		_tableFooter = previousFooter;
		_tableColumnAlignments = previousAlignments;
		_blockParent.Add(table);
	}

	protected override void VisitTableRow(Node node) => _tableParent.Add(BuildTableRow(node));

	protected override void VisitTableHeaderRow(Node node) => _tableHeader = BuildTableRow(node);

	protected override void VisitTableFooterRow(Node node) => _tableFooter = BuildTableRow(node);

	private TableRowNode BuildTableRow(Node node)
	{
		return new(DrainInner(node, ref _tableRowParent));
	}

	protected override void VisitTableCell(Node node, TableCellAlignment? alignment)
	{
		var column = _tableRowParent.Count;

		var cell = new TableCellNode(DrainInner(node, ref _inlineParent));
		_tableRowParent.Add(cell);

		if (alignment is not null)
		{
			RecordTableColumnAlignment(column, alignment.Value);
		}
	}

	private void RecordTableColumnAlignment(int column, TableCellAlignment alignment)
	{
		_tableColumnAlignments ??= ImmutableArray.CreateBuilder<TableCellAlignment>(initialCapacity: column + 1);

		// columns which declare no alignment of their own keep the default
		while (_tableColumnAlignments.Count <= column)
		{
			_tableColumnAlignments.Add(TableCellAlignment.Left);
		}

		_tableColumnAlignments[column] = alignment;
	}

	protected override void VisitCheckbox(Node node, bool isChecked)
	{
		if (isChecked)
		{
			_checkedNode ??= new CheckboxNode(true);
			_inlineParent.Add(_checkedNode);
		}
		else
		{
			_uncheckedNode ??= new CheckboxNode(false);
			_inlineParent.Add(_uncheckedNode);
		}
	}

	public DocumentNode ToModel()
	{
		var document = new DocumentNode(_blockParent.DrainFrom(0));

		// the buffers still hold what was drained out of them, which is the whole document,
		// so they are emptied here rather than only when the next document arrives
		ClearBuffers();

		return document;
	}

	/// <summary>
	/// Collects the children of <paramref name="node"/> onto a shared stack, and drains that stack into an immutable array.
	/// </summary>
	/// <remarks>
	/// Only the items this node pushed are drained, so the ones its parent pushed before it are left alone,
	/// which is how nested nodes of the same kind share a single buffer.
	/// </remarks>
	private ImmutableArray<T> DrainInner<T>(Node node, ref Scratch<T> scratch)
	{
		var start = scratch.Count;
		VisitInner(node);
		return scratch.DrainFrom(start);
	}

	/// <summary>
	/// Cuts <paramref name="segment"/> out of the document, reusing a string which was cut before if there is one.
	/// </summary>
	/// <remarks>
	/// Code block languages and embed schemes are drawn from a small vocabulary which repeats within a document
	/// and across documents. Only the cut string is kept, never the segment, so the cache holds no reference
	/// to any document it was filled from, and outlives them.
	/// </remarks>
	private string Intern(Segment segment)
	{
		_stringInternCache ??= new(StringComparer.Ordinal);

		var lookup = _stringInternCache.GetAlternateLookup<ReadOnlySpan<char>>();
		if (lookup.TryGetValue(segment.AsSpan(), out var interned))
		{
			return interned;
		}

		interned = string.Intern(segment.Value!);
		_stringInternCache.Add(interned);
		return interned;
	}

	/// <summary>
	/// Empties every buffer, so that nothing of the document which was translated is kept alive by the translator.
	/// </summary>
	private void ClearBuffers()
	{
		_blockParent.Clear();
		_inlineParent.Clear();
		_unorderedListParent.Clear();
		_orderedListParent.Clear();
		_tableParent.Clear();
		_tableRowParent.Clear();
		_tableHeader = null;
		_tableFooter = null;
		_tableColumnAlignments = null;
	}


	/// <summary>
	/// A growable stack which collects the children of every node of a kind.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Nodes nested into each other share a single buffer, because a node is always drained
	/// before its parent is, which makes its children the topmost ones on the stack.
	/// </para>
	/// <para>
	/// This is a mutable struct, so that a translator holds no buffer objects of its own.
	/// It is always held in a field and handed over by reference, never copied.
	/// </para>
	/// </remarks>
	private struct Scratch<T>
	{
		private const int InitialCapacity = 4;

		private T[]? _items;

		public int Count { get; private set; }

		public void Add(T item)
		{
			// the buffer is only rented by the first item, so an unused kind costs nothing at all
			var items = _items;
			if (items is null || Count == items.Length)
			{
				Array.Resize(ref items, Math.Max(InitialCapacity, 2 * (items?.Length ?? 0)));
				_items = items;
			}

			items[Count++] = item;
		}

		/// <summary>
		/// Drops everything, including what the buffer still holds above the top of the stack,
		/// so that a translator which is kept around holds on to no node of a document it is done with.
		/// </summary>
		public void Clear()
		{
			// drained items are left in the buffer, and would be kept alive until they are written over
			if (_items is not null && RuntimeHelpers.IsReferenceOrContainsReferences<T>())
			{
				Array.Clear(_items);
			}

			Count = 0;
		}

		/// <summary>
		/// Hands the items pushed since <paramref name="start"/> over to an immutable array, and pops them.
		/// </summary>
		public ImmutableArray<T> DrainFrom(int start)
		{
			var count = Count - start;
			Count = start;

			if (count == 0)
			{
				return [];
			}

			// the array is built to size and never handed out anywhere else, so it can be wrapped as it is
			return ImmutableCollectionsMarshal.AsImmutableArray(_items.AsSpan(start, count).ToArray());
		}
	}
}
