# Proto Yuujins – biên dịch bằng protoc (terminal)

- **Proto Nigeria (server cũ):** nằm ở `Assets/Scripts/Protobuf/*.proto`, đã có sẵn file `.cs` (namespace **Proto**). **Không** dùng script này, không đụng tới.
- **Proto Yuujins:** nằm trong `Yuujins/`. Biên dịch bằng **protoc**, output ra thư mục **riêng** `Assets/YuujinsGenerated/`, và script **tự thêm prefix namespace** `Yuujins.` (→ **Yuujins.Match.V1**, **Yuujins.User.V1**, …) → **không xung đột** với `Proto.Match`, `Proto.User` trong code Nigeria.

## Cách biên dịch

Từ **thư mục gốc Unity project** (chứa `Assets/`):

```bash
chmod +x Assets/Proto/generate_yuujins.sh
./Assets/Proto/generate_yuujins.sh
```

Nếu thiếu proto, copy từ `Tidi-Phil-Win777/Assets/Proto/Yuujins/` vào `Assets/Proto/Yuujins/` (ví dụ chạy `copy_yuujins_from_phil.sh`).

Sau khi chạy xong:
- Generated C# nằm ở **`Assets/YuujinsGenerated/`** (thư mục tách hẳn, không nằm trong Scripts/Protobuf).
- Trong Unity, thêm folder **`Assets/YuujinsGenerated`** vào project (nếu chưa có).
- Trong code dùng **`using Yuujins.Match.V1;`**, **`using Yuujins.User.V1;`**, … (có prefix **Yuujins.**) — không đụng tới `Proto.Match`, `Proto.User` ở chỗ khác.

## Cấu trúc

| Vị trí | Nội dung | Namespace |
|--------|----------|-----------|
| `Assets/Scripts/Protobuf/*.proto` + `*.cs` | Nigeria (giữ nguyên) | `Proto` |
| `Assets/Proto/Yuujins/**/*.proto` | Nguồn Yuujins | — |
| **`Assets/YuujinsGenerated/*.cs`** | C# generate từ Yuujins (có post-process thêm prefix) | **`Yuujins.Match.V1`**, **`Yuujins.User.V1`**, … |

**Sau khi generate:** Trong `DataSender.cs` (và chỗ gọi API Yuujins), dùng `using Yuujins.Match.V1;`, `using Yuujins.User.V1;`, … và bỏ comment các region Yuujins nếu cần. Code Nigeria giữ nguyên `Proto.*`.

(Nếu trước đây bạn đã generate vào `Assets/Scripts/Protobuf/GeneratedYuujins/`, có thể xóa thư mục đó sau khi chuyển sang `Assets/YuujinsGenerated/`.)
