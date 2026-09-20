namespace System.Text.Markdown.Model;


public abstract record class Node();

public abstract record class InlineNode() : Node;

public abstract record class BlockNode() : Node;


public record class DocumentNode(IReadOnlyList<BlockNode> Blocks) : BlockNode;

public record class ParagraphNode(IReadOnlyList<InlineNode> Runs) : BlockNode;

public record class HeadingNode(int Level, IReadOnlyList<InlineNode> Runs) : BlockNode;

public record class BlockQuoteNode(IReadOnlyList<BlockNode> Blocks) : BlockNode;

public record class SpoilerNode(IReadOnlyList<BlockNode> Blocks) : BlockNode;

public record class CodeBlockNode(string Code, string Language) : BlockNode;

public record class MathBlockNode(string Expression) : BlockNode;

public record class EmbedNode(string Scheme, string Id) : BlockNode;

public record class UnorderedListNode(IReadOnlyList<UnorderedListItemNode> ListItems) : BlockNode;

public record class UnorderedListItemNode(IReadOnlyList<InlineNode> Runs) : BlockNode;

public record class OrderedListNode(IReadOnlyList<OrderedListItemNode> ListItems) : BlockNode;

public record class OrderedListItemNode(IReadOnlyList<InlineNode> Runs) : BlockNode;

public record class TableBlockNode(TableRowNode? Header, IReadOnlyList<TableRowNode> Rows, TableRowNode? Footer, IReadOnlyList<TableCellAlignment>? ColumnAlignments) : BlockNode;

public record class TableRowNode(IReadOnlyList<TableCellNode> Cells) : BlockNode;

public enum TableCellAlignment
{
	Left,
	Center,
	Right
}


public record class HorizontalRuleNode() : BlockNode
{
	public static readonly HorizontalRuleNode Instance = new();
}

public record class EmptyLineNode() : BlockNode
{
	public static readonly EmptyLineNode Instance = new();
}


public record class TextNode(string Text) : InlineNode;

public record class InlineSequenceNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class ItalicNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class BoldNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class UnderlineNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class StrikethroughNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class EmailAddressNode(string EmailAddress) : InlineNode;

public record class PhoneNumberNode(string PhoneNumber) : InlineNode;

public record class UrlNode(string Url) : InlineNode;

public record class LinkNode(IReadOnlyList<InlineNode> Runs, string Url) : InlineNode;

public record class EmojiAliasNode(string Alias) : InlineNode;

public record class EmojiSmileyNode(string Emoji) : InlineNode;

public record class MentionNode(string UserName) : InlineNode;

public record class HashtagNode(string Hashtag) : InlineNode;

public record class InlineCodeNode(string Code) : InlineNode;

public record class InlineMathNode(string Expression) : InlineNode;

public record class TableCellNode(IReadOnlyList<InlineNode> Runs) : InlineNode;

public record class CheckboxNode(bool IsChecked) : InlineNode;

public record class HighlightedNode(string Text) : InlineNode;
