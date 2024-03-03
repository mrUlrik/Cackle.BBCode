# Cackle.BBCode
A simple BBCode parser written in C#. This library was written with a particular platform in mind that supports BBCodes that have only one potential variable.

Currently the library parses to a object called `BBCodeNode`. In the future exports to specific file types may be introduced.

## Example Usage
```cs
var allowedCodes = new List<string> { "b", "i", "u" };
var parser = new BBCodeParser(allowedCodes);
foreach (var node in parser.Parse("Hello [b]World[i]![/i][/b]"))
{
    Console.WriteLine(node.Content);
    foreach (var code in node.Codes)
    {
        Console.WriteLine($"  * {code.Name}");
        Console.WriteLine($"    {code.AttributeValue}");
    }
}
```
