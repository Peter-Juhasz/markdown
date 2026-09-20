namespace System.Text.Markdown.Model;

public abstract class DocumentObjectModelVisitor
{
	public void VisitDocument(DocumentNode document) => Visit(document);

	protected virtual void Visit(DocumentNode node) => VisitInner(node);
	protected virtual void Visit(HeadingNode node) => VisitInner(node);
	protected virtual void Visit(ParagraphNode node) => VisitInner(node);
	protected virtual void Visit(BlockQuoteNode node) => VisitInner(node);
	protected virtual void Visit(SpoilerNode node) => VisitInner(node);
	protected virtual void Visit(CodeBlockNode node) { }
	protected virtual void Visit(InlineCodeNode node) { }
	protected virtual void Visit(MathBlockNode node) { }
	protected virtual void Visit(InlineMathNode node) { }
	protected virtual void Visit(EmbedNode node) { }
	protected virtual void Visit(UnorderedListNode node) => VisitInner(node);
	protected virtual void Visit(UnorderedListItemNode node) => VisitInner(node);
	protected virtual void Visit(OrderedListNode node) => VisitInner(node);
	protected virtual void Visit(OrderedListItemNode node) => VisitInner(node);
	protected virtual void Visit(HorizontalRuleNode node) { }
	protected virtual void Visit(EmptyLineNode node) { }
	protected virtual void Visit(TextNode node) { }
	protected virtual void Visit(ItalicNode node) => VisitInner(node);
	protected virtual void Visit(BoldNode node) => VisitInner(node);
	protected virtual void Visit(UnderlineNode node) => VisitInner(node);
	protected virtual void Visit(StrikethroughNode node) => VisitInner(node);
	protected virtual void Visit(EmailAddressNode node) { }
	protected virtual void Visit(PhoneNumberNode node) { }
	protected virtual void Visit(UrlNode node) { }
	protected virtual void Visit(LinkNode node) => VisitInner(node);
	protected virtual void Visit(EmojiAliasNode node) { }
	protected virtual void Visit(EmojiSmileyNode node) { }
	protected virtual void Visit(MentionNode node) { }
	protected virtual void Visit(HashtagNode node) { }
	protected virtual void Visit(CheckboxNode node) { }
	protected virtual void Visit(TableBlockNode node) => VisitInner(node);
	protected virtual void Visit(TableRowNode node) => VisitInner(node);
	protected virtual void Visit(TableCellNode node) => VisitInner(node);


	protected virtual void Visit(Node node)
	{
		switch (node)
		{
			case DocumentNode n:
				Visit(n);
				break;

			case HeadingNode n:
				Visit(n);
				break;

			case ParagraphNode n:
				Visit(n);
				break;

			case BlockQuoteNode n:
				Visit(n);
				break;

			case CodeBlockNode n:
				Visit(n);
				break;

			case InlineCodeNode n:
				Visit(n);
				break;

			case MathBlockNode n:
				Visit(n);
				break;

			case InlineMathNode n:
				Visit(n);
				break;

			case EmbedNode n:
				Visit(n);
				break;

			case LinkNode n:
				Visit(n);
				break;

			case HorizontalRuleNode n:
				Visit(n);
				break;

			case EmptyLineNode n:
				Visit(n);
				break;

			case TextNode n:
				Visit(n);
				break;

			case ItalicNode n:
				Visit(n);
				break;

			case BoldNode n:
				Visit(n);
				break;

			case UnderlineNode n:
				Visit(n);
				break;

			case StrikethroughNode n:
				Visit(n);
				break;

			case EmailAddressNode n:
				Visit(n);
				break;

			case EmojiSmileyNode n:
				Visit(n);
				break;

			case EmojiAliasNode n:
				Visit(n);
				break;

			case PhoneNumberNode n:
				Visit(n);
				break;

			case UrlNode n:
				Visit(n);
				break;

			case UnorderedListNode n:
				Visit(n);
				break;

			case UnorderedListItemNode n:
				Visit(n);
				break;

			case OrderedListNode n:
				Visit(n);
				break;

			case OrderedListItemNode n:
				Visit(n);
				break;

			case MentionNode n:
				Visit(n);
				break;

			case HashtagNode n:
				Visit(n);
				break;

			case CheckboxNode n:
				Visit(n);
				break;

			case SpoilerNode n:
				Visit(n);
				break;

			case TableBlockNode n:
				Visit(n);
				break;

			case TableRowNode n:
				Visit(n);
				break;

			case TableCellNode n:
				Visit(n);
				break;

			default:
				throw new NotSupportedException();
		}
	}

	protected void VisitInner(Node node)
	{
		switch (node)
		{
			case DocumentNode n:
				{
					foreach (var run in n.Blocks)
					{
						Visit(run);
					}
				}
				break;

			case HeadingNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case ParagraphNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case BlockQuoteNode n:
				{
					foreach (var run in n.Blocks)
					{
						Visit(run);
					}
				}
				break;

			case SpoilerNode n:
				{
					foreach (var run in n.Blocks)
					{
						Visit(run);
					}
				}
				break;

			case LinkNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case ItalicNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case BoldNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case UnderlineNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case StrikethroughNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case UnorderedListNode n:
				{
					foreach (var run in n.ListItems)
					{
						Visit(run);
					}
				}
				break;

			case UnorderedListItemNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case OrderedListNode n:
				{
					foreach (var run in n.ListItems)
					{
						Visit(run);
					}
				}
				break;

			case OrderedListItemNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			case TableBlockNode n:
				{
					if (n.Header is not null)
					{
						Visit(n.Header);
					}

					foreach (var row in n.Rows)
					{
						Visit(row);
					}

					if (n.Footer is not null)
					{
						Visit(n.Footer);
					}
				}
				break;

			case TableRowNode n:
				{
					foreach (var cell in n.Cells)
					{
						Visit(cell);
					}
				}
				break;

			case TableCellNode n:
				{
					foreach (var run in n.Runs)
					{
						Visit(run);
					}
				}
				break;

			default:
				throw new NotSupportedException();
		}
	}
}
