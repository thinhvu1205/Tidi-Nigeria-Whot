#!/bin/bash
# Copy toàn bộ proto Yuujins từ project Tidi-Phil-Win777 vào Assets/Proto/Yuujins (làm lại từ đầu).
# Chạy từ repo LearnCode hoặc chỉnh SRC bên dưới.

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DST="$SCRIPT_DIR/Yuujins"
# Chỉnh đường dẫn nếu Phil project nằm chỗ khác
SRC="${SCRIPT_DIR}/../../../Tidi-Phil-Win777/Assets/Proto/Yuujins"
if [ ! -d "$SRC" ]; then
  SRC="/mnt/e/LearnCode/Unity/Tidi-Phil-Win777/Assets/Proto/Yuujins"
fi
if [ ! -d "$SRC" ]; then
  echo "Source not found: $SRC"
  echo "Copy thủ công: nội dung thư mục Yuujins (common/, user/, match/, ...) vào Assets/Proto/Yuujins/"
  exit 1
fi
rm -rf "$DST"
mkdir -p "$DST"
cp -r "$SRC"/* "$DST"/
echo "Copied Yuujins proto to $DST"
