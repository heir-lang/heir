namespace Heir.Syntax;

public sealed class Span(Location start, Location? end)
{
    public Location Start { get; } = start;
    public Location End { get; } = end ?? start;

    public override string ToString() =>
        !Start.Equals(End) ? $"{Start} - {End}" : Start.ToString();
}