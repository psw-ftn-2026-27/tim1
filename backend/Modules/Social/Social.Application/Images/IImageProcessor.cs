namespace Social.Application.Images;

/// <summary>
/// Turns an uploaded file into the bytes to store, or throws DomainException when the
/// file is not an acceptable image.
/// </summary>
public interface IImageProcessor
{
    ProcessedImage Process(Stream content);
}
