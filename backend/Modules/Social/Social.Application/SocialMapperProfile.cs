using AutoMapper;
using Social.Application.Blogs;
using Social.Application.Clubs;
using Social.Domain.Blogs;
using Social.Domain.Clubs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>();
        CreateMap<Comment, CommentDto>();
        CreateMap<Club, ClubDto>();
    }
}
