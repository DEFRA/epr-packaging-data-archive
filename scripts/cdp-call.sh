#!/usr/bin/env bash
# Call this service on CDP through the ephemeral protected endpoint.
#
# Protected-zone backends are not reachable directly. A Developer API key, generated per user in the
# CDP Portal, is the supported way in for local debugging.
#   https://portal.cdp-int.defra.cloud/user-profile#developer-api-key
#
# The key is read from the environment and never written to disk by this script. Keys last 24 hours
# in the lower environments, 2 hours in prod (break glass only).
#
#   export CDP_API_KEY='...'
#   ./scripts/cdp-call.sh /health
#   ./scripts/cdp-call.sh /v1/organisations/100123
#   CDP_ENV=test ./scripts/cdp-call.sh /openapi/v1.json
set -euo pipefail

SERVICE="epr-packaging-data-archive"
CDP_ENV="${CDP_ENV:-dev}"
ENDPOINT="${1:-/health}"

if [[ -z "${CDP_API_KEY:-}" ]]; then
  echo "CDP_API_KEY is not set." >&2
  echo "Generate one at https://portal.cdp-int.defra.cloud/user-profile#developer-api-key" >&2
  echo "then: export CDP_API_KEY='...'" >&2
  exit 1
fi

BASE="https://ephemeral-protected.api.${CDP_ENV}.cdp-int.defra.cloud/${SERVICE}"
URL="${BASE}${ENDPOINT}"

echo "GET ${URL}" >&2
curl --silent --show-error --fail-with-body \
     --header "x-api-key: ${CDP_API_KEY}" \
     --header "Content-Type: application/json" \
     --write-out '\n[HTTP %{http_code} in %{time_total}s]\n' \
     "${URL}"
