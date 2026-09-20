# Markdown

High performance Markdown parser written in .NET C#.

Supported features:
- Paragraphs, headings
- Bold, italic, strikethrough
- Links
- E-mail addresses, phone numbers, autolinks
- Images, miscellaneous embedded content
- Code, math
- Ordered and unordered lists
- Quotes, spoilers
- Pipe tables
- Horizontal rules, checkboxes,
- Emojis, emoji aliases
- Mention, hashtags

## Install

```sh
dotnet add package PeterJuhasz.Text.Markdown
```

## High-level API

To parse a markdown string to a Document Object Model:

```cs
var document = Parser.Parse(markdown);
```

Then you can navigate the document tree:

```cs
document.Blocks[0].Inlines[0];
```

Or create a visitor based on the `DocumentObjectModelVisitor` to fully traverse the document:

```cs
class HtmlWriterVisitor(HtmlWriter writer) : DocumentObjectModelVisitor
{
	public override void Visit(LinkNode link)
	{
		writer.OpenElement("a");
		writer.WriteAttribute("href", link.Url);
		VisitInner(link);
		writer.CloseElement();
	}

	public override void Visit(TextNode text)
	{
		writer.Write(text.Text);
	}

	// ...
}
```

## Low-level API

To run the parser in streaming and lazy mode, for example to translate a document to HTML without buffering to a full Document Object Model, create visitor based on the `InplaceMarkdownVisitor`:

```cs
class HtmlWriterVisitor(HtmlWriter writer) : InplaceMarkdownVisitor
{
	protected override void VisitLink(Node node, StringSegment url)
	{
		writer.OpenElement("a");
		writer.WriteAttribute("href", url);
		VisitInner(node);
		writer.CloseElement();
	}

	public override void VisitText(Node node, StringSegment text)
	{
		writer.WriteText(text);
	}

	// ...
}
```