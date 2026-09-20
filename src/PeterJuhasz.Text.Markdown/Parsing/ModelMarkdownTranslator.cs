using Microsoft.Extensions.Primitives;
using System.Text.Markdown.Model;

namespace System.Text.Markdown.Parsing;

using Segment = StringSegment;

public class ModelMarkdownTranslator : InplaceMarkdownVisitor
{
	private List<BlockNode> _blockParent = null!;
	private List<InlineNode>? _inlineParent;
	private List<UnorderedListItemNode>? _unorderedListParent;
	private List<OrderedListItemNode>? _orderedListParent;
	private List<TableRowNode>? _tableParent;
	private List<TableCellNode>? _tableRowParent;
	private TableRowNode? _tableHeader;
	private TableRowNode? _tableFooter;
	private List<TableCellAlignment>? _tableColumnAlignments;

	private Dictionary<Segment, MentionNode>? _mentionNodeCache;
	private Dictionary<Segment, HashtagNode>? _hashtagNodeCache;
	private Dictionary<Segment, EmojiAliasNode>? _emojiAliasNodeCache;
	private Dictionary<Segment, EmojiSmileyNode>? _emojiSmileyNodeCache;
	private Dictionary<Segment, string>? _stringInternCache;
	private static CheckboxNode? _checkedNode;
	private static CheckboxNode? _uncheckedNode;

	protected override void VisitDocument(Node node)
	{
		_blockParent = new(capacity: 1);
		base.VisitDocument(node);
	}

	protected override void VisitHeading(Node node, int depth)
	{
		var previousParent = _inlineParent;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new HeadingNode(depth, _inlineParent);
		_inlineParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitParagraph(Node node)
	{
		var previousParent = _inlineParent;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new ParagraphNode(_inlineParent);
		_inlineParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitBlockQuote(Node node)
	{
		var previousParent = _blockParent;
		_blockParent = new(capacity: 1);
		VisitInner(node);
		var heading = new BlockQuoteNode(_blockParent);
		_blockParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitSpoiler(Node node)
	{
		var previousParent = _blockParent;
		_blockParent = new(capacity: 1);
		VisitInner(node);
		var heading = new SpoilerNode(_blockParent);
		_blockParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitBold(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new BoldNode(_inlineParent);
		_inlineParent = previousParent;
		_inlineParent.Add(heading);
	}

	protected override void VisitItalic(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new ItalicNode(_inlineParent);
		_inlineParent = previousParent;
		_inlineParent.Add(heading);
	}

	protected override void VisitUnderline(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new UnderlineNode(_inlineParent);
		_inlineParent = previousParent;
		_inlineParent.Add(heading);
	}

	protected override void VisitStrikethrough(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new StrikethroughNode(_inlineParent);
		_inlineParent = previousParent;
		_inlineParent.Add(heading);
	}

	protected override void VisitEmailAddress(Node node, Segment emailAddress) => _inlineParent!.Add(new EmailAddressNode(emailAddress.Value!));

	protected override void VisitPhoneNumber(Node node, Segment phoneNumber) => _inlineParent!.Add(new PhoneNumberNode(phoneNumber.Value!));

	protected override void VisitUrl(Node node, Segment url) => _inlineParent!.Add(new UrlNode(url.Value!));

	protected override void VisitLink(Node node, Segment url)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new LinkNode(_inlineParent, url.Value!);
		_inlineParent = previousParent;
		_inlineParent.Add(heading);
	}

	protected override void VisitEmbed(Node node, Segment scheme, Segment id)
	{
		_stringInternCache ??= [];
		if (!_stringInternCache.TryGetValue(scheme, out var schemeString))
		{
			schemeString = scheme.Value!;
		}

		var heading = new EmbedNode(schemeString, id.Value!);
		_blockParent.Add(heading);
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

	protected override void VisitHorizontalRule(Node node) => _blockParent.Add(HorizontalRuleNode.Instance);

	protected override void VisitInlineCode(Node node, Segment code) => _inlineParent!.Add(new InlineCodeNode(code.Value!));

	protected override void VisitCodeBlock(Node node, Segment code, Segment language)
	{
		_stringInternCache ??= [];
		if (!_stringInternCache.TryGetValue(language, out var languageString))
		{
			languageString = language.Value!;
		}

		_blockParent.Add(new CodeBlockNode(code.Value!, languageString));
	}

	protected override void VisitInlineMath(Node node, Segment math) => _inlineParent!.Add(new InlineMathNode(math.Value!));

	protected override void VisitMathBlock(Node node, Segment math) => _blockParent.Add(new MathBlockNode(math.Value!));

	protected override void VisitEmptyLine(Node node) => _blockParent.Add(EmptyLineNode.Instance);

	protected override void VisitUnorderedList(Node node)
	{
		var previousParent = _unorderedListParent;
		_unorderedListParent = new(capacity: 3);
		VisitInner(node);
		var heading = new UnorderedListNode(_unorderedListParent);
		_unorderedListParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitUnorderedListItem(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new UnorderedListItemNode(_inlineParent);
		_inlineParent = previousParent;
		_unorderedListParent!.Add(heading);
	}

	protected override void VisitOrderedList(Node node)
	{
		var previousParent = _orderedListParent;
		_orderedListParent = new(capacity: 3);
		VisitInner(node);
		var heading = new OrderedListNode(_orderedListParent);
		_orderedListParent = previousParent;
		_blockParent.Add(heading);
	}

	protected override void VisitOrderedListItem(Node node)
	{
		var previousParent = _inlineParent!;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var heading = new OrderedListItemNode(_inlineParent);
		_inlineParent = previousParent;
		_orderedListParent!.Add(heading);
	}

	protected override void VisitTable(Node node)
	{
		var previousParent = _tableParent;
		var previousHeader = _tableHeader;
		var previousFooter = _tableFooter;
		var previousAlignments = _tableColumnAlignments;
		_tableParent = new(capacity: 3);
		_tableHeader = null;
		_tableFooter = null;
		_tableColumnAlignments = null;
		VisitInner(node);
		var table = new TableBlockNode(_tableHeader, _tableParent, _tableFooter, _tableColumnAlignments);
		_tableParent = previousParent;
		_tableHeader = previousHeader;
		_tableFooter = previousFooter;
		_tableColumnAlignments = previousAlignments;
		_blockParent.Add(table);
	}

	protected override void VisitTableRow(Node node) => _tableParent!.Add(BuildTableRow(node));

	protected override void VisitTableHeaderRow(Node node) => _tableHeader = BuildTableRow(node);

	protected override void VisitTableFooterRow(Node node) => _tableFooter = BuildTableRow(node);

	private TableRowNode BuildTableRow(Node node)
	{
		var previousParent = _tableRowParent;
		_tableRowParent = new(capacity: 3);
		VisitInner(node);
		var row = new TableRowNode(_tableRowParent);
		_tableRowParent = previousParent;
		return row;
	}

	protected override void VisitTableCell(Node node, TableCellAlignment? alignment)
	{
		var column = _tableRowParent!.Count;

		var previousParent = _inlineParent;
		_inlineParent = new(capacity: 1);
		VisitInner(node);
		var cell = new TableCellNode(_inlineParent);
		_inlineParent = previousParent;
		_tableRowParent.Add(cell);

		if (alignment is not null)
		{
			RecordTableColumnAlignment(column, alignment.Value);
		}
	}

	private void RecordTableColumnAlignment(int column, TableCellAlignment alignment)
	{
		_tableColumnAlignments ??= new(capacity: column + 1);

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
		var document = new DocumentNode(_blockParent);
		_blockParent = null!;
		return document;
	}
}
