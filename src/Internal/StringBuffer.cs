namespace Cackle.BBCode.Internal;

/// <summary>
///     A simple buffer for a string to track position.
/// </summary>
/// <param name="text">String to buffer.</param>
internal class StringBuffer(string text)
{
    private readonly ReadOnlyMemory<char> _buffer = text.AsMemory();

    /// <summary>
    ///     Length of the buffer.
    /// </summary>
    public int Length => _buffer.Length;

    /// <summary>
    ///     Current position of the buffer.
    /// </summary>
    public int Index { get; private set; } = -1;

    /// <summary>
    ///     Token on the current position in the buffer.
    /// </summary>
    public char Token => Index < Length ? _buffer.Span[Index] : char.MinValue;

    /// <summary>
    ///     Retrieve the value beginning at <paramref name="startIndex" /> up to the <paramref name="length" />.
    /// </summary>
    public string GetValue(int startIndex, int length)
    {
        return _buffer.Slice(startIndex, length).ToString();
    }

    /// <summary>
    ///     Retrieve the next token without progressing to the next position.
    /// </summary>
    public char Peek()
    {
        var index = Index + 1;
        return index < Length ? _buffer.Span[index] : char.MinValue;
    }

    /// <summary>
    ///     Progress to the next position in the buffer.
    /// </summary>
    /// <returns>Returns false when the end of the buffer has been reached.</returns>
    public bool Read()
    {
        Index++;
        return Index < Length;
    }
}