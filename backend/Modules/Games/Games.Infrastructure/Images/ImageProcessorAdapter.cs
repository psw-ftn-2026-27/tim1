using Games.Application.Images;
using SharedImages = Shared.Infrastructure.Images;

namespace Games.Infrastructure.Images;

/// <summary>
/// Implements Games' processor port with the shared image processor. Games passes no
/// purpose limits, so it gets the platform defaults.
/// </summary>
internal sealed class ImageProcessorAdapter : IImageProcessor
{
    private readonly SharedImages.ImageProcessor _processor = new();

    public ProcessedImage Process(Stream content)
    {
        var processed = _processor.Process(content);
        return new ProcessedImage(processed.Content, processed.Width, processed.Height);
    }
}
