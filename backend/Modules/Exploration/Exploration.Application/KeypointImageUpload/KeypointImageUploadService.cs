using Exploration.Application.Images;
using Exploration.Application.Tours;
using Exploration.Domain.Images;

namespace Exploration.Application.KeypointImageUpload;

public sealed class KeypointImageUploadService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    public KeypointImageUploadService(
        IImageRepository imageRepository,
        IImageProcessor imageProcessor,
        IImageStorage imageStorage,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _imageProcessor = imageProcessor;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
    }

    // The record is saved before the file is written, so a failure can leave a record
    // without a file, which the cleanup below removes later, but never a file without a
    // record. An expired image loses its file before its record, for the same reason.
    public async Task<Guid> UploadKeypointImageAsync(Guid userId, Stream content)
    {
        var processed = _imageProcessor.Process(content);
        var image = new Image(userId, processed.Width, processed.Height, processed.Content.Length);
        var expiredImages = (await _imageRepository.GetUnattachedAsync())
            .Where(unattached => unattached.IsExpired(DateTime.UtcNow))
            .ToList();

        foreach (var expired in expiredImages)
        {
            _imageStorage.Delete(expired.StoredFileName);
            _imageRepository.Remove(expired);
        }
        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.SaveAsync(image.StoredFileName, processed.Content);

        return image.Id;
    }
}
