namespace Shared.Infrastructure.Images;

/// <summary>The re-encoded image to store, with the dimensions of the stored image.</summary>
public sealed record ProcessedImage(byte[] Content, int Width, int Height);
