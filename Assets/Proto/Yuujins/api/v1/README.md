# Baccarat API proto (Yuujins server)

Proto copy từ server `common/proto/api/v1/` để client biên dịch ra C#.

## File

- **card.proto** – Card (rank, suit)
- **baccarat.proto** – OpCode, request/update messages cho Baccarat

Cả hai file có **`option csharp_namespace = "Yuujins.Api.V1";`** để lần biên dịch ra C# tự dùng namespace `Yuujins.Api.V1` (không cần sửa tay `Api.V1` → `Yuujins.Api.V1` trong file generated).

## Biên dịch (C#)

Đứng tại thư mục gốc project (hoặc thư mục chứa `Assets/Proto`), chạy:

```bash
# Ví dụ: protoc với include root là Assets/Proto/Yuujins, output C# vào Assets/YuujinsGenerated hoặc Scripts
protoc -I Assets/Proto/Yuujins \
  api/v1/card.proto \
  api/v1/baccarat.proto \
  --csharp_out=Assets/YuujinsGenerated
```

Nếu project dùng **buf**:

```bash
# Trong thư mục có buf.gen.yaml
buf generate
```

Sau khi generate, trong C# **bắt buộc** dùng:

```csharp
using Yuujins.Api.V1;
// Sau đó dùng: BaccaratBetRequest, BaccaratGameStateUpdate, BaccaratTableUpdate, ...
```

**Không dùng** `using Api.V1;` — namespace thật sự là `Yuujins.Api.V1`, không phải `Api.V1`.

---

## Tại sao biên dịch ra mà không import được?

1. **Sai namespace trong code**  
   File generated có `namespace Yuujins.Api.V1 { ... }`. Trong script phải gõ:
   - `using Yuujins.Api.V1;`  
   Nếu gõ `using Api.V1;` thì compiler báo không tìm thấy (vì không có namespace `Api.V1`).

2. **Output không nằm trong Unity**  
   `--csharp_out` (hoặc buf output) phải trỏ vào thư mục **bên trong** `Assets/` (ví dụ `Assets/YuujinsGenerated`). Nếu ra ngoài (ví dụ `gen/`, `output/`) thì Unity không compile và không “import” được.

3. **Codegen bỏ qua `csharp_namespace`**  
   Một số tool cũ vẫn sinh ra `namespace Api.V1` và `global::Api.V1.*`. Khi đó:
   - Hoặc dùng `using Api.V1;` nếu file thật sự là `namespace Api.V1`,  
   - Hoặc sau khi generate, tìm và thay toàn bộ `global::Api.V1.` → `global::Yuujins.Api.V1.` trong `Baccarat.cs` và `Card.cs`, rồi trong script dùng `using Yuujins.Api.V1;`.

4. **Thiếu dependency**  
   File generated dùng `Google.Protobuf`. Project Unity phải có package/code đã reference **Google.Protobuf** (và cùng version) thì mới compile được.

## Đồng bộ với server

Proto gốc nằm ở:

- `Server/yuujins/common/proto/api/v1/card.proto`
- `Server/yuujins/common/proto/api/v1/baccarat.proto`

Khi server đổi định nghĩa, copy lại hai file trên vào đây rồi biên dịch lại.
