using System.Collections.Immutable;

namespace System.Text.Markdown.Model;


public abstract record class Node();

public abstract record class InlineNode() : Node;

public abstract record class BlockNode() : Node;


public record class DocumentNode(ImmutableArray<BlockNode> Blocks) : BlockNode;

public record class ParagraphNode(ImmutableArray<InlineNode> Runs) : BlockNode;

public record class HeadingNode(int Level, ImmutableArray<InlineNode> Runs) : BlockNode;

public record class BlockQuoteNode(ImmutableArray<BlockNode> Blocks) : BlockNode;

public record class SpoilerNode(ImmutableArray<BlockNode> Blocks) : BlockNode;

public record class AlertNode(string Type, ImmutableArray<BlockNode> Blocks) : BlockNode;

public record class CodeBlockNode(string Code, string Language) : BlockNode;

public record class MathBlockNode(string Expression) : BlockNode;

public record class FrontMatterNode(string Content) : BlockNode;

public record class EmbedNode(string Scheme, string Id) : BlockNode;

public record class UnorderedListNode(ImmutableArray<UnorderedListItemNode> ListItems) : BlockNode;

public record class UnorderedListItemNode(ImmutableArray<InlineNode> Runs) : BlockNode;

public record class OrderedListNode(ImmutableArray<OrderedListItemNode> ListItems) : BlockNode;

public record class OrderedListItemNode(ImmutableArray<InlineNode> Runs) : BlockNode;

public record class TableBlockNode(TableRowNode? Header, ImmutableArray<TableRowNode> Rows, TableRowNode? Footer, ImmutableArray<TableCellAlignment>? ColumnAlignments) : BlockNode;

public record class TableRowNode(ImmutableArray<TableCellNode> Cells) : BlockNode;

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

public record class InlineSequenceNode(ImmutableArray<InlineNode> Runs) : InlineNode;

public record class ItalicNode(ImmutableArray<InlineNode> Runs) : InlineSequenceNode(Runs);

public record class BoldNode(ImmutableArray<InlineNode> Runs) : InlineSequenceNode(Runs);

public record class UnderlineNode(ImmutableArray<InlineNode> Runs) : InlineSequenceNode(Runs);

public record class StrikethroughNode(ImmutableArray<InlineNode> Runs) : InlineSequenceNode(Runs);

public record class EmailAddressNode(string EmailAddress) : InlineNode;

public record class PhoneNumberNode(string PhoneNumber) : InlineNode;

public record class UrlNode(string Url) : InlineNode;

public record class AngleBracketUrlNode(string Url) : UrlNode(Url);

public record class LinkNode(ImmutableArray<InlineNode> Runs, string Url, string? Title = null) : InlineSequenceNode(Runs);

public record class EmojiAliasNode(string Alias) : InlineNode;

public record class EmojiSmileyNode(string Emoji) : InlineNode;

public record class MentionNode(string UserName) : InlineNode;

public record class HashtagNode(string Hashtag) : InlineNode;

public record class InlineCodeNode(string Code) : InlineNode;

public record class InlineMathNode(string Expression) : InlineNode;

public record class TableCellNode(ImmutableArray<InlineNode> Runs) : InlineSequenceNode(Runs);

public record class CheckboxNode(bool IsChecked) : InlineNode;

public record class HighlightedNode(string Text) : InlineNode;
