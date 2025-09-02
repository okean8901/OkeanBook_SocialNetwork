using AutoMapper;
using OkeanBook.Models;
using OkeanBook.Models.ViewModels;

namespace OkeanBook.Mappings
{
    /// <summary>
    /// AutoMapper profile cho mapping giữa Models và ViewModels
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dest => dest.FriendCount, opt => opt.Ignore())
                .ForMember(dest => dest.PostCount, opt => opt.Ignore())
                .ForMember(dest => dest.FriendStatus, opt => opt.Ignore());

            CreateMap<ApplicationUser, ProfileViewModel>();

            // Message mappings
            CreateMap<Message, MessageViewModel>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.UserName))
                .ForMember(dest => dest.SenderAvatar, opt => opt.MapFrom(src => src.Sender.Avatar))
                .ForMember(dest => dest.IsOwn, opt => opt.Ignore());

            // Group mappings
            CreateMap<Group, GroupViewModel>()
                .ForMember(dest => dest.Members, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageTime, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore());

            CreateMap<GroupMember, GroupMemberViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.Status));

            // Post mappings
            CreateMap<Post, PostViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
                .ForMember(dest => dest.Replies, opt => opt.Ignore());

            // Chat mappings
            CreateMap<ApplicationUser, ChatUserViewModel>()
                .ForMember(dest => dest.LastMessage, opt => opt.Ignore())
                .ForMember(dest => dest.LastMessageTime, opt => opt.Ignore())
                .ForMember(dest => dest.UnreadCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsTyping, opt => opt.Ignore());
        }
    }
}
