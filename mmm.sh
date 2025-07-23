#!/bin/bash
set -e


tmpfile=$(mktemp)
trap 'rm -f "$tmpfile"; echo "Deleted MMM after use"' EXIT

echo "Downloading Monke Mod Manager..."
curl -fsSL one sec -o "$tmpfile"

chmod +x "$tmpfile"

echo "Running MMM"
"$tmpfile"
