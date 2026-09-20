using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public class ModelMarkdownTranslator : InplaceMarkdownVisitor
{
	private ImmutableArray<BlockNode>.Builder? _blockParent;
	private ImmutableArray<InlineNode>.Builder? _inlineParent;
	private ImmutableArray<UnorderedListItemNode>.Builder? _unorderedListParent;
	private ImmutableArray<OrderedListItemNode>.Builder? _orderedListParent;
	private ImmutableArray<TableRowNode>.Builder? _tableParent;
	private ImmutableArray<TableCellNode>.Builder? _tableRowParent;
	private TableRowNode? _tableHeader;
	private TableRowNode? _tableFooter;
	private ImmutableArray<TableCellAlignment>.Builder? _tableColumnAlignments;

	private Dictionary<Segment, MentionNode>? _mentionNodeCache;
	private Dictionary<Segment, HashtagNode>? _hashtagNodeCache;
	private Dictionary<Segment, EmojiAliasNode>? _emojiAliasNodeCache;
	private Dictionary<Segment, EmojiSmileyNode>? _emojiSmileyNodeCache;
	private Dictionary<Segment, string>? _stringInternCache;
	private static CheckboxNode? _checkedNode;
	private static CheckboxNode? _uncheckedNode;

	/// <summary>
	/// Collects the children of <paramref name="node"/> into a fresh buffer, and drains that buffer into an immutable array.
	/// </summary>
	/// <remarks>
	/// The previous buffer is restored afterwards, so nested nodes of the same kind can be translated.
	/// </remarks>
	private ImmutableArray<T> DrainInner<T>(Node node, ref ImmutableArray<T>.Builder? buffer, int capacity)
	{
		var previousParent = buffer;
		var parent = ImmutableArray.CreateBuilder<T>(capacity);
		buffer = parent;
		VisitInner(node);
		buffer = previousParent;
		return parent.DrainToImmutable();
	}

	protected override void VisitDocument(Node node)
	{
		_blockParent = ImmutableArray.CreateBuilder<BlockNode>(initialCapacity: 1);
		base.VisitDocument(node);
	}

	protected override void VisitHeading(Node node, int depth)
	{
		var heading = new HeadingNode(depth, DrainInner(node, ref _inlineParent, capacity: 1));
		_blockParent!.Add(heading);
	}

	protected override void VisitParagraph(Node node)
	{
		var paragraph = new ParagraphNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_blockParent!.Add(paragraph);
	}

	protected override void VisitBlockQuote(Node node)
	{
		var blockQuote = new BlockQuoteNode(DrainInner(node, ref _blockParent, capacity: 1));
		_blockParent!.Add(blockQuote);
	}

	protected override void VisitSpoiler(Node node)
	{
		var spoiler = new SpoilerNode(DrainInner(node, ref _blockParent, capacity: 1));
		_blockParent!.Add(spoiler);
	}

	protected override void VisitBold(Node node)
	{
		var bold = new BoldNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_inlineParent!.Add(bold);
	}

	protected override void VisitItalic(Node node)
	{
		var italic = new ItalicNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_inlineParent!.Add(italic);
	}

	protected override void VisitUnderline(Node node)
	{
		var underline = new UnderlineNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_inlineParent!.Add(underline);
	}

	protected override void VisitStrikethrough(Node node)
	{
		var strikethrough = new StrikethroughNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_inlineParent!.Add(strikethrough);
	}

	protected override void VisitEmailAddress(Node node, Segment emailAddress) => _inlineParent!.Add(new EmailAddressNode(emailAddress.Value!));

	protected override void VisitPhoneNumber(Node node, Segment phoneNumber) => _inlineParent!.Add(new PhoneNumberNode(phoneNumber.Value!));

	protected override void VisitUrl(Node node, Segment url) => _inlineParent!.Add(new UrlNode(url.Value!));

	protected override void VisitAngleBracketUrl(Node node, Segment url) => _inlineParent!.Add(new AngleBracketUrlNode(url.Value!));

	protected override void VisitLink(Node node, Segment url, Segment title)
	{
		var link = new LinkNode(
			Runs: DrainInner(node, ref _inlineParent, capacity: 1),
			Url: url.Value!,
			Title: title.Length > 0 ? Decode(title) : null
		);
		_inlineParent!.Add(link);
	}

	protected override void VisitEmbed(Node node, Segment scheme, Segment id)
	{
		_stringInternCache ??= [];
		if (!_stringInternCache.TryGetValue(scheme, out var schemeString))
		{
			schemeString = scheme.Value!;
		}

		var heading = new EmbedNode(schemeString, id.Value!);
		_blockParent!.Add(heading);
	}

	protected override void VisitMention(Node node, Segment userName)
	{
		_mentionNodeCache ??= [];
		if (!_mentionNodeCache.TryGetValue(userName, out var model))
		{
			model = new MentionNode(userName.Value!);
		}

		_inlineParent!.Add(model);
	}

	protected override void VisitHashtag(Node node, Segment tag)
	{
		_hashtagNodeCache ??= [];
		if (!_hashtagNodeCache.TryGetValue(tag, out var model))
		{
			model = new HashtagNode(tag.Value!);
		}

		_inlineParent!.Add(model);
	}

	protected override void VisitText(Node node, Segment text) => _inlineParent!.Add(new TextNode(Decode(text)));

	protected override void VisitEmojiAlias(Node node, Segment alias)
	{
		_emojiAliasNodeCache ??= [];
		if (!_emojiAliasNodeCache.TryGetValue(alias, out var model))
		{
			model = new EmojiAliasNode(alias.Value!);
		}

		_inlineParent!.Add(model);
	}

	protected override void VisitEmojiSmiley(Node node, Segment smiley)
	{
		_emojiSmileyNodeCache ??= [];
		if (!_emojiSmileyNodeCache.TryGetValue(smiley, out var model))
		{
			model = new EmojiSmileyNode(smiley.Value!);
		}

		_inlineParent!.Add(model);
	}

	protected override void VisitHorizontalRule(Node node) => _blockParent!.Add(HorizontalRuleNode.Instance);

	protected override void VisitInlineCode(Node node, Segment code) => _inlineParent!.Add(new InlineCodeNode(code.Value!));

	protected override void VisitCodeBlock(Node node, Segment code, Segment language)
	{
		_stringInternCache ??= [];
		if (!_stringInternCache.TryGetValue(language, out var languageString))
		{
			languageString = language.Value!;
		}

		_blockParent!.Add(new CodeBlockNode(code.Value!, languageString));
	}

	protected override void VisitInlineMath(Node node, Segment math) => _inlineParent!.Add(new InlineMathNode(math.Value!));

	protected override void VisitMathBlock(Node node, Segment math) => _blockParent!.Add(new MathBlockNode(math.Value!));

	protected override void VisitEmptyLine(Node node) => _blockParent!.Add(EmptyLineNode.Instance);

	protected override void VisitUnorderedList(Node node)
	{
		var list = new UnorderedListNode(DrainInner(node, ref _unorderedListParent, capacity: 3));
		_blockParent!.Add(list);
	}

	protected override void VisitUnorderedListItem(Node node)
	{
		var listItem = new UnorderedListItemNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_unorderedListParent!.Add(listItem);
	}

	protected override void VisitOrderedList(Node node)
	{
		var list = new OrderedListNode(DrainInner(node, ref _orderedListParent, capacity: 3));
		_blockParent!.Add(list);
	}

	protected override void VisitOrderedListItem(Node node)
	{
		var listItem = new OrderedListItemNode(DrainInner(node, ref _inlineParent, capacity: 1));
		_orderedListParent!.Add(listItem);
	}

	protected override void VisitTable(Node node)
	{
		var previousHeader = _tableHeader;
		var previousFooter = _tableFooter;
		var previousAlignments = _tableColumnAlignments;
		_tableHeader = null;
		_tableFooter = null;
		_tableColumnAlignments = null;

		var rows = DrainInner(node, ref _tableParent, capacity: 3);
		var table = new TableBlockNode(_tableHeader, rows, _tableFooter, _tableColumnAlignments?.DrainToImmutable());

		_tableHeader = previousHeader;
		_tableFooter = previousFooter;
		_tableColumnAlignments = previousAlignments;
		_blockParent!.Add(table);
	}

	protected override void VisitTableRow(Node node) => _tableParent!.Add(BuildTableRow(node));

	protected override void VisitTableHeaderRow(Node node) => _tableHeader = BuildTableRow(node);

	protected override void VisitTableFooterRow(Node node) => _tableFooter = BuildTableRow(node);

	private TableRowNode BuildTableRow(Node node)
	{
		return new(DrainInner(node, ref _tableRowParent, capacity: 3));
	}

	protected override void VisitTableCell(Node node, TableCellAlignment? alignment)
	{
		var column = _tableRowParent!.Count;

		var cell = new TableCellNode(DrainInner(node, ref _inlineParent, capacity: 1));
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
			_inlineParent!.Add(_checkedNode);
		}
		else
		{
			_uncheckedNode ??= new CheckboxNode(false);
			_inlineParent!.Add(_uncheckedNode);
		}
	}

	public DocumentNode ToModel()
	{
		var document = new DocumentNode(_blockParent!.DrainToImmutable());
		_blockParent = null;
		return document;
	}
}
