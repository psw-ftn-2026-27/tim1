using Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

internal sealed class IdentityApi : IIdentityApi
{
    private const int MaxPageSize = 50;

    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityApi(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityUserPageDto> SearchUsersAsync(string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var users = _userManager.Users;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var escaped = text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            users = users.Where(user => EF.Functions.ILike(user.Email!, $"%{escaped}%", "\\"));
        }

        var totalCount = await users.CountAsync();
        var items = await users
            .OrderBy(user => user.Email)
            .ThenBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new IdentityUserDto(user.Id, user.Email!))
            .ToListAsync();

        return new IdentityUserPageDto(items, totalCount);
    }

    public async Task<List<IdentityUserDto>> GetUsersAsync(IReadOnlyCollection<Guid> userIds)
    {
        return await _userManager.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new IdentityUserDto(user.Id, user.Email!))
            .ToListAsync();
    }
}
