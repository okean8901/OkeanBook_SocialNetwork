using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OkeanBook.Models;

namespace OkeanBook.Data
{
    /// <summary>
    /// DbContext chính của ứng dụng
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets cho các entity
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Avatar).HasMaxLength(200);
                entity.Property(e => e.Bio).HasMaxLength(500);
                entity.Property(e => e.Status).HasDefaultValue(UserStatus.Offline);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // Cấu hình Friend
            builder.Entity<Friend>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasDefaultValue(FriendStatus.Pending);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Index để tối ưu hóa truy vấn - sử dụng prefix để tránh lỗi key length
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Friends_UserId");
                entity.HasIndex(e => e.FriendId).HasDatabaseName("IX_Friends_FriendId");

                // Foreign key relationships
                entity.HasOne(e => e.User)
                    .WithMany(e => e.SentFriendRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FriendUser)
                    .WithMany(e => e.ReceivedFriendRequests)
                    .HasForeignKey(e => e.FriendId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Message
            builder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.MediaUrl).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(200);
                entity.Property(e => e.Type).HasDefaultValue(MessageType.Text);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsRecalled).HasDefaultValue(false);
                entity.Property(e => e.SentAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Foreign key relationships
                entity.HasOne(e => e.Sender)
                    .WithMany(e => e.SentMessages)
                    .HasForeignKey(e => e.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Receiver)
                    .WithMany(e => e.ReceivedMessages)
                    .HasForeignKey(e => e.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Group)
                    .WithMany(e => e.Messages)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Group
            builder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Avatar).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(e => e.Owner)
                    .WithMany(e => e.OwnedGroups)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình GroupMember
            builder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).HasDefaultValue(GroupRole.Member);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Unique constraint để tránh duplicate memberships
                entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();

                entity.HasOne(e => e.Group)
                    .WithMany(e => e.Members)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.GroupMemberships)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Post
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.MediaUrl).HasMaxLength(500);
                entity.Property(e => e.Type).HasDefaultValue(PostType.Text);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.CommentCount).HasDefaultValue(0);
                entity.Property(e => e.ShareCount).HasDefaultValue(0);
                entity.Property(e => e.IsPublic).HasDefaultValue(true);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình PostLike
            builder.Entity<PostLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Unique constraint để tránh duplicate likes
                entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Likes)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Comment
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(e => e.Post)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ParentComment)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình CommentLike
            builder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Unique constraint để tránh duplicate likes
                entity.HasIndex(e => new { e.CommentId, e.UserId }).IsUnique();

                entity.HasOne(e => e.Comment)
                    .WithMany(e => e.Likes)
                    .HasForeignKey(e => e.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Notification
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).HasDefaultValue(NotificationType.Info);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RelatedUser)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RelatedPost)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedPostId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RelatedMessage)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedMessageId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RelatedGroup)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedGroupId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
