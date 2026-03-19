#!/usr/bin/env bash
#
# Verify iOS release: pack SDK locally, build VerifyApp with PackageReference (like partner apps),
# run on simulator, check for crashes.
#
# Use before publishing to catch NuGet package issues (e.g. missing mParticleRoktBinding,
# MPAppDelegateProxy marshalling) that don't appear with ProjectReference/SampleApp.
#
# Usage:
#   ./scripts/verify-ios-release.sh [APP_PATH]
#
# Default APP_PATH: Kits/rokt/VerifyApp
#
# Uses a simulator with iOS 17/18 (not 26) to avoid Swift Observation crashes.
# Override: IOS_SIMULATOR_UDID="<udid>" ./scripts/verify-ios-release.sh
#
# Expected: VerifyApp may crash until fixes (framework, ProxyAppDelegate) are applied.
# Success = script completes without detecting new crash reports.
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SDK_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
LOCAL_NUGET="${LOCAL_NUGET:-${SDK_ROOT}/LocalNuGet}"
RUN_TIMEOUT="${RUN_TIMEOUT:-25}"
VERIFY_APP_PATH="${1:-${SDK_ROOT}/Kits/rokt/VerifyApp}"

# Version from VERSION file or VerifyApp PackageReference
if [[ -f "${SDK_ROOT}/VERSION" ]]; then
	VERSION="${VERSION:-$(cat "${SDK_ROOT}/VERSION")}"
else
	VERSION="${VERSION:-4.1.0}"
fi
if [[ -f "${VERIFY_APP_PATH}/VerifyApp.csproj" ]]; then
	# shellcheck disable=SC2312
	REF_VERSION=$(grep -E 'mParticle\.(MAUI|Maui\.Kits\.Rokt)' "${VERIFY_APP_PATH}/VerifyApp.csproj" | head -1 | sed -n 's/.*Version="\([^"]*\)".*/\1/p')
	[[ -n ${REF_VERSION} ]] && VERSION="${REF_VERSION}"
fi

SDK_PATH="${SDK_ROOT}/Sdk/MParticle.Maui.Sdk"
ROKT_KIT_PATH="${SDK_ROOT}/Kits/rokt/Sdk/MParticle.Maui.Rokt"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# shellcheck disable=SC2317
log() { echo -e "${GREEN}[INFO]${NC} $*"; }
# shellcheck disable=SC2317
warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }
# shellcheck disable=SC2317
err() { echo -e "${RED}[ERROR]${NC} $*"; }

# --- 1. Build native iOS frameworks (required before pack) ---
log "Building native iOS frameworks..."
cd "${SDK_ROOT}"

log "Resolving SPM for mParticleBinding..."
cd "${SDK_PATH}/macios/native/mParticleBinding"
xcodebuild -resolvePackageDependencies -project mParticleBinding.xcodeproj -scheme mParticleBinding -quiet

log "Building mParticleBinding for iphoneos and iphonesimulator..."
xcodebuild -project mParticleBinding.xcodeproj -scheme mParticleBinding -configuration Release -sdk iphoneos build -quiet
xcodebuild -project mParticleBinding.xcodeproj -scheme mParticleBinding -configuration Release -sdk iphonesimulator build -quiet

log "Resolving SPM for mParticleRoktBinding..."
cd "${ROKT_KIT_PATH}/macios/native/mParticleRoktBinding"
xcodebuild -resolvePackageDependencies -project mParticleRoktBinding.xcodeproj -scheme mParticleRoktBinding -quiet

log "Building mParticleRoktBinding for iphoneos and iphonesimulator..."
xcodebuild -project mParticleRoktBinding.xcodeproj -scheme mParticleRoktBinding -configuration Release -sdk iphoneos build -quiet
xcodebuild -project mParticleRoktBinding.xcodeproj -scheme mParticleRoktBinding -configuration Release -sdk iphonesimulator build -quiet

# --- 2. Pack SDK to local feed ---
log "Packing SDK to ${LOCAL_NUGET} (version ${VERSION})"
cd "${SDK_ROOT}"
mkdir -p "${LOCAL_NUGET}"

dotnet pack "${SDK_PATH}/MParticle.Maui.Sdk.csproj" -c Release -o "${LOCAL_NUGET}" -p:Version="${VERSION}"
dotnet pack "${ROKT_KIT_PATH}/mParticle.Maui.Kits.Rokt.csproj" -c Release -o "${LOCAL_NUGET}" -p:Version="${VERSION}"

# shellcheck disable=SC2312
PKG_COUNT=$(find "${LOCAL_NUGET}" -maxdepth 1 -name "*.nupkg" -print 2>/dev/null | wc -l | tr -d ' ')
log "Packages: ${PKG_COUNT} .nupkg files"

