# OkeanBook - Mạng xã hội thời gian thực

OkeanBook là một ứng dụng mạng xã hội thời gian thực được xây dựng bằng ASP.NET Core MVC với MySQL, tích hợp SignalR cho chat realtime.

## 🚀 Tính năng chính

### 👤 Quản lý người dùng
- Đăng ký/Đăng nhập/Đăng xuất với ASP.NET Core Identity
- Cập nhật hồ sơ cá nhân (avatar, mô tả, trạng thái)
- Đổi mật khẩu
- Quản lý trạng thái online/offline

### 👥 Quản lý bạn bè
- Gửi/nhận lời mời kết bạn
- Chấp nhận/từ chối lời mời
- Chặn/bỏ chặn người dùng
- Xóa bạn bè
- Tìm kiếm người dùng

### 💬 Chat thời gian thực
- Chat 1-1 với SignalR
- Chat nhóm
- Hiển thị trạng thái online/offline
- Thông báo "đang nhập..."
- Đánh dấu tin nhắn đã đọc
- Thu hồi/xóa tin nhắn
- Hỗ trợ tin nhắn văn bản, emoji, ảnh, file

### 📝 Bài viết và tương tác
- Tạo bài viết (text, ảnh, video)
- Like/Unlike bài viết
- Bình luận bài viết
- Xem bảng tin từ bạn bè
- Tìm kiếm bài viết

### 🔔 Thông báo
- Thông báo realtime cho tin nhắn mới
- Thông báo lời mời kết bạn
- Thông báo like/comment bài viết
- Thông báo nhóm

### 👥 Quản lý nhóm
- Tạo nhóm chat
- Thêm/xóa thành viên
- Phân quyền (Owner, Admin, Member)
- Chỉnh sửa thông tin nhóm

## 🛠️ Công nghệ sử dụng

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **MySQL** - Database
- **SignalR** - Realtime communication
- **ASP.NET Core Identity** - Authentication & Authorization
- **AutoMapper** - Object mapping

### Frontend
- **Bootstrap 5** - UI Framework
- **jQuery** - JavaScript library
- **Font Awesome** - Icons
- **Razor Views** - Server-side rendering

### Architecture
- **Repository Pattern** - Data access layer
- **Dependency Injection** - IoC container
- **MVC Pattern** - Application architecture

## 📋 Yêu cầu hệ thống

- .NET 8.0 SDK
- MySQL Server 8.0+
- Visual Studio 2022 hoặc VS Code

## 🚀 Hướng dẫn cài đặt

### 1. Clone repository
```bash
git clone <repository-url>
cd OkeanBook
```

### 2. Cấu hình database
1. Tạo database MySQL tên `OkeanBookDB`
2. Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OkeanBookDB;Uid=root;Pwd=your_password;"
  }
}
```

### 3. Cài đặt dependencies
```bash
dotnet restore
```

### 4. Tạo và chạy migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Chạy ứng dụng
```bash
dotnet run
```

Truy cập: `https://localhost:5001`

## 📁 Cấu trúc dự án

```
OkeanBook/
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── ChatController.cs
│   ├── FriendController.cs
│   ├── ProfileController.cs
│   └── GroupController.cs
├── Models/              # Data Models
│   ├── ApplicationUser.cs
│   ├── Friend.cs
│   ├── Message.cs
│   ├── Group.cs
│   ├── Post.cs
│   ├── Comment.cs
│   ├── Notification.cs
│   └── ViewModels/      # View Models
├── Views/               # Razor Views
│   ├── Account/
│   ├── Home/
│   ├── Chat/
│   ├── Friend/
│   ├── Profile/
│   ├── Group/
│   └── Shared/
├── Services/            # Business Logic
│   ├── Interfaces/
│   ├── UserService.cs
│   ├── FriendService.cs
│   ├── MessageService.cs
│   ├── PostService.cs
│   ├── GroupService.cs
│   └── NotificationService.cs
├── Data/                # Data Access
│   └── ApplicationDbContext.cs
├── Hubs/                # SignalR Hubs
│   └── ChatHub.cs
├── Mappings/            # AutoMapper Profiles
│   └── AutoMapperProfile.cs
└── wwwroot/             # Static Files
    ├── css/
    ├── js/
    ├── img/
    └── lib/
```

## 🗄️ Database Schema

### Bảng chính:
- **AspNetUsers** - Thông tin người dùng (Identity)
- **Friends** - Quan hệ bạn bè
- **Messages** - Tin nhắn
- **Groups** - Nhóm chat
- **GroupMembers** - Thành viên nhóm
- **Posts** - Bài viết
- **Comments** - Bình luận
- **Notifications** - Thông báo

## 🔧 Cấu hình

### SignalR
SignalR được cấu hình để hỗ trợ:
- Chat realtime
- Thông báo trạng thái online/offline
- Thông báo "đang nhập..."
- Thông báo tin nhắn mới

### Identity
ASP.NET Core Identity được cấu hình với:
- Password requirements
- Account lockout
- Cookie authentication
- User management

## 🚀 Tính năng nâng cao (có thể mở rộng)

- [ ] Gọi thoại/video (WebRTC)
- [ ] Stories (ảnh/video 24h)
- [ ] Tìm kiếm nâng cao
- [ ] Admin dashboard
- [ ] Push notifications
- [ ] File sharing
- [ ] Emoji reactions
- [ ] Message encryption

## 🤝 Đóng góp

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📝 License

Distributed under the MIT License. See `LICENSE` for more information.

## 👨‍💻 Tác giả

**OkeanBook Team**
- Email: contact@okeanbook.com
- Project Link: [https://github.com/okeanbook/okeanbook](https://github.com/okeanbook/okeanbook)

## 🙏 Acknowledgments

- Bootstrap team for the amazing UI framework
- Microsoft for ASP.NET Core and SignalR
- Font Awesome for the beautiful icons
- MySQL team for the robust database
