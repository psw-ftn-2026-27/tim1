using Shared.Domain.Exceptions;

namespace Social.Domain.Images;

public sealed record ImageDimensions
{
    public const int MinSide = 100;
    public const int MaxSide = 8000;
    public const long MaxPixels = 40_000_000;

    public int Width { get; private set; }
    public int Height { get; private set; }

    private ImageDimensions() { }

    public ImageDimensions(int width, int height)
    {
        if (width < MinSide || height < MinSide)
        {
            throw new DomainException($"An image must be at least {MinSide}x{MinSide} pixels.");
        }
        if (width > MaxSide || height > MaxSide || (long)width * height > MaxPixels)
        {
            throw new DomainException($"An image must be at most {MaxSide}x{MaxSide} pixels and {MaxPixels / 1_000_000} million pixels in total.");
        }

        Width = width;
        Height = height;
    }
}
