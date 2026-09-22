using Microsoft.Extensions.Primitives;
using PeterJuhasz.Text.Markdown.Parsing;

namespace PeterJuhasz.Text.Markdown.Tests;

/// <summary>
/// Every parser loop must consume at least one character per iteration, otherwise it never terminates.
/// These cases cover the inputs where a block or an inline node could be recognized without consuming anything.
/// </summary>
[TestClass]
public class ParserTerminationTests
{
	private const int TimeoutMilliseconds = 10_000;

	[TestMethod]

	// empty and whitespace only
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("\n")]
	[DataRow("\n\n\n")]
	[DataRow("\r\n\r\n")]
	[DataRow("\t")]
	[DataRow("   \n \t \n   ")]

	// unordered list: the delimiter alone, without an item
	[DataRow("-")]
	[DataRow("- ")]
	[DataRow("-\n-")]
	[DataRow("*")]
	[DataRow("* ")]
	[DataRow("- a\n-")]
	[DataRow("- a\n- ")]
	[DataRow("- a\n  - b\n-")]
	[DataRow("  - a")]
	[DataRow("- a\n* b")]

	// ordered list: the number alone, without an item, and broken numbering
	[DataRow("1.")]
	[DataRow("1. ")]
	[DataRow("1. a\n1. b")]
	[DataRow("1. a\n3. b")]
	[DataRow("1. a\n2.")]
	[DataRow("1. a\n10. b")]
	[DataRow("1. a\nb")]

	// block quote: the delimiter alone, without content
	[DataRow(">")]
	[DataRow("> ")]
	[DataRow(">\n>")]
	[DataRow("> a\n>")]
	[DataRow("> a\nb")]

	// spoiler: the delimiter alone, without content
	[DataRow(">!")]
	[DataRow(">! ")]
	[DataRow(">! a\n>!")]
	[DataRow(">! a\n> b")]

	// table: the delimiter alone, without cells
	[DataRow("|")]
	[DataRow("| ")]
	[DataRow("||")]
	[DataRow("|\n|")]
	[DataRow("|a")]
	[DataRow("|a|\n|-|\n|b|")]
	[DataRow("|a|\nb")]
	[DataRow("| | |")]

	// code block: unterminated, and more fence characters than supported
	[DataRow("``")]
	[DataRow("```")]
	[DataRow("```a")]
	[DataRow("```\na")]
	[DataRow("```\na\n```")]
	[DataRow("```\n```")]
	[DataRow("``````\na\n``````")]
	[DataRow("```````````\na\n```````````")]

	// math block: unterminated, and empty
	[DataRow("$$")]
	[DataRow("$$\n")]
	[DataRow("$$\na")]
	[DataRow("$$\na\n$$")]
	[DataRow("$$\n$$")]
	[DataRow("$$a$$")]
	[DataRow("$$$$")]

	// front matter: unterminated, and empty
	[DataRow("---\n")]
	[DataRow("---\na")]
	[DataRow("---\na\n---")]
	[DataRow("---\n---")]
	[DataRow("---\n\n---")]
	[DataRow("---\n---\n---")]

	// collapsible block: unterminated, empty, and nested into itself
	[DataRow("<details>")]
	[DataRow("<details>\n")]
	[DataRow("<details>a")]
	[DataRow("<details></details>")]
	[DataRow("<details>\n</details>")]
	[DataRow("</details>")]
	[DataRow("<details><summary></summary></details>")]
	[DataRow("<details><summary></details>")]
	[DataRow("<details><details></details></details>")]
	[DataRow("<details></details><details></details>")]

	// figure: unterminated, empty, longer fences, and nested into itself
	[DataRow("^^^")]
	[DataRow("^^^\n")]
	[DataRow("^^^\na")]
	[DataRow("^^^\n^^^")]
	[DataRow("^^^\n^^^ ")]
	[DataRow("^^^\n^^^^")]
	[DataRow("^^^\n^^^^\n^^^")]
	[DataRow("^^^^\n^^^^")]
	[DataRow("^^^\n^^^\n^^^\n^^^")]
	[DataRow("^^^\n^^^ a\n^^^")]

	// heading, horizontal rule and embed
	[DataRow("#")]
	[DataRow("# ")]
	[DataRow("######")]
	[DataRow("####### a")]
	[DataRow("---")]
	[DataRow("!")]
	[DataRow("!()")]
	[DataRow("!(a://b)")]
	[DataRow("!(")]

	// unmatched inline delimiters
	[DataRow("**")]
	[DataRow("***")]
	[DataRow("****")]
	[DataRow("*a")]
	[DataRow("**a")]
	[DataRow("***a**")]
	[DataRow("_")]
	[DataRow("__")]
	[DataRow("~")]
	[DataRow("~~")]
	[DataRow("`")]
	[DataRow("` `")]
	[DataRow("$")]
	[DataRow("$$$")]
	[DataRow("$a$")]
	[DataRow("[")]
	[DataRow("[]")]
	[DataRow("[]()")]
	[DataRow("[a](")]
	[DataRow("[a](b")]
	[DataRow("[a](b)")]
	[DataRow(":")]
	[DataRow("::")]
	[DataRow(":a")]
	[DataRow(":)")]
	[DataRow(";")]
	[DataRow("@")]
	[DataRow("@@")]
	[DataRow("@a")]
	[DataRow("#a")]
	[DataRow("a@")]
	[DataRow("a@b")]
	[DataRow("https:")]
	[DataRow("https://")]

	// escapes
	[DataRow("\\")]
	[DataRow("\\\\")]
	[DataRow("\\*")]
	[DataRow("\\$")]
	[DataRow("a\\")]
	[DataRow("*\\*")]
	[DataRow("\\*\\*a")]

	// checkbox
	[DataRow("- [")]
	[DataRow("- []")]
	[DataRow("- [ ]")]
	[DataRow("- [x] a")]

	public void ParseTerminates(string markdown) => AssertParseTerminates(markdown);

	[TestMethod]
	[DataRow("*")]
	[DataRow("**")]
	[DataRow("_")]
	[DataRow("~")]
	[DataRow("`")]
	[DataRow("$")]
	[DataRow("[")]
	[DataRow("[a](")]
	[DataRow(":")]
	[DataRow("@")]
	[DataRow("\\")]
	[DataRow("-")]
	[DataRow("- ")]
	[DataRow("1. ")]
	[DataRow(">")]
	[DataRow("> ")]
	[DataRow(">!")]
	[DataRow("|")]
	[DataRow("```")]
	[DataRow("$$")]
	[DataRow("<details>")]
	[DataRow("<details></details>")]
	[DataRow("^^^\n")]
	[DataRow("^^^\n^^^\n")]
	[DataRow("\n*")]
	[DataRow("\n- ")]
	[DataRow("\n|")]
	[DataRow("\n> ")]
	public void ParseLongRepetitionTerminates(string unit) => AssertParseTerminates(String.Concat(Enumerable.Repeat(unit, 10_000)));

	/// <summary>
	/// Blocks are located relative to the document, which is not necessarily the beginning of the underlying string.
	/// </summary>
	[TestMethod]
	[DataRow("")]
	[DataRow("prefix")]
	public void ParseBlocksOfSegmentWhichDoesNotStartAtTheBeginningOfTheString(string prefix)
	{
		const string document = "- a\n- b\ntail";

		var blocks = new List<string>();
		foreach (var block in new Parser.BlockParser(new StringSegment(prefix + document).Subsegment(prefix.Length)))
		{
			blocks.Add($"{block.Type}:{block.FullSegment}");
		}

		Assert.AreEqual("UnorderedList:- a\n- b\n|Paragraph:tail", String.Join('|', blocks));
	}

	/// <summary>
	/// Parses on another thread, so that a parser which never terminates fails the test instead of hanging the test run.
	/// </summary>
	private static void AssertParseTerminates(string markdown)
	{
		var parse = Task.Run(() => Parser.Parse(markdown));

		Assert.IsTrue(parse.Wait(TimeoutMilliseconds), "Parsing did not terminate.");
		Assert.IsNotNull(parse.Result);
	}
}
