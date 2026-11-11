namespace Social.Application.ClubManagement;

public sealed record UpdateClubDto(string Name, string Description, List<string>? Images);
