using Microsoft.Extensions.Primitives;
using System.Text.Markdown.Model;
using System.Text.Markdown.Parsing;

namespace System.Text.Markdown.Benchmarks.Visitors;

using Segment = StringSegment;

// the low level API has a node type of its own, which is not the Document Object Model one
using Node = Parsing.Node;

/// <summary>
/// Traverses a markdown document in streaming mode without doing any work on the nodes, to measure
/// the cost of the lazy parse and the traversal, without ever materializing a Document Object Model.
/// </summary>
/// <remarks>
/// Every node is counted, so that the traversal can neither be optimized away nor silently visit
/// nothing.
/// </remarks>
internal sealed class NoopInplaceMarkdownVisitor : InplaceMarkdownVisitor
{
	public int Count { get; private set; }

	protected override void VisitDocument(Node node)
	{
		Count = 0;
		Count++;
		base.VisitDocument(node);
	}

	// blocks
	protected override void VisitHeading(Node node, int level) { Count++; VisitInner(node); }

	protected override void VisitParagraph(Node node) { Count++; VisitInner(node); }

	protected override void VisitBlockQuote(Node node) { Count++; VisitInner(node); }

	protected override void VisitSpoiler(Node node) { Count++; VisitInner(node); }

	protected override void VisitDetails(Node node) { Count++; VisitInner(node); }

	protected override void VisitDetailsSummary(Node node) { Count++; VisitInner(node); }

	protected override void VisitFigure(Node node) { Count++; VisitInner(node); }

	protected override void VisitFigureCaption(Node node) { Count++; VisitInner(node); }

	protected override void VisitCodeBlock(Node node, Segment code, Segment language) => Count++;

	protected override void VisitMathBlock(Node node, Segment math) => Count++;

	protected override void VisitFrontMatter(Node node, Segment frontMatter) => Count++;

	protected override void VisitFootnoteContent(Node node, int number) { Count++; VisitInner(node); }

	protected override void VisitHorizontalRule(Node node) => Count++;

	protected override void VisitEmptyLine(Node node) => Count++;

	// lists
	protected override void VisitUnorderedList(Node node) { Count++; VisitInner(node); }

	protected override void VisitUnorderedListItem(Node node) { Count++; VisitInner(node); }

	protected override void VisitOrderedList(Node node) { Count++; VisitInner(node); }

	protected override void VisitOrderedListItem(Node node) { Count++; VisitInner(node); }

	protected override void VisitCheckbox(Node node, bool isChecked) => Count++;

	// tables
	protected override void VisitTable(Node node) { Count++; VisitInner(node); }

	protected override void VisitTableRow(Node node) { Count++; VisitInner(node); }

	protected override void VisitTableCell(Node node, TableCellAlignment? alignment) { Count++; VisitInner(node); }

	// inline formatting
	protected override void VisitBold(Node node) { Count++; VisitInner(node); }

	protected override void VisitItalic(Node node) { Count++; VisitInner(node); }

	protected override void VisitUnderline(Node node) { Count++; VisitInner(node); }

	protected override void VisitStrikethrough(Node node) { Count++; VisitInner(node); }

	protected override void VisitLink(Node node, Segment url, Segment title) { Count++; VisitInner(node); }

	// inline leaves
	protected override void VisitText(Node node, Segment text) => Count++;

	protected override void VisitInlineCode(Node node, Segment code) => Count++;

	protected override void VisitInlineMath(Node node, Segment math) => Count++;

	protected override void VisitUrl(Node node, Segment url) => Count++;

	protected override void VisitEmailAddress(Node node, Segment emailAddress) => Count++;

	protected override void VisitPhoneNumber(Node node, Segment phoneNumber) => Count++;

	protected override void VisitEmbed(Node node, Segment scheme, Segment id) => Count++;

	protected override void VisitEmojiAlias(Node node, Segment alias) => Count++;

	protected override void VisitEmojiSmiley(Node node, Segment smiley) => Count++;

	protected override void VisitMention(Node node, Segment userName) => Count++;

	protected override void VisitHashtag(Node node, Segment tag) => Count++;

	protected override void VisitFootnoteReference(Node node, int number) => Count++;
}
