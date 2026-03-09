#!/bin/bash
# Biên dịch proto Yuujins ra C# bằng protoc; output ra thư mục RIÊNG + prefix namespace "Yuujins."
# → Không xung đột với Proto (Match, User, Friend...): code cũ giữ Proto.Match; Yuujins dùng Yuujins.Match.V1.*

set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
YUUIINS_ROOT="$SCRIPT_DIR/Yuujins"
OUT_CS="$(cd "$SCRIPT_DIR/.." && pwd)/YuujinsGenerated"

if [ ! -d "$YUUIINS_ROOT" ]; then
  echo "Error: $YUUIINS_ROOT not found. Copy proto into Assets/Proto/Yuujins/ (e.g. run copy_yuujins_from_phil.sh)."
  exit 1
fi

mkdir -p "$OUT_CS"

PROTO_PATHS=("--proto_path=$YUUIINS_ROOT")
[ -d "/usr/include" ] && PROTO_PATHS+=("--proto_path=/usr/include")

echo "Proto root: $YUUIINS_ROOT"
echo "Output C#:  $OUT_CS"

cd "$YUUIINS_ROOT"
FILES=(
  common/v1/empty.proto common/v1/pagination.proto common/v1/position.proto
  common/v1/asset.proto common/v1/range.proto common/v1/timerange.proto
  user/v1/user.proto cfg/game/v1/game.proto cfg/bet/v1/bet.proto cfg/banner/v1/banner.proto
  match/v1/match.proto friend/v1/friend.proto rank/v1/rank.proto rewards/v1/checkin.proto
  communication/mail/v1/mail.proto economy/wallet/v1/wallet.proto promotion/coupon/v1/coupon.proto
)
TO_COMPILE=()
for f in "${FILES[@]}"; do [ -f "$f" ] && TO_COMPILE+=("$f"); done
if [ ${#TO_COMPILE[@]} -eq 0 ]; then
  echo "Error: No .proto files found under $YUUIINS_ROOT"; exit 1
fi

protoc --csharp_out="$OUT_CS" "${PROTO_PATHS[@]}" "${TO_COMPILE[@]}"

# 1) Thêm prefix "Yuujins." vào dòng namespace
add_prefix() {
  local f="$1"
  [ ! -f "$f" ] && return
  if grep -q '^namespace Yuujins\.' "$f" 2>/dev/null; then return; fi
  sed -i.bak 's/^namespace /namespace Yuujins./' "$f" 2>/dev/null || true
  rm -f "${f}.bak"
}
export -f add_prefix
find "$OUT_CS" -name "*.cs" -exec bash -c 'add_prefix "$1"' _ {} \;

# 2) Sửa tham chiếu global:: trong generated (Cfg.Game.V1, Match.V1, ...) thành global::Yuujins....
for f in "$OUT_CS"/*.cs; do
  [ ! -f "$f" ] && continue
  sed -i.bak -E 's/global::(Common|User|Match|Friend|Rank|Rewards|Cfg|Communication|Economy|Promotion)\./global::Yuujins.\1./g' "$f" 2>/dev/null || true
  rm -f "${f}.bak"
done

echo "Done. Output: Assets/YuujinsGenerated/ (namespace Yuujins.*)"
echo "Code: using Yuujins.Match.V1; Yuujins.User.V1; ... — không đụng Proto."