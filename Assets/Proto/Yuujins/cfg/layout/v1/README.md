# Layout proto (client)

Message: `Tile`, `Tab`, `Layout`, `Layout.Request.Read`, `Layout.Response.Read` — dùng cho API get layout (tabs + tiles) và lưu local để hiển thị UI theo layout.

## Biên dịch (tự chạy trên máy)

Từ thư mục gốc project Unity (chứa `Assets`):

```bash
# C# output vào Assets/YuujinsGenerated (cùng thư mục Baccarat.cs, Card.cs)
protoc --csharp_out=Assets/YuujinsGenerated -I Assets/Proto/Yuujins Assets/Proto/Yuujins/cfg/layout/v1/layout.proto
```

- Namespace trong file `.cs` phải là `Yuujins.Cfg.Layout.V1` (đã set trong proto bằng `option csharp_namespace`).
- Nếu assembly `YuujinsGenerated.asmdef` có sẵn, thêm file `Layout.cs` (hoặc tên file protoc sinh ra) vào assembly đó.
