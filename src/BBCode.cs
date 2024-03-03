using System.Diagnostics;

namespace Cackle.BBCode;

/// <summary>
///     Represents a BBCode and it's optional attribute.
/// </summary>
[DebuggerDisplay("{Name}")]
public class BBCode(string name) : IEquatable<BBCode>
{
    /// <summary>
    ///     The name of the code.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     The value of it's attribute, if any.
    /// </summary>
    public string? AttributeValue { get; set; }

    /// <inheritdoc />
    public bool Equals(BBCode? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BBCode)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}