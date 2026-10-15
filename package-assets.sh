#!/usr/bin/env bash
# Populate <outroot>/bin with the sing-box core and the geo / rule-set assets.
#
# Usage: package-assets.sh <outroot> <target> <arch>
#   target: windows | linux | macos
#   arch:   64 | x64 | amd64 | arm64
#
# sing-box comes from the KexiChanProjectProxy fork, which publishes
# windows and linux amd64/arm64 archives only. Other platforms are packaged
# without a core; v2rayN then reports the core as missing.
# Set SING_VER (e.g. 1.14.2.7) to pin a version instead of the latest release.
set -euo pipefail

SINGBOX_REPO="KexiChanProjectProxy/sing-box"

outroot="${1:?outroot required}"
target="${2:?target required}"
arch="${3:?arch required}"

case "$arch" in
  64|x64|amd64) goarch="amd64" ;;
  arm64|aarch64) goarch="arm64" ;;
  *) echo "[!] Unknown arch: $arch" >&2; exit 1 ;;
esac

bin_dir="$outroot/bin"
srss_dir="$bin_dir/srss"
mkdir -p "$bin_dir/sing_box" "$srss_dir"

singbox_latest_version() {
  curl -fsSL "https://api.github.com/repos/${SINGBOX_REPO}/releases/latest" \
    | grep -Eo '"tag_name":\s*"v[^"]+"' \
    | sed -E 's/.*"v([^"]+)".*/\1/' \
    | head -n1
}

download_singbox() {
  local ext="" exe="sing-box" ver="${SING_VER:-}" url tmp bin

  case "$target" in
    windows) ext="zip"; exe="sing-box.exe" ;;
    linux)   ext="tar.gz" ;;
    *)
      echo "[*] ${SINGBOX_REPO} has no ${target} build; packaging without sing-box."
      return 0
      ;;
  esac

  [[ -n "$ver" ]] || ver="$(singbox_latest_version)" || true
  [[ -n "$ver" ]] || { echo "[!] Failed to resolve sing-box version" >&2; return 1; }

  url="https://github.com/${SINGBOX_REPO}/releases/download/v${ver}/sing-box-${ver}-${target}-${goarch}.${ext}"
  echo "[+] Download sing-box: $url"

  tmp="$(mktemp -d)"
  curl -fL "$url" -o "$tmp/sing-box.${ext}"
  if [[ "$ext" == "zip" ]]; then
    unzip -q "$tmp/sing-box.${ext}" -d "$tmp/x"
  else
    mkdir -p "$tmp/x"
    tar -C "$tmp/x" -xzf "$tmp/sing-box.${ext}"
  fi

  bin="$(find "$tmp/x" -type f -name "$exe" | head -n1 || true)"
  [[ -n "$bin" ]] || { echo "[!] $exe not found in archive" >&2; rm -rf "$tmp"; return 1; }
  install -m 755 "$bin" "$bin_dir/sing_box/$exe"

  # Needed by the naive outbound (purego build loads it next to the binary).
  find "$tmp/x" -type f \( -name 'libcronet.dll' -o -name 'libcronet*.so*' \) -exec install -m 644 {} "$bin_dir/sing_box/" \;
  rm -rf "$tmp"
}

download_geo_assets() {
  local f
  echo "[+] Download geo files to ${bin_dir}"
  curl -fsSL -o "$bin_dir/geosite.dat" "https://github.com/Loyalsoldier/V2ray-rules-dat/releases/latest/download/geosite.dat"
  curl -fsSL -o "$bin_dir/geoip.dat" "https://github.com/Loyalsoldier/V2ray-rules-dat/releases/latest/download/geoip.dat"
  curl -fsSL -o "$bin_dir/geoip-only-cn-private.dat" "https://raw.githubusercontent.com/Loyalsoldier/geoip/release/geoip-only-cn-private.dat"
  curl -fsSL -o "$bin_dir/Country.mmdb" "https://raw.githubusercontent.com/Loyalsoldier/geoip/release/Country.mmdb"
  curl -fsSL -o "$bin_dir/geoip.metadb" "https://github.com/MetaCubeX/meta-rules-dat/releases/latest/download/geoip.metadb"

  echo "[+] Download sing-box rule-sets to ${srss_dir}"
  for f in geoip-private.srs geoip-cn.srs geoip-facebook.srs geoip-fastly.srs geoip-google.srs geoip-netflix.srs geoip-telegram.srs geoip-twitter.srs; do
    curl -fsSL -o "$srss_dir/$f" "https://raw.githubusercontent.com/2dust/sing-box-rules/refs/heads/rule-set-geoip/$f"
  done
  for f in geosite-cn.srs geosite-gfw.srs geosite-google.srs geosite-greatfire.srs geosite-geolocation-cn.srs geosite-category-ads-all.srs geosite-private.srs; do
    curl -fsSL -o "$srss_dir/$f" "https://raw.githubusercontent.com/2dust/sing-box-rules/refs/heads/rule-set-geosite/$f"
  done
}

if [[ "${WITH_CORE:-sing-box}" != "none" ]]; then
  download_singbox
fi
download_geo_assets
