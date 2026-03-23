#!/bin/bash
# Auto compile ALL proto in Yuujins -> C# (Unity)
# Output riêng + prefix namespace Yuujins.*

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
YUUIINS_ROOT="$SCRIPT_DIR/Yuujins"
OUT_CS="$(cd "$SCRIPT_DIR/.." && pwd)/YuujinsGenerated"

if [ ! -d "$YUUIINS_ROOT" ]; then
  echo "Error: $YUUIINS_ROOT not found."
  exit 1
fi

mkdir -p "$OUT_CS"

PROTO_PATHS=("--proto_path=$YUUIINS_ROOT")
[ -d "/usr/include" ] && PROTO_PATHS+=("--proto_path=/usr/include")

echo "Proto root: $YUUIINS_ROOT"
echo "Output C#:  $OUT_CS"

cd "$YUUIINS_ROOT"

# 🔥 Lấy toàn bộ file proto
PROTO_FILES=$(find . -name "*.proto" | sed 's|^\./||')

if [ -z "$PROTO_FILES" ]; then
  echo "Error: No .proto files found"
  exit 1
fi

echo "Found proto files:"
echo "$PROTO_FILES"

# 🚀 Compile tất cả proto
protoc --csharp_out="$OUT_CS" "${PROTO_PATHS[@]}" $PROTO_FILES

echo "Compile done!"

# ==============================
# 🔧 FIX NAMESPACE
# ==============================

# 1) Thêm prefix "Yuujins." vào namespace
add_prefix() {
  local f="$1"
  [ ! -f "$f" ] && return

  if grep -q '^namespace Yuujins\.' "$f" 2>/dev/null; then
    return
  fi

  sed -i.bak 's/^namespace /namespace Yuujins./' "$f" 2>/dev/null || true
  rm -f "${f}.bak"
}

export -f add_prefix
find "$OUT_CS" -name "*.cs" -exec bash -c 'add_prefix "$1"' _ {} \;

# 2) Fix global:: reference
for f in "$OUT_CS"/*.cs; do
  [ ! -f "$f" ] && continue

  sed -i.bak -E 's/global::(Analytic|Common|User|Match|Friend|Rank|Rewards|Cfg|Communication|Economy|Promotion)\./global::Yuujins.\1./g' "$f" 2>/dev/null || true
  rm -f "${f}.bak"
done

echo "Done!"
echo "Output: $OUT_CS"
echo "Use in code: using Yuujins.*;"