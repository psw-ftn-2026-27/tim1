using Exploration.Application.Images;
using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeypointAuthoring;

public sealed class KeypointAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly IImageReadRepository _imageReadRepository;
    private readonly IImageStorage _imageStorage;

    public KeypointAuthoringQueries(
        ITourReadRepository tourReadRepository,
        IImageReadRepository imageReadRepository,
        IImageStorage imageStorage)
    {
        _tourReadRepository = tourReadRepository;
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
    }

    public async Task<List<KeypointDto>> GetKeypointsAsync(Guid tourId, Guid authorId)
    {
        var keypoints = await _tourReadRepository.GetKeypointsAsync(tourId, authorId);
        if (keypoints is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return keypoints;
    }

    public async Task<KeypointDto> GetKeypointAsync(Guid tourId, Guid authorId, Guid keypointId)
    {
        var keypoint = await _tourReadRepository.GetKeypointAsync(tourId, authorId, keypointId);
        if (keypoint is null)
        {
            throw new NotFoundException($"Keypoint {keypointId} does not exist.");
        }
        return keypoint;
    }

    // The keypoint is looked up through the author's tour, so only the author gets the image.
    public async Task<ImageContentDto> GetKeypointImageAsync(Guid tourId, Guid authorId, Guid keypointId)
    {
        var keypoint = await GetKeypointAsync(tourId, authorId, keypointId);

        var storedFileName = await _imageReadRepository.GetStoredFileNameAsync(keypoint.ImageId);
        if (storedFileName is null)
        {
            throw new NotFoundException($"Image {keypoint.ImageId} does not exist.");
        }

        var content = await _imageStorage.ReadAsync(storedFileName);
        if (content is null)
        {
            throw new NotFoundException($"Image {keypoint.ImageId} does not exist.");
        }
        return content;
    }
}
