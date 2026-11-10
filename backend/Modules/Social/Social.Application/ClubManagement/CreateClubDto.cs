namespace Social.Application.ClubManagement;

public sealed record CreateClubDto(string Name, string Description, List<string>? Images);
