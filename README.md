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
	protected override void VisitLink(Node node, StringSegment url, StringSegment title)
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

## Benchmarks

### Markdown to HTML

| Method                    | Mean     | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| Markdig (1.3.2)                   | 56.00 us | 0.458 us | 0.429 us |  1.00 | 5.2490 | 1.0376 |  97.37 KB |        1.00 |
| PeterJuhasz.Text.Markdown | 19.56 us | 0.384 us | 0.471 us |  0.35 | 3.2043 | 0.1831 |  59.05 KB |        0.61 |

### Parse

| Method                    | Mean     | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| Markdig (1.3.2)                   | 49.98 us | 0.544 us | 0.509 us |  1.00 | 4.4556 | 0.8545 |  81.95 KB |        1.00 |
| PeterJuhasz.Text.Markdown | 18.82 us | 0.167 us | 0.148 us |  0.38 | 2.4414 | 0.1526 |  44.99 KB |        0.55 |

### Visitor

| Method                                                    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| 'Markdig (walk)' (1.3.2)                                          |  4.598 us | 0.0561 us | 0.0525 us |  1.00 |    0.02 | 0.1373 |    2696 B |       1.000 |
| 'PeterJuhasz.Text.Markdown (high level)'                  |  5.163 us | 0.0231 us | 0.0205 us |  1.12 |    0.01 |      - |      24 B |       0.009 |
| 'PeterJuhasz.Text.Markdown (low level, includes parsing)' | 11.831 us | 0.2287 us | 0.2543 us |  2.57 |    0.06 |      - |     136 B |       0.050 |
