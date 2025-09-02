using System.Collections.Generic;

namespace OkeanBook.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<PostViewModel> Posts { get; set; } = new List<PostViewModel>();
        public IEnumerable<NotificationItemViewModel> Notifications { get; set; } = new List<NotificationItemViewModel>();
        public IEnumerable<UserViewModel> FriendSuggestions { get; set; } = new List<UserViewModel>();
        public IEnumerable<UserViewModel> RecentFriends { get; set; } = new List<UserViewModel>();
        public IEnumerable<GroupViewModel> UserGroups { get; set; } = new List<GroupViewModel>();
        public IEnumerable<TrendingTopicViewModel> TrendingTopics { get; set; } = new List<TrendingTopicViewModel>();
        public int CurrentPage { get; set; }
        public int UnreadNotificationCount { get; set; }
    }

    public class TrendingTopicViewModel
    {
        public string Hashtag { get; set; } = string.Empty;
        public int PostCount { get; set; }
    }

    public class NotificationItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CreatedAgo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}


