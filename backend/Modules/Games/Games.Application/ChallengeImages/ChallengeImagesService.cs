using Games.Application.Images;
using Games.Domain.Images;

namespace Games.Application.ChallengeImages;

public sealed class ChallengeImagesService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    public ChallengeImagesService(
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
    // without a file, but never a file without a record.
    public async Task<Guid> UploadImageAsync(Stream content)
    {
        var processed = _imageProcessor.Process(content);
        var image = new Image(processed.Width, processed.Height, processed.Content.Length);

        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.SaveAsync(image.StoredFileName, processed.Content);

        return image.Id;
    }
}
