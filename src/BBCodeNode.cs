namespace Cackle.BBCode;

/// <summary>
///     Representation of BBCodes attributed to text.
/// </summary>
/// <param name="startIndex">The first position of content from the source.</param>
/// <param name="length">The length of the content, if already known.</param>
public class BBCodeNode(int startIndex, int length = 0)
{
    /// <summary>
    ///     The content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    ///     Collection of codes found in the source.
    /// </summary>
    public HashSet<BBCode> Codes { get; set; } = [];

    /// <summary>
    ///     The first position of content from source.
    /// </summary>
    public int StartIndex { get; set; } = startIndex;

    /// <summary>
    ///     The length of the content.
    /// </summary>
    public int Length { get; set; } = length;
}