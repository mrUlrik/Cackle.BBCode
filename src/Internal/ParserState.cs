namespace Cackle.BBCode.Internal;

/// <summary>
///     Identifies the state of the <see cref="BBCodeParser" />.
/// </summary>
internal enum ParserState
{
    /// <summary>
    ///     Parsing a span of text.
    /// </summary>
    Text,

    /// <summary>
    ///     Parsing an opening BBCode tag.
    /// </summary>
    OpeningTag,

    /// <summary>
    ///     Parsing a closing BBCode tag.
    /// </summary>
    ClosingTag
}