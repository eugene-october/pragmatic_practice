using System.Runtime.CompilerServices;

namespace Fsm.Kernel.Internal;

/// <summary>
/// Turns an enum into a dense array index.
/// </summary>
/// <remarks>
/// The engine indexes its transition table directly with enum values, which is only sound when the
/// enum is backed by <see cref="int"/> and declares exactly the values <c>0..N-1</c>. Rather than
/// failing obscurely at run time, the violation is captured in <see cref="Problem"/> and reported
/// by the builder as an ordinary definition diagnostic.
/// </remarks>
internal static class EnumIndex<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>All declared values, ascending. Meaningful only when <see cref="Problem"/> is null.</summary>
    internal static readonly TEnum[] Values;

    internal static readonly int Count;

    /// <summary>Human readable reason this enum cannot index a dense table, or null when it can.</summary>
    internal static readonly string? Problem;

    static EnumIndex()
    {
        Values = Enum.GetValues<TEnum>();
        Count = Values.Length;

        if (Enum.GetUnderlyingType(typeof(TEnum)) != typeof(int))
        {
            Problem = $"'{typeof(TEnum).Name}' must be backed by 'int' to index a dense transition table, "
                    + $"but is backed by '{Enum.GetUnderlyingType(typeof(TEnum)).Name}'.";
            return;
        }

        if (Count == 0)
        {
            Problem = $"'{typeof(TEnum).Name}' declares no members.";
            return;
        }

        var seen = new bool[Count];
        foreach (var value in Values)
        {
            var index = ToIndex(value);
            if (index < 0 || index >= Count || seen[index])
            {
                Problem = $"'{typeof(TEnum).Name}' must declare {Count} distinct members numbered 0..{Count - 1} "
                        + $"to index a dense transition table, but '{value}' is {index}.";
                return;
            }

            seen[index] = true;
        }
    }

    /// <summary>Reinterprets the enum as its underlying <see cref="int"/> without boxing.</summary>
    internal static int ToIndex(TEnum value) => Unsafe.As<TEnum, int>(ref value);

    /// <summary>Inverse of <see cref="ToIndex"/>. Valid only when <see cref="Problem"/> is null.</summary>
    internal static TEnum FromIndex(int index) => Values[index];
}
