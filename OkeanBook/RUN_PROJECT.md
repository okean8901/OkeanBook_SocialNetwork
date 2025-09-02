# Hướng dẫn chạy dự án OkeanBook

## Yêu cầu hệ thống

- .NET 8.0 SDK
- MySQL Server 8.0+ hoặc MariaDB 10.3+
- Visual Studio 2022 hoặc VS Code

## Bước 1: Cài đặt MySQL

Xem file `DATABASE_SETUP.md` để biết cách cài đặt MySQL.

## Bước 2: Cấu hình Database

1. Tạo database `OkeanBookDB` trong MySQL
2. Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OkeanBookDB;Uid=root;Pwd=your_password;"
  }
}
```

## Bước 3: Cài đặt Entity Framework Tools

```bash
dotnet tool install --global dotnet-ef
```

## Bước 4: Áp dụng Migrations

```bash
cd OkeanBook
dotnet ef database update
```

## Bước 5: Chạy dự án

### Cách 1: Sử dụng Visual Studio
1. Mở file `OkeanBook.sln` trong Visual Studio
2. Nhấn F5 hoặc Ctrl+F5 để chạy

### Cách 2: Sử dụng Command Line
```bash
cd OkeanBook
dotnet run
```

### Cách 3: Sử dụng VS Code
1. Mở thư mục dự án trong VS Code
2. Nhấn F5 hoặc sử dụng terminal: `dotnet run`

## Bước 6: Truy cập ứng dụng

Mở trình duyệt và truy cập: `https://localhost:5001` hoặc `http://localhost:5000`

## Tính năng chính

### 1. Đăng ký/Đăng nhập
- Đăng ký tài khoản mới
- Đăng nhập với email và mật khẩu
- Quên mật khẩu

### 2. Quản lý hồ sơ
- Xem và chỉnh sửa thông tin cá nhân
- Upload avatar
- Thay đổi mật khẩu

### 3. Quản lý bạn bè
- Gửi lời mời kết bạn
- Chấp nhận/từ chối lời mời
- Xem danh sách bạn bè

### 4. Chat thời gian thực
- Chat 1-1 với bạn bè
- Chat nhóm
- Gửi tin nhắn, hình ảnh, file
- Thu hồi tin nhắn

### 5. Bài viết và tương tác
- Tạo bài viết
- Like và comment
- Chia sẻ bài viết

### 6. Thông báo
- Thông báo real-time
- Thông báo về like, comment, friend request

## Cấu trúc dự án

```
OkeanBook/
├── Controllers/          # Controllers xử lý HTTP requests
├── Models/              # Data models và ViewModels
├── Views/               # Razor views
├── Services/            # Business logic services
├── Data/                # Database context
├── Hubs/                # SignalR hubs cho real-time
├── Mappings/            # AutoMapper profiles
├── wwwroot/             # Static files (CSS, JS, images)
└── Migrations/          # Entity Framework migrations
```

## API Endpoints

### Authentication
- `GET /Account/Login` - Trang đăng nhập
- `POST /Account/Login` - Xử lý đăng nhập
- `GET /Account/Register` - Trang đăng ký
- `POST /Account/Register` - Xử lý đăng ký
- `POST /Account/Logout` - Đăng xuất

### Chat
- `GET /Chat` - Trang chat chính
- `GET /Chat/GetMessages` - Lấy tin nhắn
- `POST /Chat/MarkAsRead` - Đánh dấu đã đọc
- `POST /Chat/RecallMessage` - Thu hồi tin nhắn

### Friends
- `GET /Friend` - Danh sách bạn bè
- `POST /Friend/SendRequest` - Gửi lời mời kết bạn
- `POST /Friend/AcceptRequest` - Chấp nhận lời mời
- `POST /Friend/RejectRequest` - Từ chối lời mời

### Posts
- `GET /Home` - Trang chủ (news feed)
- `POST /Home/CreatePost` - Tạo bài viết
- `POST /Home/LikePost` - Like bài viết
- `POST /Home/Comment` - Comment bài viết

## SignalR Hubs

### ChatHub
- `JoinChat` - Tham gia chat
- `SendMessage` - Gửi tin nhắn
- `SendGroupMessage` - Gửi tin nhắn nhóm
- `Typing` - Trạng thái đang gõ
- `StopTyping` - Dừng gõ

## Troubleshooting

### Lỗi kết nối database
- Kiểm tra MySQL service đang chạy
- Kiểm tra connection string
- Kiểm tra quyền truy cập database

### Lỗi build
- Chạy `dotnet restore` để restore packages
- Kiểm tra .NET 8.0 SDK đã cài đặt
- Xóa thư mục `bin` và `obj` rồi build lại

### Lỗi migration
- Kiểm tra database đã tạo
- Chạy `dotnet ef database update` để áp dụng migrations
- Nếu cần, xóa database và tạo lại

## Phát triển thêm

### Thêm tính năng mới
1. Tạo model trong thư mục `Models/`
2. Thêm DbSet trong `ApplicationDbContext`
3. Tạo migration: `dotnet ef migrations add FeatureName`
4. Áp dụng migration: `dotnet ef database update`
5. Tạo service trong thư mục `Services/`
6. Tạo controller trong thư mục `Controllers/`
7. Tạo view trong thư mục `Views/`

### Cấu hình thêm
- Thêm cấu hình trong `appsettings.json`
- Cấu hình logging trong `Program.cs`
- Thêm middleware nếu cần

## Liên hệ

Nếu có vấn đề gì, vui lòng tạo issue hoặc liên hệ qua email.
