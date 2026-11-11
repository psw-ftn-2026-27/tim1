using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record GroupSize
{
    public int Min { get; private init; }
    public int Max { get; private init; }

    private GroupSize() { }

    public GroupSize(int min, int max)
    {
        if (min < 1)
        {
            throw new DomainException("Minimum group size must be at least 1.");
        }
        if (max < min)
        {
            throw new DomainException("Maximum group size must be greater than or equal to minimum.");
        }

        Min = min;
        Max = max;
    }
}
