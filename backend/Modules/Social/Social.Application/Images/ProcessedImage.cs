using Social.Domain.Images;

namespace Social.Application.Images;

public sealed record ProcessedImage(byte[] Content, ImageDimensions OriginalDimensions);
