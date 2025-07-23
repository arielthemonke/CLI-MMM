#!/bin/bash
set -e


tmpfile=$(mktemp)
trap 'rm -f "$tmpfile"; echo "Deleted MMM after use"' EXIT

echo "Downloading Monke Mod Manager..."
curl -fsSL https://github.com/arielthemonke/CLI-MMM/releases/download/v1.0.0/MMMCLI -o "$tmpfile"

chmod +x "$tmpfile"

echo "Running MMM"
"$tmpfile"
