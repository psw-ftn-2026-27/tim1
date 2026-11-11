namespace Social.Application.ClubManagement;

public sealed record ClubDto(Guid Id, Guid OwnerId, string Name, string Description, List<string> Images);
