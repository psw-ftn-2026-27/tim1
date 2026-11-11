namespace Social.Application.Images;

/// <summary>
/// Turns an uploaded file into the image to store, or throws DomainException when the file
/// is not an acceptable image. Implemented in Infrastructure over the shared image processor;
/// Social uses the platform's default purpose limits (P23).
/// </summary>
public interface IImageProcessor
{
    ProcessedImage Process(Stream content);
}
