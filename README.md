# Markdown

High performance Markdown parser written in .NET C#.

Supported features:
- Paragraphs, headings
- Bold, italic, strikethrough
- Links, urls, angle bracket links
- E-mail addresses, phone numbers
- Images, miscellaneous embedded content
- Code, math
- Ordered and unordered lists
- Quotes, spoilers, alerts
- Collapsible details
- Pipe tables
- Horizontal rules, checkboxes,
- Emojis, emoji aliases
- Mention, hashtags
- YAML front matter
- Comments
- Footnotes

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
| Markdig                   | 56.06 us | 0.428 us | 0.400 us |  1.00 | 5.2490 | 1.0376 |  97.37 KB |        1.00 |
| PeterJuhasz.Text.Markdown | 16.68 us | 0.109 us | 0.102 us |  0.30 | 2.4109 | 0.1221 |  44.63 KB |        0.46 |

### Parse

| Method                    | Mean     | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |---------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
| Markdig                   | 46.95 us | 0.382 us | 0.357 us |  1.00 | 4.4556 | 0.8545 |  81.95 KB |        1.00 |
| PeterJuhasz.Text.Markdown | 16.09 us | 0.052 us | 0.046 us |  0.34 | 1.5259 | 0.0610 |  28.47 KB |        0.35 |

### Visitor

| Method                                                    | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| 'Markdig (walk)'                                          |  4.514 us | 0.0202 us | 0.0179 us |  1.00 | 0.1373 |    2696 B |       1.000 |
| 'PeterJuhasz.Text.Markdown (high level)'                  |  5.379 us | 0.0201 us | 0.0188 us |  1.19 |      - |      24 B |       0.009 |
| 'PeterJuhasz.Text.Markdown (low level, includes parsing)' | 11.196 us | 0.0160 us | 0.0150 us |  2.48 |      - |     136 B |       0.050 |
