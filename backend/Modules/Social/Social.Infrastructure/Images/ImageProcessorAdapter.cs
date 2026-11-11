using Social.Application.Images;
using SharedImages = Shared.Infrastructure.Images;

namespace Social.Infrastructure.Images;

/// <summary>
/// Implements Social's processor port with the shared image processor. Social passes no
/// purpose limits, so it gets the platform defaults (P23).
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
