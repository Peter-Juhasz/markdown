using PeterJuhasz.Text.Markdown.Model;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

[TestClass]
public class ListTests : TranslationTestBase
{
	[TestMethod]

	// nested into an unordered list
	[TranslationDataRow("- a\n  - b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("- a\n  - b\n  - c", "<ul><li>a<ul><li>b</li><li>c</li></ul></li></ul>")]
	[TranslationDataRow("- a\n  - b\n- c", "<ul><li>a<ul><li>b</li></ul></li><li>c</li></ul>")]
	[TranslationDataRow("* a\n  * b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]

	// a nested list is marked on its own, and a marker of another kind starts a list of its own, just like it does outside
	[TranslationDataRow("- a\n  * b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("- a\n  * b\n  - c", "<ul><li>a<ul><li>b</li></ul><ul><li>c</li></ul></li></ul>")]

	// nested into an ordered list, whose numbering goes on past the items nested into it
	[TranslationDataRow("1. a\n   1. b\n   2. c\n2. d", "<ol><li>a<ol><li>b</li><li>c</li></ol></li><li>d</li></ol>")]
	[TranslationDataRow("1. a\n  1. b\n2. c", "<ol><li>a<ol><li>b</li></ol></li><li>c</li></ol>")]
	[TranslationDataRow("1. a\n   1. b\n2. c\n   1. d\n3. e", "<ol><li>a<ol><li>b</li></ol></li><li>c<ol><li>d</li></ol></li><li>e</li></ol>")]

	// broken numbering only ends the nested list
	[TranslationDataRow("1. a\n   1. b\n   3. c\n2. d", "<ol><li>a<ol><li>b</li></ol><p>3. c</p></li><li>d</li></ol>")]

	// lists of different kinds nested into each other
	[TranslationDataRow("- a\n  1. b\n  2. c", "<ul><li>a<ol><li>b</li><li>c</li></ol></li></ul>")]
	[TranslationDataRow("1. a\n   - b\n2. c", "<ol><li>a<ul><li>b</li></ul></li><li>c</li></ol>")]

	// nested deeply, and back out to any level
	[TranslationDataRow("- a\n  - b\n    - c\n  - d\n- e", "<ul><li>a<ul><li>b<ul><li>c</li></ul></li><li>d</li></ul></li><li>e</li></ul>")]
	[TranslationDataRow("- a\n  - b\n    - c\n- d", "<ul><li>a<ul><li>b<ul><li>c</li></ul></li></ul></li><li>d</li></ul>")]

	// any indentation deeper than the marker nests
	[TranslationDataRow("- a\n - b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("- a\n\t- b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("- a\n        - b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]

	// items of a nested list need not be indented evenly
	[TranslationDataRow("- a\n    - b\n  - c", "<ul><li>a<ul><li>b</li><li>c</li></ul></li></ul>")]

	// indentation is measured from the marker of the item above, not from the beginning of the line
	[TranslationDataRow("  - a\n  - b", "<ul><li>a</li><li>b</li></ul>")]
	[TranslationDataRow("  - a\n    - b", "<ul><li>a<ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("  - a\n- b", "<ul><li>a</li><li>b</li></ul>")]

	// blocks other than lists nested into an item
	[TranslationDataRow("- a\n  text", "<ul><li>a<p>text</p></li></ul>")]
	[TranslationDataRow("- a\n  text\n  - b", "<ul><li>a<p>text</p><ul><li>b</li></ul></li></ul>")]
	[TranslationDataRow("1. a\n   text\n2. b", "<ol><li>a<p>text</p></li><li>b</li></ol>")]
	[TranslationDataRow("- a\n  # heading", "<ul><li>a<h1>heading</h1></li></ul>")]
	[TranslationDataRow("- a\n  > quote", "<ul><li>a<blockquote><p>quote</p></blockquote></li></ul>")]

	// nested items carry inline content, and checkboxes
	[TranslationDataRow("- a\n  - **b** *c*", "<ul><li>a<ul><li><b>b</b> <i>c</i></li></ul></li></ul>")]
	[TranslationDataRow("- [x] a\n  - [ ] b", """<ul><li><input type="checkbox" disabled checked /> a<ul><li><input type="checkbox" disabled /> b</li></ul></li></ul>""")]

	// an empty line ends the list, however the line after it is indented
	[TranslationDataRow("- a\n\n  - b", "<ul><li>a</li></ul><ul><li>b</li></ul>")]

	// a line which is not indented ends the list
	[TranslationDataRow("text\n- a\n  - b\ntext", "<p>text</p><ul><li>a<ul><li>b</li></ul></li></ul><p>text</p>")]

	// line endings
	[TranslationDataRow("- a\r\n  - b\r\n- c", "<ul><li>a<ul><li>b</li></ul></li><li>c</li></ul>")]

	// inside a collapsible block, whose content is measured by the lines it is written on too
	[TranslationDataRow("<details>\n<summary>title</summary>\n\n- a\n  - b\n\n</details>", "<details><summary>title</summary><ul><li>a<ul><li>b</li></ul></li></ul></details>")]
	[TranslationDataRow("  <details>\n  - a\n  - b\n  </details>", "<details><ul><li>a</li><li>b</li></ul></details>")]
	[TranslationDataRow("  <details>\n  - a\n    - b\n  </details>", "<details><ul><li>a<ul><li>b</li></ul></li></ul></details>")]

	public void Translate(string markdown, string expectedHtml) => AssertTranslation(markdown, expectedHtml);


	[TestMethod]
	public void ParseNestedUnorderedList()
	{
		var document = Parser.Parse("- a\n  - b\n- c");

		var list = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, list.ListItems);

		var first = list.ListItems[0];
		Assert.AreEqual("a", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(first.Runs)).Text);
		var nested = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(first.Blocks));
		var nestedItem = Assert.ContainsSingle(nested.ListItems);
		Assert.AreEqual("b", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(nestedItem.Runs)).Text);
		Assert.IsEmpty(nestedItem.Blocks);

		var second = list.ListItems[1];
		Assert.AreEqual("c", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(second.Runs)).Text);
		Assert.IsEmpty(second.Blocks);
	}


	[TestMethod]
	public void ParseNestedOrderedList()
	{
		var document = Parser.Parse("1. a\n   1. b\n   2. c\n2. d");

		var list = Assert.IsInstanceOfType<OrderedListNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, list.ListItems);

		var nested = Assert.IsInstanceOfType<OrderedListNode>(Assert.ContainsSingle(list.ListItems[0].Blocks));
		Assert.HasCount(2, nested.ListItems);
		Assert.AreEqual("b", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(nested.ListItems[0].Runs)).Text);
		Assert.AreEqual("c", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(nested.ListItems[1].Runs)).Text);

		Assert.IsEmpty(list.ListItems[1].Blocks);
	}


	[TestMethod]
	public void ParseListsOfDifferentKindsNestedIntoEachOther()
	{
		var document = Parser.Parse("- a\n  1. b\n     - c");

		var list = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(document.Blocks));
		var ordered = Assert.IsInstanceOfType<OrderedListNode>(Assert.ContainsSingle(Assert.ContainsSingle(list.ListItems).Blocks));
		var unordered = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(Assert.ContainsSingle(ordered.ListItems).Blocks));
		Assert.AreEqual("c", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(Assert.ContainsSingle(unordered.ListItems).Runs)).Text);
	}


