using Games.Application.Images;
using Shared.Domain.Exceptions;

namespace Games.Application.ChallengeImages;

public sealed class ChallengeImagesQueries
{
    private readonly IImageReadRepository _imageReadRepository;
    private readonly IImageStorage _imageStorage;

    public ChallengeImagesQueries(IImageReadRepository imageReadRepository, IImageStorage imageStorage)
    {
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
    }

    public async Task<ImageContentDto> GetImageAsync(Guid imageId)
    {
        var file = await _imageReadRepository.GetByIdAsync(imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        var content = await _imageStorage.ReadAsync(file.StoredFileName);
        if (content is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return content;
    }
}
