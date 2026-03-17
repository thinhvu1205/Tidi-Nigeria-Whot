# Layout proto (client)

Biên dịch từ thư mục gốc project (chứa `Assets`):

```bash
# Ví dụ với protoc (C# out):
protoc --csharp_out=Assets/YuujinsGenerated --proto_path=Assets/Proto/Yuujins layout.proto
# Hoặc nếu dùng path đầy đủ:
# protoc --csharp_out=Assets/YuujinsGenerated -I Assets/Proto/Yuujins Assets/Proto/Yuujins/cfg/layout/v1/layout.proto
```

Sau khi sinh code, kiểm tra namespace trong file `.cs` là `Yuujins.Cfg.Layout.V1` (đã set trong proto bằng `option csharp_namespace`).
Nếu thư mục `cfg/layout/v1` nằm trong `Assets/Proto/Yuujins`, có thể cần thêm `-I Assets/Proto/Yuujins` và đường dẫn đầy đủ tới `cfg/layout/v1/layout.proto`.
