#!/usr/bin/env bash

set -euo pipefail

base_url="${1:-http://localhost:5229}"
request_count="${2:-11}"

if (( request_count < 11 )); then
  echo "At least 11 requests are required to verify the 10-per-minute limit." >&2
  exit 2
fi

if ! curl --fail --silent --show-error "${base_url}/health" >/dev/null; then
  echo "The Notification Forwarder is not available at ${base_url}. Start it before running this script." >&2
  exit 1
fi

results_directory="$(mktemp -d)"
trap 'rm -rf "${results_directory}"' EXIT

echo "Sending ${request_count} warning notifications concurrently to ${base_url}/notifications..."

for request_number in $(seq 1 "${request_count}"); do
  (
    payload=$(printf '{"level":"warning","title":"Rate-limit test %s","message":"Automated rate-limit verification.","source":"rate-limit-script"}' "${request_number}")
    status_code=$(curl --silent --output "${results_directory}/${request_number}.json" --write-out '%{http_code}' \
      --connect-timeout 5 \
      --max-time 45 \
      --header 'Content-Type: application/json' \
      --data "${payload}" \
      "${base_url}/notifications")
    printf '%s %s\n' "${request_number}" "${status_code}" >"${results_directory}/${request_number}.status"
  ) &
done

wait

rate_limited=$(awk '$2 == 429 { count++ } END { print count + 0 }' "${results_directory}"/*.status)
expected_rate_limited=$((request_count - 10))

printf '\nRequest results:\n'
sort -n "${results_directory}"/*.status
printf '\nHTTP 429 responses: %s (expected: %s)\n' "${rate_limited}" "${expected_rate_limited}"

if (( rate_limited == expected_rate_limited )); then
  echo "PASS: the shared 10-per-minute limit is enforced."
  exit 0
fi

echo "FAIL: expected ${expected_rate_limited} rate-limited request(s)." >&2
exit 1
