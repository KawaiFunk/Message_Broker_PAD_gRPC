#!/usr/bin/env bash
set -euo pipefail

# Intrare/ieșire
PROTO_DIR="proto"
OUT_DIR="generated"

mkdir -p "${OUT_DIR}"

python -m grpc_tools.protoc \
  -I"${PROTO_DIR}" \
  --python_out="${OUT_DIR}" \
  --grpc_python_out="${OUT_DIR}" \
  "${PROTO_DIR}/publisher.proto"
  

# Fix importuri relative (dacă vrei pachet importabil)
touch "${OUT_DIR}/__init__.py"
