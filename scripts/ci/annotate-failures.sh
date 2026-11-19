# Sourced by the CI test scripts. run_annotated runs a test command and, when it fails, turns the
# failing tests into GitHub ::error annotations. Annotations are readable without signing in (the
# run summary and the check-runs API), unlike job logs and artifacts.

# GitHub keeps at most 10 error annotations per step: one summary plus this many tests.
_ANNOTATE_MAX_TESTS=9

_annotate_escape() {
	# Workflow-command data must encode % and line breaks. CRs are dropped with tr: BSD sed (macOS)
	# has no \r escape.
	tr -d '\r' | sed -e 's/%/%25/g' | awk 'BEGIN { ORS = "%0A" } { print }'
}

_annotate_failures() {
	local label="$1" log="$2" rc="$3"
	local count
	count=$(grep -c '\[FAIL\]' "$log" || true)

	if [ "$count" -gt 0 ]; then
		echo "::error title=${label}::${count} test(s) failed (exit ${rc}). First ${_ANNOTATE_MAX_TESTS} are annotated below."
		grep -n '\[FAIL\]' "$log" | head -n "$_ANNOTATE_MAX_TESTS" | while IFS=: read -r n line; do
			name=$(printf '%s' "$line" | sed -e 's/\[xUnit\.net [^]]*\]//' -e 's/\[FAIL\]//' -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//')
			# The assertion message follows the [FAIL] line; keep it short.
			detail=$(sed -n "$((n + 1)),$((n + 8))p" "$log" \
				| awk '/Stack Trace:|\[(STARTING|FINISHED|FAIL|PASS)\]/ { exit } { print }' \
				| sed -e 's/^[[:space:]]*//' | _annotate_escape)
			echo "::error title=${label}: ${name}::${detail}"
		done
		return
	fi

	# No [FAIL] line: the runner itself died (e.g. a native GTK crash). Name the last test started.
	local last tail_out
	last=$(grep 'STARTING' "$log" | tail -n 1 | sed -e 's/^[[:space:]]*//')
	tail_out=$(tail -n 15 "$log" | _annotate_escape)
	echo "::error title=${label}::Exit ${rc} with no [FAIL] lines. Last test started: ${last:-unknown}%0A%0ALast output:%0A${tail_out}"
}

# Usage: run_annotated <label> <command> [args...]. Use `env VAR=value cmd` for per-command variables.
run_annotated() {
	local label="$1"
	shift
	local log rc
	log="$(mktemp)"
	set +e
	"$@" 2>&1 | tee "$log"
	rc=${PIPESTATUS[0]}
	set -e
	if [ "$rc" -ne 0 ]; then
		_annotate_failures "$label" "$log" "$rc"
	fi
	rm -f "$log"
	return "$rc"
}
