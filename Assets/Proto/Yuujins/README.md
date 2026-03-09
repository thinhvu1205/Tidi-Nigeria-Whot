# Nguồn proto Yuujins

Đặt toàn bộ file `.proto` (common/, user/, match/, cfg/, friend/, rank/, rewards/, communication/, economy/, promotion/) trong thư mục này.

Nếu đang làm lại từ đầu: copy từ **Tidi-Phil-Win777**:

```bash
./copy_yuujins_from_phil.sh
```

Hoặc copy thủ công từ `Tidi-Phil-Win777/Assets/Proto/Yuujins/` vào đây.

Sau đó chạy `./generate_yuujins.sh` từ thư mục Unity (có Assets/) để generate C# ra `Assets/Scripts/Protobuf/GeneratedYuujins/` — không đè lên Proto (Nigeria).