	[TestMethod]
	public void ParseParagraphNestedIntoItem()
	{
		var document = Parser.Parse("- a\n  text");

		var item = Assert.ContainsSingle(Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(document.Blocks)).ListItems);
		Assert.AreEqual("a", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(item.Runs)).Text);
		var paragraph = Assert.IsInstanceOfType<ParagraphNode>(Assert.ContainsSingle(item.Blocks));
		Assert.AreEqual("text", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(paragraph.Runs)).Text);
	}


	[TestMethod]
	public void ParseFlatListHasNoNestedBlocks()
	{
		var document = Parser.Parse("- a\n- b");

		var list = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(document.Blocks));
		Assert.HasCount(2, list.ListItems);
		Assert.IsTrue(list.ListItems.All(i => i.Blocks.IsEmpty));
	}


	/// <summary>
	/// Lists are only nested up to a limit, so that a document can make the parser neither recurse
	/// nor read the same lines again without bounds, and the lines nested any deeper are kept as paragraphs.
	/// </summary>
	[TestMethod]
	public void ParseNestedDeeperThanLimit()
	{
		const int depth = 100;

		// every item is nested into the one above it
		var markdown = String.Join('\n', Enumerable.Range(0, depth).Select(i => $"{new string(' ', i)}- a{i}z"));
		var document = Parser.Parse(markdown);

		var list = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(document.Blocks));
		for (var level = 1; level < InplaceMarkdownVisitor.MaxListNestingDepth; level++)
		{
			list = Assert.IsInstanceOfType<UnorderedListNode>(Assert.ContainsSingle(Assert.ContainsSingle(list.ListItems).Blocks));
		}

		var deepest = Assert.ContainsSingle(list.ListItems);
		Assert.AreEqual($"a{InplaceMarkdownVisitor.MaxListNestingDepth - 1}z", Assert.IsInstanceOfType<TextNode>(Assert.ContainsSingle(deepest.Runs)).Text);
		Assert.HasCount(depth - InplaceMarkdownVisitor.MaxListNestingDepth, deepest.Blocks);
		Assert.IsTrue(deepest.Blocks.All(b => b is ParagraphNode));

		// nothing which was written is lost
		var visitor = new TextCollectingVisitor();
		visitor.VisitDocument(document);
		var texts = String.Concat(visitor.Texts);
		for (var i = 0; i < depth; i++)
		{
			Assert.IsTrue(texts.Contains($"a{i}z", StringComparison.Ordinal), $"The text of item {i} is lost.");
		}
	}


	[TestMethod]
	public void VisitNestedItemsInOrder()
	{
		var document = Parser.Parse("- a\n  - b\n    - c\n  - d\n- e");

		var visitor = new TextCollectingVisitor();
		visitor.VisitDocument(document);

		// the blocks nested into an item are visited after its own content, just like they are written
		Assert.AreEqual("a|b|c|d|e", String.Join('|', visitor.Texts));
	}

	private sealed class TextCollectingVisitor : DocumentObjectModelVisitor
	{
		public List<string> Texts { get; } = [];

		protected override void Visit(TextNode node) => Texts.Add(node.Text);
	}
}
