using System;

namespace Prefetch;

/// <summary>
///     Thrown when a prefetch file (or a structure inside one) cannot be parsed.
///     <see cref="Offset" /> and <see cref="Field" /> identify where parsing failed when that is known.
/// </summary>
public class PrefetchParseException : Exception
{
    public PrefetchParseException(string message, int offset = -1, string field = null, Exception inner = null)
        : base(message, inner)
    {
        Offset = offset;
        Field = field;
    }

    /// <summary>
    ///     Byte offset of the failing read relative to the buffer being parsed, or -1 when unknown.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    ///     Name of the field or structure being read when parsing failed, if known.
    /// </summary>
    public string Field { get; }
}
