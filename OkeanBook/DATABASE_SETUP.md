# Hướng dẫn cài đặt Database cho OkeanBook

## Yêu cầu hệ thống

1. **MySQL Server 8.0+** hoặc **MariaDB 10.3+**
2. **.NET 8.0 SDK**
3. **Entity Framework Core Tools**

## Bước 1: Cài đặt MySQL

### Windows:
1. Tải MySQL Community Server từ: https://dev.mysql.com/downloads/mysql/
2. Cài đặt với cấu hình mặc định
3. Ghi nhớ mật khẩu root

### Linux (Ubuntu/Debian):
```bash
sudo apt update
sudo apt install mysql-server
sudo mysql_secure_installation
```

### macOS:
```bash
brew install mysql
brew services start mysql
```

## Bước 2: Tạo Database

1. Đăng nhập vào MySQL:
```bash
mysql -u root -p
```

2. Tạo database:
```sql
CREATE DATABASE OkeanBookDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

3. Tạo user (tùy chọn):
```sql
CREATE USER 'okeanbook'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON OkeanBookDB.* TO 'okeanbook'@'localhost';
FLUSH PRIVILEGES;
```

## Bước 3: Cấu hình Connection String

Cập nhật file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OkeanBookDB;Uid=root;Pwd=your_password;"
  }
}
```

Hoặc nếu sử dụng user riêng:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OkeanBookDB;Uid=okeanbook;Pwd=your_password;"
  }
}
```

## Bước 4: Cài đặt Entity Framework Tools

```bash
dotnet tool install --global dotnet-ef
```

## Bước 5: Áp dụng Migrations

1. Di chuyển vào thư mục dự án:
```bash
cd OkeanBook
```

2. Áp dụng migrations:
```bash
dotnet ef database update
```

## Bước 6: Kiểm tra kết nối

Chạy ứng dụng:
```bash
dotnet run
```

Nếu không có lỗi, database đã được cấu hình thành công!

## Troubleshooting

### Lỗi "Access denied for user 'root'@'localhost'":
- Kiểm tra mật khẩu trong connection string
- Đảm bảo MySQL service đang chạy
- Thử reset mật khẩu root MySQL

### Lỗi "Database does not exist":
- Tạo database trước khi chạy migration
- Kiểm tra tên database trong connection string

### Lỗi "Table already exists":
- Xóa database và tạo lại
- Hoặc sử dụng `dotnet ef database drop` rồi `dotnet ef database update`

## Cấu trúc Database

Sau khi áp dụng migrations, database sẽ có các bảng:

- **AspNetUsers** - Thông tin người dùng
- **AspNetRoles** - Vai trò người dùng
- **AspNetUserRoles** - Liên kết user-role
- **Friends** - Quan hệ bạn bè
- **Messages** - Tin nhắn chat
- **Groups** - Nhóm chat
- **GroupMembers** - Thành viên nhóm
- **Posts** - Bài viết
- **Comments** - Bình luận
- **Notifications** - Thông báo
- Và các bảng khác...

## Backup và Restore

### Backup:
```bash
mysqldump -u root -p OkeanBookDB > okeanbook_backup.sql
```

### Restore:
```bash
mysql -u root -p OkeanBookDB < okeanbook_backup.sql
```