# --- 3. Select and boot simulator (avoid iOS 26 - Swift Observation crashes) ---
# Find first iPhone from iOS 17/18 runtime (not 26)
if [[ -z ${IOS_SIMULATOR_UDID-} ]]; then
	log "Finding simulator with iOS 17/18 (avoiding iOS 26)..."
	DEVICES_RAW=$(xcrun simctl list devices available 2>/dev/null)
	log "Available runtimes:"
	echo "${DEVICES_RAW}" | grep -E "^-- " | head -5 | sed 's/^/  /' || true
	SIM_UDID=$(echo "${DEVICES_RAW}" | awk '
    /^-- iOS / {
      if ($0 !~ /26\./) { in_ok = 1 } else { in_ok = 0 }
      next
    }
    in_ok && /iPhone/ {
      if (match($0, /[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}/)) {
        print substr($0, RSTART, RLENGTH)
        exit
      }
    }
  ')
	if [[ -z ${SIM_UDID} ]]; then
		err "No iOS 17/18 simulator found. Install older runtime or set IOS_SIMULATOR_UDID."
		exit 1
	fi
	IOS_SIMULATOR_UDID="${SIM_UDID}"
	log "Selected simulator UDID: ${IOS_SIMULATOR_UDID}"
fi
# Shutdown any booted simulator, boot our target
xcrun simctl shutdown all 2>/dev/null || true
log "Booting simulator ${IOS_SIMULATOR_UDID}..."
xcrun simctl boot "${IOS_SIMULATOR_UDID}" 2>/dev/null || true
sleep 5

# --- 4. Configure app to use local feed ---
if [[ ! -d ${VERIFY_APP_PATH} ]] || [[ ! -f "${VERIFY_APP_PATH}/VerifyApp.csproj" ]]; then
	err "VerifyApp not found at ${VERIFY_APP_PATH}"
	err "Usage: $0 [APP_PATH]"
	exit 1
fi

NUGET_CONFIG="${VERIFY_APP_PATH}/nuget.config"
BACKUP_CONFIG=""
if [[ -f ${NUGET_CONFIG} ]]; then
	BACKUP_CONFIG="${NUGET_CONFIG}.bak.$$"
	cp "${NUGET_CONFIG}" "${BACKUP_CONFIG}"
fi

cat >"${NUGET_CONFIG}" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-sdk" value="${LOCAL_NUGET}" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="maui-nativelibraryinterop" value="https://pkgs.dev.azure.com/xamarin/public/_packaging/maui-nativelibraryinterop/nuget/v3/index.json" />
  </packageSources>
</configuration>
EOF

# shellcheck disable=SC2317
cleanup_nuget_config() {
	if [[ -n ${BACKUP_CONFIG} ]] && [[ -f ${BACKUP_CONFIG} ]]; then
		mv "${BACKUP_CONFIG}" "${NUGET_CONFIG}"
	elif [[ -f ${NUGET_CONFIG} ]]; then
		rm -f "${NUGET_CONFIG}"
	fi
}
# shellcheck disable=SC2317
trap cleanup_nuget_config EXIT

# --- 5. Clear NuGet cache and restore ---
log "Clearing NuGet cache to ensure fresh packages from LocalNuGet..."
dotnet nuget locals all --clear

log "Restoring and building VerifyApp..."
cd "${VERIFY_APP_PATH}"
TARGET_FW="net10.0-ios"
dotnet restore
dotnet build -f "${TARGET_FW}" -c Release --no-restore

# --- 6. Run app and check for crash ---
log "Launching app on simulator (timeout ${RUN_TIMEOUT}s)..."
CRASH_LOG_DIR="${HOME}/Library/Logs/DiagnosticReports"
# shellcheck disable=SC2312
CRASH_COUNT_BEFORE=$(find "${CRASH_LOG_DIR}" \( -name "*.ips" -o -name "*.crash" \) -mmin -1 2>/dev/null | wc -l | tr -d ' ')

# Run in background, capture PID (explicit UDID ensures we use the booted non-iOS26 simulator)
# shellcheck disable=SC2312
(dotnet build -t:Run -f "${TARGET_FW}" -c Release -p:_DeviceName=:v2:udid="${IOS_SIMULATOR_UDID}" 2>&1) &
RUN_PID=$!

# Wait with timeout
EXIT_CODE=0
elapsed=0
while kill -0 "${RUN_PID}" 2>/dev/null && [[ ${elapsed} -lt ${RUN_TIMEOUT} ]]; do
	sleep 2
	elapsed=$((elapsed + 2))
done

if kill -0 "${RUN_PID}" 2>/dev/null; then
	log "App ran for ${RUN_TIMEOUT}s without exiting - stopping..."
	kill "${RUN_PID}" 2>/dev/null || true
	wait "${RUN_PID}" 2>/dev/null || true
	EXIT_CODE=0
else
	set +e
	wait "${RUN_PID}" 2>/dev/null
	EXIT_CODE=$?
	set -e
fi

# --- 7. Check for crash reports ---
sleep 3
# shellcheck disable=SC2312
CRASH_COUNT_AFTER=$(find "${CRASH_LOG_DIR}" \( -name "*.ips" -o -name "*.crash" \) -mmin -3 2>/dev/null | wc -l | tr -d ' ')
NEW_CRASHES=$((CRASH_COUNT_AFTER - CRASH_COUNT_BEFORE))

if [[ ${EXIT_CODE} -ne 0 ]]; then
	err "App exited with code ${EXIT_CODE} (possible crash on startup)"
	exit 1
fi

if [[ ${NEW_CRASHES} -gt 0 ]]; then
	err "Detected ${NEW_CRASHES} new crash report(s)"
	find "${CRASH_LOG_DIR}" \( -name "*.ips" -o -name "*.crash" \) -mmin -3 -exec echo "  {}" \;
	exit 1
fi

log "Verification passed: app ran without crash"
exit 0
